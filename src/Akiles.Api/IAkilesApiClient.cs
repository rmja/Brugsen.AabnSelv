using Akiles.Api.Members;

namespace Akiles.Api;

public interface IAkilesApiClient
{
    public IMembers Members { get; }
}
