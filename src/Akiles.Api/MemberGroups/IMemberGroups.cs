using Refit;

namespace Akiles.Api.MemberGroups;

public interface IMemberGroups
{
    [Get("/member_groups")]
    Task<PagedList<MemberGroup>> ListMemberGroupsAsync(
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<MemberGroup> ListMemberGroupsAsync(string? sort = null) =>
        new PaginationEnumerable<MemberGroup>(
            (cursor, cancellationToken) =>
                ListMemberGroupsAsync(
                    cursor,
                    Constants.DefaultPaginationLimit,
                    sort,
                    cancellationToken
                )
        );

    [Get("/member_groups/{memberGroupId}")]
    Task<MemberGroup> GetMemberGroupAsync(
        string memberGroupId,
        CancellationToken cancellationToken = default
    );

    [Post("/member_groups")]
    Task<MemberGroup> CreateMemberAsync(
        MemberGroupInit member,
        CancellationToken cancellationToken = default
    );

    [Patch("/member_groups/{memberGroupId}")]
    Task<MemberGroup> EditMemberGroupAsync(
        string memberGroupId,
        MemberGroupPatch patch,
        CancellationToken cancellationToken = default
    );

    [Delete("/member_groups/{memberGroupId}")]
    Task<MemberGroup> DeleteMemberAsync(
        string memberGroupId,
        CancellationToken cancellationToken = default
    );
}
