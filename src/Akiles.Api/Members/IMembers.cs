using Refit;

namespace Akiles.Api.Members;

public interface IMembers
{
    [Get("/members")]
    Task<PagedList<Member>> ListMembersAsync(
        string? cursor,
        int? limit,
        string? sort,
        ListMembersFilter? filter,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Member> ListMembersAsync(
        string? sort = null,
        ListMembersFilter? filter = null
    ) =>
        new PaginationEnumerable<Member>(
            (cursor, cancellationToken) =>
                ListMembersAsync(
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    filter,
                    cancellationToken
                )
        );

    [Post("/members")]
    Task<Member> CreateMemberAsync(
        MemberInit member,
        CancellationToken cancellationToken = default
    );

    [Post("/members/{memberId}/emails")]
    Task<MemberEmail> CreateEmailAsync(
        string memberId,
        MemberEmailInit email,
        CancellationToken cancellationToken = default
    );

    [Post("/members/{memberId}/group_associations")]
    Task<MemberGroupAssociation> CreateGroupAssociationAsync(
        string memberId,
        MemberGroupAssociationInit group,
        CancellationToken cancellationToken = default
    );
}

public record ListMembersFilter
{
    public IsDeleted? IsDeleted { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
