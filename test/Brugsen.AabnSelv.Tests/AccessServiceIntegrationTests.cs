using Akiles.Api;
using Brugsen.AabnSelv.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brugsen.AabnSelv.Tests;

public class AccessServiceIntegrationTests
{
    private readonly IAkilesApiClient _client;
    private readonly IAccessService _accessService;

    public AccessServiceIntegrationTests()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IAccessService, AccessService>()
            .AddAkilesApi()
            .AddGadgets()
            .AddDevices()
            .AddKeyedSingleton(
                ServiceKeys.ApiKeyClient,
                (services, _) =>
                {
                    var factory = services.GetRequiredService<IAkilesApiClientFactory>();
                    var apiKey = config["AkilesApiKey"]!;
                    return factory.Create(apiKey);
                }
            )
            .AddSingleton<TimeProvider, DanishTimeProvider>()
            .Configure<BrugsenAabnSelvOptions>(config)
            .BuildServiceProvider();

        _client = services.GetRequiredKeyedService<IAkilesApiClient>(ServiceKeys.ApiKeyClient);
        _accessService = services.GetRequiredService<IAccessService>();
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn()
    {
        // Given
        const string MemberId = "mem_41eleh9x15dus6z8yqyh";

        // When
        var result = await _accessService.IsMemberCheckedInAsync(
            _client,
            MemberId,
            notBefore: DateTimeOffset.Now.AddHours(-1)
        );

        // Then
        Assert.False(result);
    }
}
