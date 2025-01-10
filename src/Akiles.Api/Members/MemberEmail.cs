namespace Akiles.Api.Members;

public record MemberEmail
{
    public required string Id { get; set; }
    public required string OrganizationId { get; set; }
    public required string MemberId { get; set; }
    public required string Email { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}
