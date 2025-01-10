namespace Akiles.Api.Members;

public record Member
{
    public required string Id { get; set; }
    public required string OrganizationId { get; set; }
    public required string Name { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = [];
}
