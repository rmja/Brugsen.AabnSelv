using Akiles.ApiClient;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Queries;

public class GetMember : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/api/members/{memberId}", HandleAsync);

    private static async Task<IResult> HandleAsync(
        string memberId,
        IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    )
    {
        var member = await apiClient.Members.GetMemberAsync(
            memberId,
            //MembersExpand.Emails | MembersExpand.GroupAssociations,
            cancellationToken
        );
        if (member.IsDeleted)
        {
            return Results.NotFound();
        }

        //return Results.Ok(member.ToDto(member.Emails!.First(), member.GroupAssociations));

        var email = await apiClient.Members.ListEmailsAsync(memberId).FirstAsync(cancellationToken);
        var groupAssociations = await apiClient
            .Members.ListGroupAssociationsAsync(memberId)
            .ToListAsync(cancellationToken);
        return Results.Ok(
            member.ToDto(email, isApproved: member.IsApproved(groupAssociations, options.Value))
        );
    }
}
