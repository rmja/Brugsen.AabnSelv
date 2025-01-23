using Akiles.Api;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Controllers;

public sealed class AccessController(
    IAccessGadget access,
    IAlarmGadget alarm,
    ILightGadget light,
    IFrontDoorLockGadget doorLock,
    IFrontDoorGadget door,
    TimeProvider timeProvider,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    ILogger<AccessController> logger
) : BackgroundService, IAccessController
{
    private ITimer? _blackoutTimer;
    private ITimer? _lockdownTimer;
    private bool _blackoutSignalled = false;
    private bool _lockdownSignalled = false;
    private readonly SemaphoreSlim _signal = new(0);

    public IAccessGadget AccessGadget => access;
    public TimeSpan BlackoutDelay { get; } = TimeSpan.FromSeconds(30);
    public TimeSpan LockdownDelay { get; } = TimeSpan.FromSeconds(60);

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _blackoutTimer = timeProvider.CreateTimer(
            SignalBlackout,
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan
        );
        _lockdownTimer = timeProvider.CreateTimer(
            SignalLockdown,
            null,
            Timeout.InfiniteTimeSpan,
            Timeout.InfiniteTimeSpan
        );
        return base.StartAsync(cancellationToken);
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
            await _signal.WaitAsync(stoppingToken);

            if (_blackoutSignalled)
            {
                _blackoutSignalled = false;
                if (light.State != LightState.Off)
                {
                    await light.TurnOffAsync(client, CancellationToken.None);
                }
            }

            if (_lockdownSignalled)
            {
                _lockdownSignalled = false;
                if (alarm.State != AlarmState.Armed)
                {
                    await alarm.ArmAsync(client, CancellationToken.None);
                }
            }
        }
    }

    public async Task ProcessCheckInAsync(string eventId, string memberId)
    {
        logger.LogInformation(
            "Processing check-in event {EventId} for member {MemberId}",
            eventId,
            memberId
        );

        // Disarm timers
        _blackoutTimer!.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _lockdownTimer!.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _blackoutSignalled = false;
        _lockdownSignalled = false;

        if (alarm.State != AlarmState.Disarmed)
        {
            await alarm.DisarmAsync(client, CancellationToken.None);
        }

        if (light.State != LightState.On)
        {
            await light.TurnOnAsync(client, CancellationToken.None);
        }

        if (doorLock.State != LockState.Unlocked)
        {
            await doorLock.UnlockAsync(client, CancellationToken.None);

            // Wait for the lock to perform the unlock operation before we try and open the door
            await Task.Delay(doorLock.UnlockOperationDuration, timeProvider);
        }

        await door.OpenOnceAsync(client, CancellationToken.None);
    }

    public async Task ProcessCheckOutAsync(string eventId, string memberId)
    {
        logger.LogInformation(
            "Processing check-out event {EventId} for member {MemberId}",
            eventId,
            memberId
        );

        var now = timeProvider.GetLocalNow();

        // Ensure that we are checked-in, otherwise we might open the door without turning off the alarm.
        // We are actually checked out at this point, as this processing happens after the "check-out" event.
        // We therefore explicity ignore the current check-out event when determining if we are currently checked in.
        var memberIsCheckedIn = await access.IsMemberCheckedInAsync(
            client,
            memberId,
            notBefore: now.AddHours(-1),
            ignoreEventId: eventId
        );
        if (!memberIsCheckedIn)
        {
            var activity = await access.GetActivityAsync(
                client,
                memberId,
                notBefore: now.AddHours(-1)
            );
            var recentActivity = activity.FirstOrDefault();

            logger.LogWarning(
                "Member {MemberId} tried to check-out but was not checked in. Ignored event {IgnoreEventId} ({CheckOutEventIgnored}). Most recent check-in/check-out: {RecentCheckIn} / {RecentCheckOut}",
                memberId,
                eventId,
                recentActivity?.CheckOutEvent?.Id == eventId,
                recentActivity?.CheckInEvent?.CreatedAt,
                recentActivity?.CheckOutEvent?.CreatedAt
            );
            return;
        }

        logger.LogInformation("Checking-out member {MemberId}", memberId);

        await door.OpenOnceAsync(client, CancellationToken.None);

        var anyCheckedIn = await access.IsAnyCheckedInAsync(
            client,
            notBefore: timeProvider.GetLocalNow().AddHours(-1),
            CancellationToken.None
        );
        if (anyCheckedIn)
        {
            logger.LogInformation("There are other members checked-in - keep the lights on");
        }
        else
        {
            logger.LogInformation(
                "Member {MemberId} is the last checked-in - turn off the light",
                memberId
            );

            _blackoutTimer!.Change(BlackoutDelay, Timeout.InfiniteTimeSpan);
            _lockdownTimer!.Change(LockdownDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private void SignalBlackout(object? state)
    {
        _blackoutSignalled = true;
        _signal.Release();
    }

    private void SignalLockdown(object? state)
    {
        _lockdownSignalled = true;
        _signal.Release();
    }

    public override void Dispose()
    {
        base.Dispose();
        _blackoutTimer?.Dispose();
        _lockdownTimer?.Dispose();
    }
}
