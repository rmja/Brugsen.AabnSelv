using Refit;

namespace Akiles.Api.Gadgets;

public interface IGadgets
{
    [Get("/gadgets/{gadgetId}")]
    Task<Gadget> GetGadgetAsync(string gadgetId, CancellationToken cancellationToken = default);

    [Patch("/gadgets/{gadgetId}")]
    Task<Gadget> EditGadgetAsync(
        string gadgetId,
        GadgetPatch patch,
        CancellationToken cancellationToken = default
    );

    [Post("/gadgets/{gadgetId}/actions/{actionId}")]
    Task DoGadgetActionAsync(
        string gadgetId,
        string actionId,
        CancellationToken cancellationToken = default
    );
}
