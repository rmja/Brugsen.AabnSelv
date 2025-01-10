using Microsoft.Extensions.DependencyInjection;

namespace Akiles.Api.Tests;

public class MembersTests
{
    private readonly IAkilesApiClient _client;

    public MembersTests()
    {
        var services = new ServiceCollection()
            .AddAkilesApi(options =>
            {
                options.ClientId = TestSecrets.ClientId;
                options.ClientSecret = TestSecrets.ClientSecret;
            })
            .BuildServiceProvider();
        _client = services.GetRequiredService<IAkilesApiClient>();
    }

    [Fact]
    public async Task CanListMembers()
    {
        // Given

        // When
        var members = await _client.Members.ListMembersAsync().ToListAsync();

        // Then
        Assert.Equal(2, members.Count);
    }
}
