using Brugsen.AabnSelv.Endpoints;

namespace Brugsen.AabnSelv.Models;

public class ActionEventDto
{
    public required string Action { get; set; }
    public AccessMethodDto? Method { get; set; }
    public DateTime CreatedAt { get; set; }
}
