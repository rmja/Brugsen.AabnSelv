using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Webhooks.Commands;

public class RegisterWebhooks : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/webhooks/register", HandleAsync);

    // E.g. curl -X POST http://localhost:60900/api/webhooks/register?url=https://example.com/aabn-selv/api/webhooks/gadget-action/use --verbose
    // The id and secret is then returned in the response
    private static async Task<IResult> HandleAsync(
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
                IsEnabled = true,
            }
        );

        return Results.Ok(webhook);
    }
}
