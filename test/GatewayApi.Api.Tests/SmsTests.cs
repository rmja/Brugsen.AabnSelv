using System.Text.Json;
using GatewayApi.Api.Sms;

namespace GatewayApi.Api.Tests;

public class SmsTests(ApiFixture fixture) : IClassFixture<ApiFixture>
{
    private readonly IGatewayApiClient _client = fixture.Client;

    [Fact]
    public void CanSerializeSmsMessage()
    {
        // Given
        var sms = new SmsMessage
        {
            Class = SmsClass.Premium,
            Message = "Test 123",
            Sender = "Brugsen",
            Priority = SmsPriority.Urgent,
            Recipients = { new(4520285909) }
        };

        // When
        var json = JsonSerializer.Serialize(sms, GatewayApiJsonSerializerOptions.Value);

        // Then
        Assert.Equal(
            "{\"class\":\"premium\",\"message\":\"Test 123\",\"sender\":\"Brugsen\",\"priority\":\"URGENT\",\"recipients\":[{\"msisdn\":4520285909}]}",
            json
        );
    }

    [Fact(Skip = "Avoid sms spam")]
    public async Task CanSendSmsMessage()
    {
        // Given
        var sms = new SmsMessage
        {
            Message = "Gateway API test",
            Sender = "Brugsen",
            Recipients = { new(4520285909) }
        };

        // When
        var response = await _client.Sms.SendSmsAsync(sms);

        // Then
        Assert.NotEqual(0, Assert.Single(response.Ids));
    }
}
