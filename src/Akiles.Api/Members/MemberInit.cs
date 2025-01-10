namespace Akiles.Api.Members;

public record MemberInit
{
    public required string Name { get; init; }
    public DateTime? StartsAt { get; init; }
    public DateTime? EndsAt { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];
}
