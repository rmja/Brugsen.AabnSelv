using Akiles.Api;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public class FinalLockdownController(
    ILightGadget lightGadget,
    IFrontDoorLockGadget lockGadget,
    IAlarmGadget alarmGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<FinalLockdownController> logger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService
{
    private bool _firstTick = true;
    private DateTime? _lightTimeout;
    private DateTime? _lockAndAlarmTimeout;
    private static readonly TimeSpan LightDelay = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LockAndAlarmDelay = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var extendedSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.ExtendedOpeningHoursScheduleId,
            stoppingToken
        );
        var scheduleObtained = timeProvider.GetLocalNow();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProvider.GetLocalNow().DateTime;
            if (scheduleObtained.Date != now.Date)
            {
                extendedSchedule = await client.Schedules.GetScheduleAsync(
                    options.Value.ExtendedOpeningHoursScheduleId,
                    stoppingToken
                );
                scheduleObtained = timeProvider.GetLocalNow();
            }

            var wakeup = await TickAsync(extendedSchedule, stoppingToken);
            if (wakeup is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
                continue;
            }

            logger.LogInformation("Sleeping until {Wakeup}", wakeup);
            var duration = wakeup.Value - timeProvider.GetLocalNow();
            if (duration > TimeSpan.Zero)
            {
                await Task.Delay(duration, stoppingToken);
            }
        }
    }

    public async Task<DateTime?> TickAsync(
        Schedule extendedSchedule,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var currentPeriod = extendedSchedule.GetCurrentPeriod(now);

        if (_firstTick)
        {
            if (currentPeriod is null)
            {
                var previousEnd = extendedSchedule.GetEarlierPeriods(now).FirstOrDefault()?.End;
                if (previousEnd is not null)
                {
                    // Schedule final lockdown to after the previous end
                    _lightTimeout ??= previousEnd.Value.Add(LightDelay);
                    _lockAndAlarmTimeout ??= previousEnd.Value.Add(LockAndAlarmDelay);

                    logger.LogInformation(
                        EventIds.LockdownSchedule,
                        "Next final lockdown scheduled to {LightTimeout} and {LockAndAlarmTimeout}",
                        _lightTimeout,
                        _lockAndAlarmTimeout
                    );
                }
            }
            _firstTick = false;
        }

        // Process lockdown if scheduled
        if (now >= _lightTimeout)
        {
            logger.LogInformation(
                EventIds.LockdownSequence,
                "Turning off the light as part of final lockdown - was scheduled to {LightTimeout}",
                _lightTimeout
            );

            await lightGadget.TurnOffAsync(client, cancellationToken);
            _lightTimeout = null;
        }
        if (now >= _lockAndAlarmTimeout)
        {
            logger.LogInformation(
                EventIds.LockdownSequence,
                "Locking the door and arming the alarm as part of final lockdown - was scheduled to {LockAndAlarmTimeout}",
                _lockAndAlarmTimeout
            );

            await lockGadget.LockAsync(client, cancellationToken);
            await alarmGadget.ArmAsync(client, cancellationToken);
            _lockAndAlarmTimeout = null;
        }

        var nextEnd = currentPeriod is not null
            ? currentPeriod.End
            : extendedSchedule.GetLaterPeriods(startNotBefore: now).FirstOrDefault()?.End;
        if (nextEnd is not null)
        {
            // Schedule final lockdown to after the next end
            _lightTimeout ??= nextEnd.Value.Add(LightDelay);
            _lockAndAlarmTimeout ??= nextEnd.Value.Add(LockAndAlarmDelay);

            logger.LogInformation(
                EventIds.LockdownSchedule,
                "Next final lockdown scheduled to {LightTimeout} and {LockAndAlarmTimeout}",
                _lightTimeout,
                _lockAndAlarmTimeout
            );
        }

        if (_lightTimeout is not null && _lockAndAlarmTimeout is not null)
        {
            return DateTimeEx.Min(_lightTimeout.Value, _lockAndAlarmTimeout.Value);
        }
        else
        {
            return _lightTimeout ?? _lockAndAlarmTimeout;
        }
    }
}
