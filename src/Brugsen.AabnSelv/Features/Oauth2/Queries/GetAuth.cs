using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Oauth2.Queries;

public class GetAuth : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/oauth2/auth", Handle);

    static IResult Handle(HttpRequest request) =>
        Results.Redirect("https://auth.akiles.app/oauth2/auth" + request.QueryString);
}
