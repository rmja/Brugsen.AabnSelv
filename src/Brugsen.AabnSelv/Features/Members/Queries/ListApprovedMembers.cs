using Akiles.ApiClient;
using Akiles.ApiClient.Members;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Queries;

public class ListApprovedMembers : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/api/members/approved", HandleAsync);

    private static async Task<IResult> HandleAsync(
        [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    )
    {
        var members = await apiClient
            .Members.ListMembersAsync(
                filter: new() { IsDeleted = IsDeleted.False },
                expand: MembersExpand.GroupAssociations | MembersExpand.Emails
            )
            .WhereAsync(x => x.IsApproved(options.Value) && x.Emails?.Count > 0, cancellationToken)
            .ToListAsync(cancellationToken);
        return Results.Ok(
            members.Select(x => x.ToDto(x.Emails!.First(), isApproved: true)).ToList()
        );
    }
}
