﻿using System.Text.Json;
using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Models;
using Brugsen.AabnSelv.Services;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class HistoryEndpoints
{
    private static readonly Dictionary<string, GadgetEntity> _gadgetEntities =
        Enum.GetValues<GadgetEntity>()
            .ToDictionary(x => JsonNamingPolicy.KebabCaseLower.ConvertName(x.ToString()), x => x);

    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var history = builder.MapGroup("/api/history");

        history.MapGet("/access-activity", GetAccessActivityAsync);
        history.MapGet("/{gadgetEntity}-action-events", GetGadgetActionEventsAsync);
    }

    private static async Task<IResult> GetAccessActivityAsync(
        DateTime? notBefore,
        IAccessService accessService,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var activity = await accessService.GetActivityAsync(
            client,
            memberId: null,
            notBefore: timeProvider.GetLocalDateTimeOffset(
                notBefore ?? timeProvider.GetLocalNow().Date.AddDays(-1)
            ),
            EventsExpand.SubjectMember,
            cancellationToken
        );

        return Results.Ok(activity.Select(x => x.ToDto()));
    }

    private static async Task<IResult> GetGadgetActionEventsAsync(
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
            return Results.Ok(Array.Empty<EventDto>());
        }

        var events = await client
            .Events.ListRecentGadgetEventsAsync(
                gadget.GadgetId,
                notBefore: timeProvider.GetLocalDateTimeOffset(
                    notBefore ?? timeProvider.GetLocalNow().Date.AddDays(-1)
                ),
                EventsExpand.None,
                cancellationToken
            )
            .ToListAsync(cancellationToken);

        return Results.Ok(
            events.Where(x => x.Object.GadgetActionId is not null).Select(x => x.ToDto())
        );
    }
}
