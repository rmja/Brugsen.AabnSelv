using Akiles.Api;
using Akiles.Api.Schedules;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public class AlarmController(
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<AlarmController> logger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService
{
    private string _alarmGadgetId = null!;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options.Value.AlarmGadgetId is null)
        {
            logger.LogWarning("Alarm controller is disabled: No alarm gadget configured");
            return;
        }
        _alarmGadgetId = options.Value.AlarmGadgetId;

        var schedule = await GetScheduleAsync(stoppingToken);
        var scheduleObtained = timeProvider.GetLocalNow();

        var state = await GetAlarmStateAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProvider.GetLocalNow().DateTime;
            if (scheduleObtained.Date != now.Date)
            {
                schedule = await GetScheduleAsync(stoppingToken);
                scheduleObtained = timeProvider.GetLocalNow();
            }

            var currentRange = schedule.GetCurrentRange(now);
            if (currentRange is null)
            {
                if (state != AlarmState.Armed)
                {
                    await ArmAsync(stoppingToken);
                    state = AlarmState.Armed;
                }
            }
            else
            {
                if (state != AlarmState.Disarmed)
                {
                    await DisarmAsync(stoppingToken);
                    state = AlarmState.Disarmed;
                }
            }

            var futureRange = schedule.GetRangePeriods(startNotBefore: now).FirstOrDefault();
            if (futureRange == default)
            {
                // The schedule is empty, i.e. it does not have any ranges

                schedule = await GetScheduleAsync(stoppingToken);
                scheduleObtained = timeProvider.GetLocalNow();
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
            else
            {
                var earliest = DateTimeEx.Min(futureRange.Start, now.AddMinutes(10));
                var duration = earliest - timeProvider.GetLocalNow();
                await Task.Delay(duration, stoppingToken);
            }
        }
    }

    private Task<Schedule> GetScheduleAsync(CancellationToken cancellationToken)
    {
        var schedule = new Schedule { OrganizationId = "", Name = "Alarm Frakoblet" };

        foreach (var weekday in Enum.GetValues<DayOfWeek>())
        {
            schedule.Weekdays[weekday].Ranges.Add(new(new TimeOnly(04, 50), new TimeOnly(23, 10)));
        }
        return Task.FromResult(schedule);
    }

    private async Task<AlarmState> GetAlarmStateAsync(CancellationToken cancellationToken)
    {
        var gadget = await client.Gadgets.GetGadgetAsync(_alarmGadgetId, cancellationToken);
        var value = gadget.Metadata.GetValueOrDefault(MetadataKeys.Gadget.AlarmState);
        var state = value switch
        {
            "armed" => AlarmState.Armed,
            "disarmed" => AlarmState.Disarmed,
            _ => AlarmState.Unknown
        };

        logger.LogInformation("Found alarm to be in {AlarmState} state", state);
        return state;
    }

    private async Task ArmAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Arming alarm");

        await client.Gadgets.DoGadgetActionAsync(
            _alarmGadgetId,
            GadgetActions.AlarmArm,
            cancellationToken
        );
        await client.Gadgets.EditGadgetAsync(
            _alarmGadgetId,
            new() { Metadata = new() { [MetadataKeys.Gadget.AlarmState] = "armed" } },
            CancellationToken.None
        );
    }

    private async Task DisarmAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Disarming alarm");

        await client.Gadgets.DoGadgetActionAsync(
            _alarmGadgetId,
            GadgetActions.AlarmDisarm,
            cancellationToken
        );
        await client.Gadgets.EditGadgetAsync(
            _alarmGadgetId,
            new() { Metadata = new() { [MetadataKeys.Gadget.AlarmState] = "disarmed" } },
            CancellationToken.None
        );
    }

    enum AlarmState
    {
        Unknown,
        Armed,
        Disarmed,
    }
}
