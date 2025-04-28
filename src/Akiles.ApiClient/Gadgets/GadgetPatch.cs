namespace Akiles.ApiClient.Gadgets;

public record GadgetPatch
{
    public Dictionary<string, string>? Metadata { get; set; }
}
