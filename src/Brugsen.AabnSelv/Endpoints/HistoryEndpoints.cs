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

        history.MapGet("/access-activity", GetAccessActivityAsync);
        history.MapGet("/{gadgetName}-events", GetGadgetEventsAsync);
    }

    private static async Task<IResult> GetAccessActivityAsync(
        IAccessGadget accessGadget,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var notBefore = timeProvider.GetLocalNow().AddDays(-7);

        var activity = await accessGadget.GetActivityAsync(
            client,
            memberId: null,
            notBefore,
            EventsExpand.SubjectMember,
            cancellationToken
        );

        return Results.Ok(activity.Select(x => x.ToDto()));
    }

    private static async Task<IResult> GetGadgetEventsAsync(
        string gadgetName,
        IAlarmGadget alarm,
        IFrontDoorLockGadget doorLock,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        ILogger<AlarmGadget> gadgetLogger,
        CancellationToken cancellationToken
    )
    {
        var gadgetId = gadgetName switch
        {
            "alarm" => alarm.GadgetId,
            "front-door-lock" => doorLock.GadgetId,
            _ => null
        };
        if (gadgetId is null)
        {
            return Results.NotFound();
        }
        if (gadgetId.StartsWith("noop-"))
        {
            return Results.Ok(Array.Empty<EventDto>());
        }

        var notBefore = timeProvider.GetLocalNow().AddDays(-3);
        var events = await client
            .Events.ListRecentGadgetEventsAsync(
                gadgetId,
                notBefore,
                EventsExpand.None,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        return Results.Ok(events.Select(x => x.ToDto()));
    }
}
