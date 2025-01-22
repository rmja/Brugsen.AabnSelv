using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class AccessGadget(
    string gadgetId,
    IAlarmGadget alarm,
    ILightGadget light,
    IFrontDoorLockGadget doorLock,
    IFrontDoorGadget door,
    TimeProvider timeProvider,
    ILogger<AccessGadget> logger
) : IAccessGadget
{
    public string GadgetId { get; } = gadgetId;

    public Task CheckInAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger.LogInformation("Check-in");
        return client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.CheckIn, cancellationToken);
    }

    public Task CheckOutAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger.LogInformation("Check-out");
        return client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.CheckOut, cancellationToken);
    }

    public async Task ProcessCheckInAsync(IAkilesApiClient client, string eventId, string memberId)
    {
        logger.LogInformation("Processing check-in member {MemberId}", memberId);

        // These may have no effect if there are already customers inside
        await alarm.DisarmAsync(client, CancellationToken.None);
        await light.TurnOnAsync(client, CancellationToken.None);
        await doorLock.UnlockAsync(client, CancellationToken.None);

        // This has always effect
        await door.OpenOnceAsync(client, CancellationToken.None);
    }

    public async Task ProcessCheckOutAsync(IAkilesApiClient client, string eventId, string memberId)
    {
        logger.LogInformation("Processing check-out for member {MemberId}", memberId);

        var now = timeProvider.GetLocalNow();

        // Ensure that we are checked-in, otherwise we might open the door without turning off the alarm.
        // We are actually checked out at this point, as this processing happens after the "check-out" event.
        // We therefore explicity ignore the current check-out event when determining if we are currently checked in.
        var memberIsCheckedIn = await IsMemberCheckedInAsync(
            client,
            memberId,
            notBefore: now.AddHours(-1),
            ignoreEventId: eventId
        );
        if (!memberIsCheckedIn)
        {
            var activity = await GetActivityAsync(client, memberId, notBefore: now.AddHours(-1));
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

        var anyCheckedIn = await IsAnyCheckedInAsync(
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

            await light.TurnOffAsync(client, CancellationToken.None);
        }

        // Lockdown is controlled by the DynamicLockdownController
    }

    public async Task<bool> IsMemberCheckedInAsync(
        IAkilesApiClient client,
        string memberId,
        DateTimeOffset notBefore,
        string? ignoreEventId = null,
        CancellationToken cancellationToken = default
    )
    {
        var activity = await GetActivityAsync(
            client,
            memberId,
            notBefore,
            EventsExpand.None,
            cancellationToken
        );
        var lastActivityWithCheckIn = activity.FirstOrDefault(x => x.CheckInEvent is not null);
        if (lastActivityWithCheckIn is null)
        {
            // No activity with check-in found
            return false;
        }

        return lastActivityWithCheckIn.CheckOutEvent is null
            || lastActivityWithCheckIn.CheckOutEvent.Id == ignoreEventId;
    }

    public async Task<bool> IsAnyCheckedInAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        CancellationToken cancellationToken = default
    )
    {
        var activities = await GetActivityAsync(
            client,
            memberId: null,
            notBefore,
            EventsExpand.None,
            cancellationToken
        );

        // Re-order activities so that we process the oldest activity first
        activities.Reverse();

        var isCheckedIn = new HashSet<string>();

        foreach (var activity in activities)
        {
            if (activity.CheckInEvent is not null)
            {
                isCheckedIn.Add(activity.MemberId);
            }
            if (activity.CheckOutEvent is not null)
            {
                isCheckedIn.Remove(activity.MemberId);
            }
        }

        return isCheckedIn.Count > 0;
    }

    public async Task<List<AccessActivity>> GetActivityAsync(
        IAkilesApiClient client,
        string? memberId,
        DateTimeOffset notBefore,
        EventsExpand expand = EventsExpand.None,
        CancellationToken cancellationToken = default
    )
    {
        var filter = new ListEventsFilter() { Object = new() { GadgetId = GadgetId }, };
        if (memberId is not null)
        {
            filter.Subject = new() { MemberId = memberId };
        }

        var recentEvents = await client
            .Events.ListRecentEventsAsync(notBefore, filter, expand, cancellationToken)
            .ToListAsync(cancellationToken);

        // Change event order to ascending, that is, the oldest event is the first
        recentEvents.Reverse();

        var activities = new List<AccessActivity>(recentEvents.Count / 2);
        var inStore = new Dictionary<string, AccessActivity>();
        foreach (var evnt in recentEvents)
        {
            memberId = evnt.Subject.MemberId;
            if (memberId is null)
            {
                continue;
            }

            switch (evnt.Object.GadgetActionId)
            {
                case Actions.CheckIn:

                    {
                        var activity = new AccessActivity()
                        {
                            MemberId = memberId,
                            CheckInEvent = evnt
                        };
                        activities.Add(activity);

                        // Override any in-store in case the member exited by other means
                        inStore[memberId] = activity;
                    }
                    break;
                case Actions.CheckOut:

                    {
                        if (inStore.Remove(memberId, out var activity))
                        {
                            activity.CheckOutEvent = evnt;
                        }
                        else
                        {
                            activities.Add(
                                new AccessActivity { MemberId = memberId, CheckOutEvent = evnt }
                            );
                        }
                    }
                    break;
            }
        }

        // Change activites order to descending, that is, the newest event is the first
        activities.Reverse();

        return activities;
    }

    public IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        CancellationToken cancellationToken
    ) => client.Events.ListRecentGadgetEventsAsync(GadgetId, notBefore, expand, cancellationToken);

    public static class Actions
    {
        public const string CheckIn = "check_in";
        public const string CheckOut = "check_out";
    }
}

public record AccessActivity
{
    public required string MemberId { get; init; }
    public Event? CheckInEvent { get; set; }
    public Event? CheckOutEvent { get; set; }
}
