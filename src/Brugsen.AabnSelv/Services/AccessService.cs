using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv.Services;

public class AccessService(
    IAppAccessGadget appAccessGadget,
    ICheckInPinpadGadget checkInPinpadGadget,
    IFrontDoorGadget frontDoorGadget
) : IAccessService
{
    public async Task<bool> IsMemberCheckedInAsync(
        IAkilesApiClient client,
        string memberId,
        DateTimeOffset notBefore,
        string? ignoreEventId,
        CancellationToken cancellationToken
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
        CancellationToken cancellationToken
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
        EventsExpand expand,
        CancellationToken cancellationToken
    )
    {
        var filter = new ListEventsFilter() { };
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

            if (evnt.Object.GadgetId == appAccessGadget.GadgetId)
            {
                switch (evnt.Object.GadgetActionId)
                {
                    case AppAccessGadget.Actions.CheckIn:
                        ProcessCheckIn();
                        break;
                    case AppAccessGadget.Actions.CheckOut:
                        ProcessCheckOut();
                        break;
                }
            }
            else if (evnt.Object.GadgetId == checkInPinpadGadget.GadgetId)
            {
                ProcessCheckIn();
            }
            else if (
                /* evnt.Object.DeviceId == checkOutPinpadDevice.DeviceId
                && */evnt.Object.GadgetId == frontDoorGadget.GadgetId
            )
            {
                ProcessCheckOut();
            }

            void ProcessCheckIn()
            {
                var activity = new AccessActivity() { MemberId = memberId, CheckInEvent = evnt };
                activities.Add(activity);

                // Override any in-store in case the member exited by other means
                inStore[memberId] = activity;
            }

            void ProcessCheckOut()
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
        }

        // Change activites order to descending, that is, the newest event is the first
        activities.Reverse();

        return activities;
    }
}

public record AccessActivity
{
    public required string MemberId { get; init; }
    public Event? CheckInEvent { get; set; }
    public Event? CheckOutEvent { get; set; }
}
