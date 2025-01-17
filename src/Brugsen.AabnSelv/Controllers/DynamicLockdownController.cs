using Akiles.Api;
using Akiles.Api.Events;
using Akiles.Api.Schedules;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Controllers;

public class DynamicLockdownController(
    IFrontDoorGadget frontDoorGadget,
    ILightGadget lightGadget,
    IFrontDoorLockGadget lockGadget,
    IAlarmGadget alarmGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<DynamicLockdownController> logger,
    IOptions<BrugsenAabnSelvOptions> options
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var regularSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.RegularOpeningHoursScheduleId,
            stoppingToken
        );
        var extendedSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.ExtendedOpeningHoursScheduleId,
            stoppingToken
        );
        var schedulesObtained = timeProvider.GetLocalNow();

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = timeProvider.GetLocalNow().DateTime;
            if (schedulesObtained.Date != now.Date)
            {
                regularSchedule = await client.Schedules.GetScheduleAsync(
                    options.Value.RegularOpeningHoursScheduleId,
                    stoppingToken
                );
                extendedSchedule = await client.Schedules.GetScheduleAsync(
                    options.Value.ExtendedOpeningHoursScheduleId,
                    stoppingToken
                );
                schedulesObtained = timeProvider.GetLocalNow();
            }

            var wakeup = await TickAsync(regularSchedule, extendedSchedule, stoppingToken);
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
        Schedule regularSchedule,
        Schedule extendedSchedule,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetLocalNow().DateTime;

        var regularPeriod = regularSchedule.GetCurrentPeriod(now);
        if (regularPeriod is not null)
        {
            // The store is open during regular hours
            // Sleep at least until the store closes
            return regularPeriod.End;
        }

        var currentRange = extendedSchedule.GetCurrentRange(now);
        if (currentRange is null)
        {
            // Extended access not granted - nothing to do at the moment

            var futureRegularPeriod = regularSchedule
                .GetPeriods(startNotBefore: now)
                .FirstOrDefault();
            var futureExtendedPeriod = extendedSchedule
                .GetPeriods(startNotBefore: now)
                .FirstOrDefault();

            // Sleep until the earliest next open start time

            if (futureRegularPeriod is not null && futureExtendedPeriod is not null)
            {
                return DateTimeEx.Min(futureRegularPeriod.Start, futureExtendedPeriod.Start);
            }
            else
            {
                return futureRegularPeriod?.Start ?? futureExtendedPeriod?.Start;
            }
        }

        await TryLockdownAsync(cancellationToken);

        return null;
    }

    private async Task TryLockdownAsync(CancellationToken cancellationToken)
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
            await lightGadget.TurnOffAsync(client, cancellationToken);
            await lockGadget.LockAsync(client, cancellationToken);
            await alarmGadget.ArmAsync(client, cancellationToken);
        }
    }
}
