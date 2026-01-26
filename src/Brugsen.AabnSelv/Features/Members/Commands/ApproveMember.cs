using Akiles.ApiClient;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Commands;

public class ApproveMember : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/members/{memberId}/approve", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string memberId,
        IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    )
    {
        await apiClient.Members.CreateGroupAssociationAsync(
            memberId,
            new() { MemberGroupId = options.Value.ApprovedMemberGroupId },
            cancellationToken
        );
        return Results.NoContent();
    }
}
