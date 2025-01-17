﻿using Akiles.Api;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Controllers;

public class AlarmController(
    IAlarmGadget alarmGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var schedule = await GetScheduleAsync(stoppingToken);
        var scheduleObtained = timeProvider.GetLocalNow();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProvider.GetLocalNow().DateTime;
            if (scheduleObtained.Date != now.Date)
            {
                schedule = await GetScheduleAsync(stoppingToken);
                scheduleObtained = timeProvider.GetLocalNow();
            }

            var nextTick = await TickAsync(schedule, stoppingToken);
            if (nextTick is null)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);

                schedule = await GetScheduleAsync(stoppingToken);
                scheduleObtained = timeProvider.GetLocalNow();
                continue;
            }

            var earliest = DateTimeEx.Min(nextTick.Value, now.AddMinutes(10));
            var duration = earliest - timeProvider.GetLocalNow();
            await Task.Delay(duration, stoppingToken);
        }
    }

    public async Task<DateTime?> TickAsync(Schedule schedule, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow().DateTime;
        var currentRange = schedule.GetCurrentRange(now);
        if (currentRange is null)
        {
            if (alarmGadget.ArmState != AlarmGadgetArmState.Armed)
            {
                await alarmGadget.ArmAsync(client, cancellationToken);
            }

            var futureRange = schedule.GetRangePeriods(startNotBefore: now).FirstOrDefault();
            if (futureRange == default)
            {
                // The schedule is empty, i.e. it does not have any ranges
                return null;
            }

            return futureRange.Start;
        }
        else
        {
            if (alarmGadget.ArmState != AlarmGadgetArmState.Disarmed)
            {
                await alarmGadget.DisarmAsync(client, cancellationToken);
            }

            return DateOnly.FromDateTime(now.Date).ToDateTime(currentRange.End);
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
}
