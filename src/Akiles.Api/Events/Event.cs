namespace Akiles.Api.Events;

public record Event
{
    public string Id { get; set; } = null!;

    //public required string OrganizationId { get; set; }
    public required EventSubject Subject { get; set; }
    public required EventVerb Verb { get; set; }
    public required EventObject Object { get; set; }
    public DateTime CreatedAt { get; set; }
}
