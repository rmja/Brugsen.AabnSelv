namespace Akiles.Api.Gadgets;

public record GadgetPatch
{
    public Dictionary<string, string>? Metadata { get; set; }
}
