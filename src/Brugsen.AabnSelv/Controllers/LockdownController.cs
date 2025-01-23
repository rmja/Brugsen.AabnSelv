using Akiles.Api;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public sealed class LockdownController(
    ILightGadget lightGadget,
    IFrontDoorLockGadget lockGadget,
    IAlarmGadget alarmGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<LockdownController> logger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService, IDisposable
{
    private Schedule _schedule = null!;
    private ITimer? _blackoutTimer;
    private ITimer? _lockdownTimer;
    private bool _blackoutSignalled = false;
    private bool _lockdownSignalled = false;
    private readonly SemaphoreSlim _signal = new(0);

    public TimeSpan BlackoutDelay { get; } = TimeSpan.FromMinutes(5);
    public TimeSpan LockdownDelay { get; } = TimeSpan.FromMinutes(10);

    public DateTimeOffset? BlackoutAt { get; private set; }
    public DateTimeOffset? LockdownAt { get; private set; }

    public Schedule Schedule
    {
        get => _schedule;
        set
        {
            _schedule = value;
            ScheduleAssigned = timeProvider.GetUtcNow();
        }
    }

    public DateTimeOffset ScheduleAssigned { get; private set; }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await RefreshScheduleAsync(cancellationToken);

        var fireTimes = GetFireTimes(allowPastFireTimes: true);

        BlackoutAt = fireTimes?.BlackoutAt;
        var blackoutDue = BlackoutAt.HasValue
            ? GetDurationUntil(BlackoutAt.Value)
            : Timeout.InfiniteTimeSpan;
        _blackoutTimer = timeProvider.CreateTimer(
            SignalBlackout,
            state: null,
            blackoutDue,
            Timeout.InfiniteTimeSpan
        );

        LockdownAt = fireTimes?.LockdownAt;
        var lockdownDue = LockdownAt.HasValue
            ? GetDurationUntil(LockdownAt.Value)
            : Timeout.InfiniteTimeSpan;
        _lockdownTimer = timeProvider.CreateTimer(
            SignalLockdown,
            state: null,
            lockdownDue,
            Timeout.InfiniteTimeSpan
        );

        logger.LogInformation(
            EventIds.LockdownSchedule,
            "Initial blackout scheduled at {BlackoutAt} (due in {BlackoutDue} and lockout scheduled at {LockdownAt} (due in {LockdownDue})",
            BlackoutAt,
            blackoutDue,
            LockdownAt,
            lockdownDue
        );

        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _blackoutTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _lockdownTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait until signal - with some timeout so that we occasionally refresh the schedule
            await _signal.WaitAsync(TimeSpan.FromHours(1), stoppingToken);

            if (_blackoutSignalled)
            {
                _blackoutSignalled = false;
                await PerformBlackoutAsync(stoppingToken);
            }
            if (_lockdownSignalled)
            {
                _lockdownSignalled = false;
                await PerformLockdownAsync(stoppingToken);
            }

            await RefreshScheduleAsync(stoppingToken);

            if (!BlackoutAt.HasValue)
            {
                var blackoutAt = GetFireTimes()?.BlackoutAt;
                if (blackoutAt.HasValue)
                {
                    var blackoutDue = GetDurationUntil(blackoutAt.Value);
                    lock (this)
                    {
                        _blackoutTimer!.Change(blackoutDue, Timeout.InfiniteTimeSpan);
                        BlackoutAt = blackoutAt;
                    }

                    logger.LogInformation(
                        EventIds.LockdownSchedule,
                        "Scheduled blackout at {BlackoutAt} (due in {BlackoutDue})",
                        blackoutAt,
                        blackoutDue
                    );
                }
            }
            if (!LockdownAt.HasValue)
            {
                var lockdownAt = GetFireTimes()?.LockdownAt;
                if (lockdownAt.HasValue)
                {
                    var lockdownDue = GetDurationUntil(lockdownAt.Value);
                    lock (this)
                    {
                        _lockdownTimer!.Change(lockdownDue, Timeout.InfiniteTimeSpan);
                        LockdownAt = lockdownAt;
                    }

                    logger.LogInformation(
                        EventIds.LockdownSchedule,
                        "Scheduled lockdown at {LockdownAt} (due in {LockdownDue})",
                        lockdownAt,
                        lockdownDue
                    );
                }
            }
        }
    }

    private async Task RefreshScheduleAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow();
        if (ScheduleAssigned.Date != now.Date)
        {
            Schedule = await client.Schedules.GetScheduleAsync(
                options.Value.ExtendedOpeningHoursScheduleId,
                cancellationToken
            );

            logger.LogInformation("Schedule {ScheduleName} refreshed", Schedule.Name);
        }
    }

    private (DateTimeOffset BlackoutAt, DateTimeOffset LockdownAt)? GetFireTimes(
        bool allowPastFireTimes = false
    )
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var currentPeriod = Schedule.GetCurrentPeriod(now);
        if (currentPeriod is null && allowPastFireTimes)
        {
            var pastEnd = Schedule.GetEarlierPeriods(now).FirstOrDefault()?.End;
            if (pastEnd is not null)
            {
                var end = timeProvider.GetDateTimeOffset(pastEnd.Value);
                var blackoutAt = end.Add(BlackoutDelay);
                var lockdownAt = end.Add(LockdownDelay);
                return (blackoutAt, lockdownAt);
            }
        }
        else
        {
            var futureEnd = currentPeriod is not null
                ? currentPeriod.End
                : Schedule.GetLaterPeriods(startNotBefore: now).FirstOrDefault()?.End;
            if (futureEnd is not null)
            {
                var end = timeProvider.GetDateTimeOffset(futureEnd.Value);
                var blackoutAt = end.Add(BlackoutDelay);
                var lockdownAt = end.Add(LockdownDelay);
                return (blackoutAt, lockdownAt);
            }
        }

        // Not currently in an active period, and there are no future periods
        return null;
    }

    private TimeSpan GetDurationUntil(DateTimeOffset time)
    {
        var duration = time - timeProvider.GetLocalNow();
        return duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
    }

    private async Task PerformBlackoutAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            EventIds.LockdownSequence,
            "Turning off the light as part of blackout"
        );

        await lightGadget.TurnOffAsync(client, cancellationToken);
    }

    private async Task PerformLockdownAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation(
            EventIds.LockdownSequence,
            "Locking the door and arming the alarm as part of lockdown"
        );

        await lockGadget.LockAsync(client, cancellationToken);
        await alarmGadget.ArmAsync(client, cancellationToken);
    }

    private void SignalBlackout(object? state)
    {
        // Disarm
        lock (this)
        {
            _blackoutTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            BlackoutAt = null;
        }

        // Signal
        _blackoutSignalled = true;
        _signal.Release();
    }

    private void SignalLockdown(object? state)
    {
        // Disarm
        lock (this)
        {
            _lockdownTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            LockdownAt = null;
        }

        // Signal
        _lockdownSignalled = true;
        _signal.Release();
    }

    public override void Dispose()
    {
        base.Dispose();
        _blackoutTimer?.Dispose();
        _lockdownTimer?.Dispose();
        _signal.Dispose();
    }
}
