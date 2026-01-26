using System.Text.Json;
using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Models;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.History.Queries;

public class GetGadgetActionEvents : IEndpoint
{
    private static readonly Dictionary<string, GadgetEntity> _gadgetEntities =
        Enum.GetValues<GadgetEntity>()
            .ToDictionary(x => JsonNamingPolicy.KebabCaseLower.ConvertName(x.ToString()), x => x);

    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/api/history/{gadgetEntity}-action-events", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string gadgetEntity,
        DateTime? notBefore,
        IEnumerable<IGadget> gadgets,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        ILogger<AlarmGadget> gadgetLogger,
        CancellationToken cancellationToken
    )
    {
        if (!_gadgetEntities.TryGetValue(gadgetEntity, out var entity))
        {
            return Results.NotFound();
        }

        var gadget = gadgets.SingleOrDefault(x => x.GadgetEntity == entity);
        if (gadget is null)
        {
            return Results.NotFound();
        }

        if (gadget.GadgetId.StartsWith("noop-"))
        {
            return Results.Ok(Array.Empty<ActionEventDto>());
        }

        var events = await client
            .Events.ListEventsAsync(
                "created_at:desc",
                new ListEventsFilter()
                {
                    Object = new() { GadgetId = gadget.GadgetId },
                    CreatedAt = new()
                    {
                        GreaterThanOrEqual = timeProvider.GetLocalDateTimeOffset(
                            notBefore ?? timeProvider.GetLocalNow().Date.AddDays(-1)
                        ),
                    },
                },
                EventsExpand.None
            )
            .ToListAsync(cancellationToken);

        return Results.Ok(
            events.Where(x => x.Object.GadgetActionId is not null).Select(x => x.ToDto()).ToList()
        );
    }
}
