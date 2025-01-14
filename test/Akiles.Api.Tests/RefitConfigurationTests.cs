using System.Net;
using Akiles.Api.Members;

namespace Akiles.Api.Tests;

public class RefitConfigurationTests
{
    [Theory]
    [InlineData(MembersExpand.None, "")]
    [InlineData(
        MembersExpand.Cards | MembersExpand.GroupAssociations,
        "&expand=cards%2Cgroup_associations"
    )]
    public async Task CorrectlySerializesQueryStringParameters(
        MembersExpand expand,
        string expectedExpand
    )
    {
        // Given
        var fakeMessageHandler = new FakeMessageHandler();
        var httpClient = new HttpClient(fakeMessageHandler);
        var client = new AkilesApiClient(httpClient, "access-token");

        // When
        await Assert.ThrowsAsync<AkilesApiException>(
            () =>
                client
                    .Members.ListMembersAsync(
                        "created_at:desc",
                        new()
                        {
                            Email = "email",
                            IsDeleted = IsDeleted.Any,
                            Metadata = new() { ["source"] = "hello" }
                        },
                        expand
                    )
                    .ToListAsync()
        );

        // Then
        var request = Assert.Single(fakeMessageHandler.RequestMessages);
        Assert.Equal(
            "?limit=100&sort=created_at%3Adesc&is_deleted=any&email=email&metadata.source=hello"
                + expectedExpand,
            request.RequestUri?.Query
        );
    }

    class FakeMessageHandler(HttpStatusCode statusCode = HttpStatusCode.NotFound)
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> RequestMessages { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            RequestMessages.Add(request);
            return Task.FromResult(
                new HttpResponseMessage(statusCode) { RequestMessage = request }
            );
        }
    }
}
