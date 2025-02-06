using Akiles.Api;
using Akiles.Api.Members;
using Brugsen.AabnSelv.Models;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Endpoints;

public static class MembersEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var members = builder.MapGroup("/api/members");

        members.MapPost("/signup", SignupAsync);
        members.MapGet("/pending-approval", GetAllPendingApprovalAsync);
        members.MapGet("/approved", GetAllApprovedAsync);
        members.MapGet("/{memberId}", GetMemberAsync);
        members.MapGet("/{memberId}/is-approved", GetMemberApprovedIndicationAsync);
        members.MapPost("/{memberId}/approve", ApproveAsync);
        members.MapDelete("/{memberId}", DeleteAsync);
    }

    private static async Task<IResult> SignupAsync(
        MemberDto init,
        [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient apiClient,
        CancellationToken cancellationToken
    )
    {
        var existing = await Task.WhenAll(
            [
                apiClient
                    .Members.ListMembersAsync(null, new ListMembersFilter() { Email = init.Email })
                    .FirstOrDefaultAsync(cancellationToken),
                apiClient
                    .Members.ListMembersAsync(
                        null,
                        new ListMembersFilter()
                        {
                            Metadata = new()
                            {
                                [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber
                            }
                        }
                    )
                    .FirstOrDefaultAsync(cancellationToken)
            ]
        );

        if (existing[0] is not null)
        {
            return Results.Problem(
                type: "api://members/signup/email-conflict",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        if (existing[1] is not null)
        {
            return Results.Problem(
                type: "api://members/signup/laesoe-card-number-conflict",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var member = await apiClient.Members.CreateMemberAsync(
            new()
            {
                Name = init.Name,
                Metadata =
                {
                    [MetadataKeys.Member.Phone] = init.Phone,
                    [MetadataKeys.Member.Address] = init.Address,
                    [MetadataKeys.Member.CoopMembershipNumber] = init.CoopMembershipNumber,
                    [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber,
                    [MetadataKeys.Member.LaesoeCardColor] = init
                        .LaesoeCardColor.ToString()
                        .ToLowerInvariant()
                }
            },
            cancellationToken
        );

        var email = await apiClient.Members.CreateEmailAsync(
            member.Id,
            new() { Email = init.Email },
            CancellationToken.None
        );

        // The pin is only used on check-out, so it is not "that sensitive".
        // When Akiles support multiple pins with different sensitivity level,
        // then this pin should clearly be marked as "not sensitive".
        await apiClient.Members.CreatePinAsync(
            member.Id,
            new() { Pin = init.Phone.TrimStart('+') },
            CancellationToken.None
        );

        return Results.Ok(member.ToDto(email, isApproved: false));
    }

    private static async Task<IResult> GetAllPendingApprovalAsync(
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
            .WhereAsync(
                x => !x.IsApproved(options.Value) && x.Emails?.Any() == true,
                cancellationToken
            )
            .ToListAsync(cancellationToken);
        return Results.Ok(members.Select(x => x.ToDto(x.Emails!.First(), isApproved: true)));
    }

    private static async Task<IResult> GetAllApprovedAsync(
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
            .WhereAsync(
                x => x.IsApproved(options.Value) && x.Emails?.Any() == true,
                cancellationToken
            )
            .ToListAsync(cancellationToken);
        return Results.Ok(members.Select(x => x.ToDto(x.Emails!.First(), isApproved: true)));
    }

    private static async Task<IResult> GetMemberAsync(
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

    private static async Task<IResult> GetMemberApprovedIndicationAsync(
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
            .Members.ListGroupAssociationsAsync(memberId)
            .ToListAsync(cancellationToken);

        return Results.Ok(member.IsApproved(groupAssociations, options.Value));
    }

    private static async Task<IResult> ApproveAsync(
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

    private static async Task<IResult> DeleteAsync(
        string memberId,
        IAkilesApiClient apiClient,
        CancellationToken cancellationToken
    )
    {
        await apiClient.Members.DeleteMemberAsync(memberId, cancellationToken);
        return Results.NoContent();
    }
}
