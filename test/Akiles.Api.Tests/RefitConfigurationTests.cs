using System.Net;

namespace Akiles.Api.Tests;

public class RefitConfigurationTests
{
    [Fact]
    public async Task CorrectlySerializesQueryStringParameters()
    {
        // Given
        var fakeMessageHandler = new FakeMessageHandler();
        var httpClient = new HttpClient(fakeMessageHandler);
        var client = new AkilesApiClient(
            httpClient,
            new() { ClientId = "id", ClientSecret = "secret" }
        );

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
                        }
                    )
                    .ToListAsync()
        );

        // Then
        var request = Assert.Single(fakeMessageHandler.RequestMessages);
        Assert.Equal(
            "?limit=100&sort=created_at%3Adesc&is_deleted=any&email=email&metadata.source=hello",
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
