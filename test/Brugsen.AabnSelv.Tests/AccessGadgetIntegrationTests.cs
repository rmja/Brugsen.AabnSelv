using Akiles.Api;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brugsen.AabnSelv.Tests;

public class AccessGadgetIntegrationTests
{
    private readonly IAkilesApiClient _client;
    private readonly IAccessGadget _accessGadget;

    public AccessGadgetIntegrationTests()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var services = new ServiceCollection()
            .AddLogging()
            .AddAkilesApi()
            .AddGadgets()
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
        _accessGadget = services.GetRequiredService<IAccessGadget>();
    }

    [Fact]
    public async Task CanGetIsMemberCheckedIn()
    {
        // Given
        const string MemberId = "mem_41eleh9x15dus6z8yqyh";

        // When
        var result = await _accessGadget.IsMemberCheckedInAsync(
            _client,
            MemberId,
            notBefore: DateTimeOffset.Now.AddHours(-1)
        );

        // Then
        Assert.False(result);
    }
}
