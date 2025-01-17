using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Models;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class HistoryEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var history = builder.MapGroup("/api/history");

        history.MapGet("/store-activity", GetStoreActivityAsync);
        history.MapGet("/alarm-events", GetAlarmEventsAsync);
    }

    private static async Task<IResult> GetStoreActivityAsync(
        IFrontDoorGadget frontDoorGadget,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var notBefore = timeProvider.GetLocalNow().AddDays(-7);

        var recentEvents = await frontDoorGadget
            .GetRecentEventsAsync(client, notBefore, EventsExpand.ObjectMember, cancellationToken)
            .ToListAsync(cancellationToken);

        // Change event order to ascending
        recentEvents.Reverse();

        var activities = new List<StoreActivityDto>(recentEvents.Count / 2);
        var inStore = new Dictionary<string, StoreActivityDto>();
        foreach (var evnt in recentEvents)
        {
            var member = evnt.ObjectMember;
            if (member is null)
            {
                continue;
            }

            switch (evnt.Object.GadgetActionId)
            {
                case FrontDoorGadget.Actions.OpenEntry:

                    {
                        var activity = new StoreActivityDto()
                        {
                            MemberId = member.Id,
                            MemberName = member.Name,
                            CheckedInAt = evnt.CreatedAt
                        };
                        activities.Add(activity);

                        // Override any in-store in case the member exited by other means
                        inStore[activity.MemberId] = activity;
                    }
                    break;
                case FrontDoorGadget.Actions.OpenExit:

                    {
                        if (inStore.Remove(member.Id, out var activity))
                        {
                            activity.CheckedOutAt = evnt.CreatedAt;
                        }
                        else
                        {
                            activities.Add(
                                new StoreActivityDto
                                {
                                    MemberId = member.Id,
                                    MemberName = member.Name,
                                    CheckedOutAt = evnt.CreatedAt
                                }
                            );
                        }
                    }
                    break;
            }
        }

        // Change activites order to descending
        activities.Reverse();

        return Results.Ok(activities);
    }

    private static async Task<IResult> GetAlarmEventsAsync(
        IAlarmGadget alarmGadget,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        ILogger<AlarmGadget> gadgetLogger,
        CancellationToken cancellationToken
    )
    {
        var notBefore = timeProvider.GetLocalNow().AddDays(-7);

        var events = await alarmGadget
            .GetRecentEventsAsync(client, notBefore, EventsExpand.None, cancellationToken)
            .ToListAsync(cancellationToken);

        return Results.Ok(
            events.Select(x => new EventDto()
            {
                Action = x.Object.GadgetActionId!,
                CreatedAt = x.CreatedAt
            })
        );
    }
}
