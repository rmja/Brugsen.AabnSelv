using Akiles.ApiClient;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Commands;

public class DeleteMember : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapDelete("/api/members/{memberId}", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string memberId,
        IAkilesApiClient apiClient,
        CancellationToken cancellationToken
    )
    {
        await apiClient.Members.DeleteMemberAsync(memberId, cancellationToken);
        return Results.NoContent();
    }
}
