using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Akiles.Api.Tests;

public class ApiFixture
{
    public IAkilesApiClient Client { get; }

    public ApiFixture()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<MembersTests>().Build();
        var services = new ServiceCollection()
            .AddAkilesApi(options =>
            {
                options.ApiKey = config["AkilesApiKey"];
            })
            .BuildServiceProvider();
        Client = services.GetRequiredService<IAkilesApiClient>();
    }
}
