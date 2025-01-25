using Akiles.Api;
using Akiles.Api.Events;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class WebhooksEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var webhook = builder.MapGroup("/api/webhooks");

        webhook.MapPost("/register", RegisterWebhooksAsync);
        webhook.MapPost("/gadget-action/use", ProcessGadgetActionEventAsync);
    }

    // E.g. curl -X POST http://localhost:60900/api/webhooks/register?url=https://example.com/aabn-selv/api/webhooks/gadget-action/use --verbose
    // The id and secret is then returned the response
    private static async Task<IResult> RegisterWebhooksAsync(
        string url,
        [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options
    )
    {
        if (options.Value.WebhookId is not null)
        {
            return Results.Conflict();
        }

        var webhook = await client.Webhooks.CreateWebhookAsync(
            new()
            {
                Filter = { new(EventObjectType.GadgetAction, EventVerb.Use) },
                Url = url,
                IsEnabled = true
            }
        );

        return Results.Ok(webhook);
    }

    private static async Task<IResult> ProcessGadgetActionEventAsync(
        HttpRequest request,
        WebhookEventValidator validator,
        IAccessGadget accessGadget,
        ICheckInPinpadGadget checkInPinpadGadget,
        ICheckOutPinpadGadget checkOutPinpadGadget,
        IAccessController accessProcessor,
        CancellationToken cancellationToken
    )
    {
        var evnt = await validator.ReadSignedEventOrNullAsync(request, cancellationToken);
        if (evnt is null)
        {
            return Results.BadRequest();
        }
        var memberId = evnt.Subject.MemberId;
        if (memberId is null)
        {
            return Results.NoContent();
        }

        if (evnt.Object.GadgetId == accessGadget.GadgetId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case AccessGadget.Actions.CheckIn:
                    await accessProcessor.ProcessCheckInAsync(evnt.Id, memberId);
                    break;
                case AccessGadget.Actions.CheckOut:
                    await accessProcessor.ProcessCheckOutAsync(
                        evnt.Id,
                        memberId,
                        enforceCheckedIn: true
                    );
                    break;
            }
        }
        else if (evnt.Object.GadgetId == checkInPinpadGadget.GadgetId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case CheckInPinpadGadget.Actions.CheckIn:
                {
                    await accessProcessor.ProcessCheckInAsync(evnt.Id, memberId);
                    break;
                }
            }
        }
        else if (evnt.Object.GadgetId == checkOutPinpadGadget.GadgetId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case CheckOutPinpadGadget.Actions.CheckOut:
                {
                    await accessProcessor.ProcessCheckOutAsync(
                        evnt.Id,
                        memberId,
                        enforceCheckedIn: false
                    );
                    break;
                }
            }
        }

        return Results.NoContent();
    }
}
