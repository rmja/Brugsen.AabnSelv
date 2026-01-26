using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Oauth2.Queries;

public class GetAuth : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/oauth2/token", HandleAsync);

    private static async Task HandleAsync(
        HttpRequest request,
        HttpResponse response,
        HttpClient httpClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    )
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var entries = form.Select(x => KeyValuePair.Create(x.Key, x.Value.First()!)).ToList();
        entries.Add(KeyValuePair.Create("client_secret", options.Value.AkilesClientSecret));
        var content = new FormUrlEncodedContent(entries);
        var result = await httpClient.PostAsync(
            "https://auth.akiles.app/oauth2/token",
            content,
            cancellationToken
        );

        await CopyToResponseAsync(result, response, cancellationToken);
    }

    private static async Task CopyToResponseAsync(
        HttpResponseMessage clientResponse,
        HttpResponse apiResponse,
        CancellationToken cancellationToken
    )
    {
        apiResponse.StatusCode = (int)clientResponse.StatusCode;
        foreach (var header in clientResponse.Content.Headers)
        {
            foreach (var value in header.Value)
            {
                apiResponse.Headers.Append(header.Key, value);
            }
        }
        using var stream = await clientResponse.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(apiResponse.Body, cancellationToken);
    }
}
