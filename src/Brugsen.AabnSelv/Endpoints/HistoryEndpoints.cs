using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class HistoryEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var history = builder.MapGroup("/api/history");

        history.MapGet("/access-activity", GetAccessActivityAsync);
        history.MapGet("/alarm-events", GetAlarmEventsAsync);
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

        return Results.Ok(events.Select(x => x.ToDto()));
    }
}
