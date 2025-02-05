namespace Akiles.Api.MemberGroups;

public record MemberGroupPatch
{
    public List<MemberGroupPermissionRule>? Permissions { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
