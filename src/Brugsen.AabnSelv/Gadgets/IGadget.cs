namespace Brugsen.AabnSelv.Gadgets;

public interface IGadget : IAkilesEntity
{
    string GadgetId { get; }
    GadgetEntity GadgetEntity { get; }
}
