using Akiles.Api.Events;
using Akiles.Api.Gadgets;
using Akiles.Api.Members;

namespace Akiles.Api;

public interface IAkilesApiClient
{
    public IEvents Events { get; }
    public IGadgets Gadgets { get; }
    public IMembers Members { get; }
}
