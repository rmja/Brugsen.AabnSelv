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

        history.MapGet("/front-door-activity", GetFrontDoorActivityAsync);
        history.MapGet("/alarm-events", GetAlarmEventsAsync);
    }

    private static async Task<IResult> GetFrontDoorActivityAsync(
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var frontDoorGadget = new DoorGadget(options.Value.FrontDoorGadgetId, client);
        var notBefore = timeProvider.GetLocalNow().AddDays(-7);

        var recentEvents = await frontDoorGadget
            .GetRecentEventsAsync(notBefore, EventsExpand.ObjectMember, cancellationToken)
            .ToListAsync(cancellationToken);

        // Change event order to ascending
        recentEvents.Reverse();

        var activities = new List<FrontDoorActivityDto>(recentEvents.Count / 2);
        var inStore = new Dictionary<string, FrontDoorActivityDto>();
        foreach (var evnt in recentEvents)
        {
            var member = evnt.ObjectMember;
            if (member is null)
            {
                continue;
            }

            switch (evnt.Object.GadgetActionId)
            {
                case DoorGadget.Actions.OpenEntry:

                    {
                        var activity = new FrontDoorActivityDto()
                        {
                            MemberId = member.Id,
                            MemberName = member.Name,
                            EnteredAt = evnt.CreatedAt
                        };
                        activities.Add(activity);

                        // Override any in-store in case the member exited by other means
                        inStore[activity.MemberId] = activity;
                    }
                    break;
                case DoorGadget.Actions.OpenExit:

                    {
                        if (inStore.Remove(member.Id, out var activity))
                        {
                            activity.ExitedAt = evnt.CreatedAt;
                        }
                        else
                        {
                            activities.Add(
                                new FrontDoorActivityDto
                                {
                                    MemberId = member.Id,
                                    MemberName = member.Name,
                                    ExitedAt = evnt.CreatedAt
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
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        ILogger<AlarmGadget> gadgetLogger,
        CancellationToken cancellationToken
    )
    {
        var gadgetId = options.Value.AlarmGadgetId;
        if (gadgetId is null)
        {
            return Results.Ok(Array.Empty<EventDto>());
        }

        var gadget = new AlarmGadget(gadgetId, client, gadgetLogger);
        var notBefore = timeProvider.GetLocalNow().AddDays(-7);

        var events = await gadget
            .GetRecentEventsAsync(notBefore, EventsExpand.None, cancellationToken)
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
