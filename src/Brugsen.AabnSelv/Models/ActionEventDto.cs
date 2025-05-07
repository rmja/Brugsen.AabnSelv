using Brugsen.AabnSelv.Endpoints;

namespace Brugsen.AabnSelv.Models;

public record ActionEventDto
{
    public required string Action { get; set; }
    public AccessMethod? Method { get; set; }
    public DateTime CreatedAt { get; set; }
}
