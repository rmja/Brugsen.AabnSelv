using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GatewayApi.Api.Tests;

public class ApiFixture
{
    public IGatewayApiClient Client { get; }

    public ApiFixture()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<ApiFixture>().Build();
        var services = new ServiceCollection()
            .AddGatewayApi(options =>
            {
                options.Token = config["GatewayApiToken"]!;
            })
            .BuildServiceProvider();
        Client = services.GetRequiredService<IGatewayApiClient>();
    }
}
