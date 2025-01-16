using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class OAuthEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var oauth2 = builder.MapGroup("/oauth2");

        oauth2.MapGet(
            "/auth",
            (HttpRequest request) =>
            {
                return Results.Redirect(
                    "https://auth.akiles.app/oauth2/auth" + request.QueryString
                );
            }
        );
        oauth2.MapPost(
            "/token",
            async (
                HttpRequest request,
                HttpResponse response,
                HttpClient httpClient,
                IOptions<BrugsenAabnSelvOptions> options,
                CancellationToken cancellationToken
            ) =>
            {
                var form = await request.ReadFormAsync(cancellationToken);
                var entries = form.Select(x => KeyValuePair.Create(x.Key, x.Value.First()!))
                    .ToList();
                entries.Add(KeyValuePair.Create("client_secret", options.Value.AkilesClientSecret));
                var content = new FormUrlEncodedContent(entries);
                var result = await httpClient.PostAsync(
                    "https://auth.akiles.app/oauth2/token",
                    content,
                    cancellationToken
                );

                await CopyToResponseAsync(result, response, cancellationToken);
            }
        );
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
