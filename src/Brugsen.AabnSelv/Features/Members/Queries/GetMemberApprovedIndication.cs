using Akiles.ApiClient;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Queries;

public class GetMemberApprovedIndication : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/api/members/{memberId}/is-approved", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string memberId,
        [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    )
    {
        var member = await apiClient.Members.GetMemberAsync(memberId, cancellationToken);
        if (member.IsDeleted)
        {
            return Results.NotFound();
        }
        var groupAssociations = await apiClient
            .Members.EnumerateGroupAssociationsAsync(memberId)
            .ToListAsync(cancellationToken);

        return Results.Ok(member.IsApproved(groupAssociations, options.Value));
    }
}
