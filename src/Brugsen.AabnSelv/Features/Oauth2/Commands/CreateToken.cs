using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Oauth2.Commands;

public class CreateToken : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/oauth2/auth", Handle);

    static void Handle(HttpRequest request) =>
        Results.Redirect("https://auth.akiles.app/oauth2/auth" + request.QueryString);
}
