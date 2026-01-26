using Akiles.ApiClient.Events;
using Akiles.ApiClient.Webhooks;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Devices;
using Brugsen.AabnSelv.Gadgets;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Webhooks.Commands;

public class ProcessGadgetActionEvent : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/webhooks/gadget-action/use", HandleAsync);

    private static async Task<IResult> HandleAsync(
        HttpRequest request,
        IWebhookBinder webhookBinder,
        IAppAccessGadget appAccessGadget,
        ICheckInPinpadGadget checkInPinpadGadget,
        ICheckOutPinpadDevice checkOutPinpadDevice,
        IAccessController accessProcessor,
        ILogger<Endpoint> logger,
        CancellationToken cancellationToken
    )
    {
        var expectedSignatureHex = request
            .Headers[WebhookConstants.SignatureHeaderName]
            .FirstOrDefault();
        if (expectedSignatureHex is null)
        {
            return Results.BadRequest();
        }
        var evnt = await webhookBinder.BindEventAsync(
            request.Body,
            expectedSignatureHex,
            cancellationToken
        );
        if (evnt is null)
        {
            return Results.BadRequest();
        }
        var memberId = evnt.Subject.MemberId;
        if (memberId is null)
        {
            return Results.NoContent();
        }

        var logLevel = evnt.Verb == EventVerb.Use ? LogLevel.Information : LogLevel.Debug;
        logger.Log(
            logLevel,
            "Received {Verb} verb for gadget {GadgetId} with action {GadgetActionId} for member {MemberId}",
            evnt.Verb,
            evnt.Object.GadgetId,
            evnt.Object.GadgetActionId,
            memberId
        );

        if (evnt.Object.GadgetId == appAccessGadget.GadgetId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case AppAccessGadget.Actions.CheckIn:
                    await accessProcessor.ProcessCheckInAsync(evnt.Id, memberId, openDoor: true);
                    break;
                case AppAccessGadget.Actions.CheckOut:
                    await accessProcessor.ProcessCheckOutAsync(evnt.Id, memberId, openDoor: true);
                    break;
            }
        }
        else if (evnt.Object.GadgetId == checkInPinpadGadget.GadgetId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case CheckInPinpadGadget.Actions.CheckIn:
                {
                    await accessProcessor.ProcessCheckInAsync(evnt.Id, memberId, openDoor: false);
                    break;
                }
            }
        }
        else // if (evnt.Object.DeviceId == checkOutPinpadDevice.DeviceId)
        {
            switch (evnt.Object.GadgetActionId)
            {
                case FrontDoorGadget.Actions.OpenOnce:
                {
                    await accessProcessor.ProcessCheckOutAsync(evnt.Id, memberId, openDoor: false);
                    break;
                }
            }
        }

        return Results.NoContent();
    }
}
