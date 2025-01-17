using Akiles.Api;
using Akiles.Api.Events;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Controllers;

public class LightController(
    ILightGadget lightGadget,
    IFrontDoorGadget frontDoorGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var storeSchedule = await GetStoreRegularOpeningHoursScheduleAsync(stoppingToken);
        var storeScheduleObtained = timeProvider.GetLocalNow();

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5), timeProvider);
        do
        {
            var now = timeProvider.GetLocalNow().DateTime;
            if (storeScheduleObtained.Date != now.Date)
            {
                storeSchedule = await GetStoreRegularOpeningHoursScheduleAsync(stoppingToken);
                storeScheduleObtained = timeProvider.GetLocalNow();
            }

            await TickAsync(storeSchedule, stoppingToken);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task TickAsync(Schedule storeSchedule, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow();
        var recentEvents = await frontDoorGadget
            .GetRecentEventsAsync(
                client,
                notBefore: now.AddHours(-1),
                EventsExpand.None,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        // Change event order to ascending
        recentEvents.Reverse();

        var anyEntries = false;
        var membersInStore = new HashSet<string>();
        foreach (var evnt in recentEvents)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case FrontDoorGadget.Actions.OpenEntry:
                    anyEntries = true;
                    membersInStore.Add(evnt.Object.MemberId!);
                    break;
                case FrontDoorGadget.Actions.OpenExit:
                    membersInStore.Remove(evnt.Object.MemberId!);
                    break;
            }
        }

        if (anyEntries && membersInStore.Count == 0)
        {
            var currentRange = storeSchedule.GetCurrentRange(now.DateTime);
            if (currentRange is null)
            {
                // Only turn the light off if outside regular opening hours
                // If there is no schedule range for "now", then we are outside opening hours

                await lightGadget.TurnOffAsync(client, cancellationToken);
            }
        }
    }

    private Task<Schedule> GetStoreRegularOpeningHoursScheduleAsync(
        CancellationToken cancellationToken
    )
    {
        var schedule = new Schedule { OrganizationId = "", Name = "Butikkens åbningstider" };

        schedule
            .Weekdays[DayOfWeek.Monday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Tuesday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Wednesday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(17, 30)));
        schedule
            .Weekdays[DayOfWeek.Thursday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(19, 00)));
        schedule
            .Weekdays[DayOfWeek.Friday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(19, 00)));
        schedule
            .Weekdays[DayOfWeek.Saturday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(13, 30)));
        schedule
            .Weekdays[DayOfWeek.Sunday]
            .Ranges.Add(new(new TimeOnly(08, 00), new TimeOnly(13, 30)));
        return Task.FromResult(schedule);
    }
}
