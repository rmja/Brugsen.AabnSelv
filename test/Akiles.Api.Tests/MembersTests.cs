using Akiles.Api.Members;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Akiles.Api.Tests;

public class MembersTests
{
    private readonly IAkilesApiClient _client;

    public MembersTests()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<MembersTests>().Build();
        var services = new ServiceCollection()
            .AddAkilesApi(options =>
            {
                options.ApiKey = config["AkilesApiKey"];
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
        Assert.NotEmpty(members);
    }

    [Fact]
    public async Task CanListMembersWithExpand()
    {
        // Given

        // When
        var members = await _client
            .Members.ListMembersAsync(expand: MembersExpand.Emails)
            .ToListAsync();

        // Then
        Assert.NotEmpty(members);
        Assert.Contains(members, x => x.Emails is not null);
    }
}
