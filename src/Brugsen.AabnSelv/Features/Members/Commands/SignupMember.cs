using Akiles.ApiClient;
using Akiles.ApiClient.Members;
using Brugsen.AabnSelv.Models;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.Members.Commands;

public class SignupMember : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapPost("/api/members/signup", HandleAsync);

    private static async Task<IResult> HandleAsync(
        MemberDto init,
        [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient apiClient,
        CancellationToken cancellationToken
    )
    {
        var existing = await Task.WhenAll(
            [
                apiClient
                    .Members.EnumerateMembersAsync(
                        null,
                        new ListMembersFilter() { Email = init.Email }
                    )
                    .FirstOrDefaultAsync(cancellationToken),
                apiClient
                    .Members.EnumerateMembersAsync(
                        null,
                        new ListMembersFilter()
                        {
                            Metadata = new() { [MetadataKeys.Member.Phone] = init.Phone },
                        }
                    )
                    .FirstOrDefaultAsync(cancellationToken),
                init.LaesoeCardNumber is not null
                    ? apiClient
                        .Members.EnumerateMembersAsync(
                            null,
                            new ListMembersFilter()
                            {
                                Metadata = new()
                                {
                                    [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber,
                                },
                            }
                        )
                        .FirstOrDefaultAsync(cancellationToken)
                    : Task.FromResult<Member?>(null),
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
                type: "api://members/signup/phone-conflict",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        if (existing[2] is not null)
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
                Language = "da",
                Metadata =
                {
                    [MetadataKeys.Member.Phone] = init.Phone,
                    [MetadataKeys.Member.Address] = init.Address,
                    [MetadataKeys.Member.CoopMembershipNumber] =
                        init.CoopMembershipNumber.TrimStart('0'),
                    [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber,
                    [MetadataKeys.Member.LaesoeCardColor] = init
                        .LaesoeCardColor?.ToString()
                        .ToLowerInvariant(),
                },
            },
            cancellationToken
        );

        var email = await apiClient.Members.CreateEmailAsync(
            member.Id,
            new() { Email = init.Email },
            CancellationToken.None
        );

        try
        {
            // The pin is only used on check-out, so it is not "that sensitive".
            // When Akiles support multiple pins with different sensitivity level,
            // then this pin should clearly be marked as "not sensitive".
            await apiClient.Members.CreatePinAsync(
                member.Id,
                new() { Pin = init.Phone.TrimStart('+') },
                CancellationToken.None
            );
        }
        catch (AkilesApiException ex) when (ex.ErrorType == AkilesErrorTypes.InvalidRequest)
        {
            // Swallow
            // The pin could not be created as it is already being used
            // This does not make any sense as we have already validated that the phone number is unique
        }

        return Results.Ok(member.ToDto(email, isApproved: false));
    }
}
