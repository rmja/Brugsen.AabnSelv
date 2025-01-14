using Microsoft.Extensions.DependencyInjection;

namespace Akiles.Api;

public class AkilesApiClientFactory(IServiceProvider services) : IAkilesApiClientFactory
{
    public IAkilesApiClient Create(string accessToken) =>
        ActivatorUtilities.CreateInstance<AkilesApiClient>(services, [accessToken]);
}

public interface IAkilesApiClientFactory
{
    IAkilesApiClient Create(string accessToken);
}
