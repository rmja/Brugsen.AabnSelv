using Akiles.Api.Members;

namespace Brugsen.AabnSelv;

public static class MemberExtensions
{
    public static bool IsApproved(this Member member, BrugsenAabnSelvOptions options) =>
        member.GroupAssociations is not null
        && member.IsApproved(member.GroupAssociations, options);

    public static bool IsApproved(
        this Member member,
        List<MemberGroupAssociation> groupAssociations,
        BrugsenAabnSelvOptions options
    ) => groupAssociations.Any(x => x.MemberGroupId == options.ApprovedMemberGroupId);
}
