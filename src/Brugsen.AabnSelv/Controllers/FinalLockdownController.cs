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
    private DateTime? _lightTimeout;
    private DateTime? _lockAndAlarmTimeout;

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

        // Process lockdown if scheduled
        if (now >= _lightTimeout)
        {
            logger.LogInformation(
                EventIds.LockdownSequence,
                "Turning off the light as part of final lockdown"
            );

            await lightGadget.TurnOffAsync(client, cancellationToken);
            _lightTimeout = null;
        }
        if (now >= _lockAndAlarmTimeout)
        {
            logger.LogInformation(
                EventIds.LockdownSequence,
                "Locking the door and arming the alarm as part of final lockdown"
            );

            await lockGadget.LockAsync(client, cancellationToken);
            await alarmGadget.ArmAsync(client, cancellationToken);
            _lockAndAlarmTimeout = null;
        }

        var currentPeriod = extendedSchedule.GetCurrentPeriod(now);
        var nextEnd = currentPeriod is not null
            ? currentPeriod.End
            : extendedSchedule.GetPeriods(startNotBefore: now).FirstOrDefault()?.End;
        if (nextEnd is not null)
        {
            // Schedule final lockdown to after the next end
            _lightTimeout ??= nextEnd.Value.AddMinutes(10);
            _lockAndAlarmTimeout ??= nextEnd.Value.AddMinutes(15);

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
