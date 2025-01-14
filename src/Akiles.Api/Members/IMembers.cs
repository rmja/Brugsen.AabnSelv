using Refit;

namespace Akiles.Api.Members;

public interface IMembers
{
    [Get("/members")]
    Task<PagedList<Member>> ListMembersAsync(
        string? cursor,
        int? limit,
        string? sort,
        ListMembersFilter? filter = null,
        MembersExpand expand = MembersExpand.None,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Member> ListMembersAsync(
        string? sort = null,
        ListMembersFilter? filter = null,
        MembersExpand expand = MembersExpand.None
    ) =>
        new PaginationEnumerable<Member>(
            (cursor, cancellationToken) =>
                ListMembersAsync(
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    filter,
                    expand,
                    cancellationToken
                )
        );

    [Get("/members/{memberId}")]
    Task<Member> GetMemberAsync(
        string memberId,
        //MembersExpand expand = MembersExpand.None,
        CancellationToken cancellationToken = default
    );

    [Post("/members")]
    Task<Member> CreateMemberAsync(
        MemberInit member,
        CancellationToken cancellationToken = default
    );

    [Delete("/members/{memberId}")]
    Task<Member> DeleteMemberAsync(string memberId, CancellationToken cancellationToken = default);

    [Post("/members/{memberId}/emails")]
    Task<MemberEmail> CreateEmailAsync(
        string memberId,
        MemberEmailInit email,
        CancellationToken cancellationToken = default
    );

    [Get("/members/{memberId}/emails")]
    Task<PagedList<MemberEmail>> ListEmailsAsync(
        string memberId,
        string? cursor,
        int? limit,
        string? sort,
        string? q = null,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<MemberEmail> ListEmailsAsync(
        string memberId,
        string? sort = null,
        string? q = null
    ) =>
        new PaginationEnumerable<MemberEmail>(
            (cursor, cancellationToken) =>
                ListEmailsAsync(
                    memberId,
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    q,
                    cancellationToken
                )
        );

    [Post("/members/{memberId}/cards")]
    Task<MemberCard> CreateCardAsync(
        string memberId,
        MemberCardInit card,
        CancellationToken cancellationToken = default
    );

    [Post("/members/{memberId}/group_associations")]
    Task<MemberGroupAssociation> CreateGroupAssociationAsync(
        string memberId,
        MemberGroupAssociationInit group,
        CancellationToken cancellationToken = default
    );

    [Get("/members/{memberId}/group_associations")]
    Task<PagedList<MemberGroupAssociation>> ListGroupAssociationsAsync(
        string memberId,
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<MemberGroupAssociation> ListGroupAssociationsAsync(
        string memberId,
        string? sort = null
    ) =>
        new PaginationEnumerable<MemberGroupAssociation>(
            (cursor, cancellationToken) =>
                ListGroupAssociationsAsync(
                    memberId,
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    cancellationToken
                )
        );
}

public record ListMembersFilter
{
    public IsDeleted? IsDeleted { get; set; }
    public string? Email { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
