namespace GatewayApi.ApiClient.Sms;

public record SendSmsResponse
{
    public required List<long> Ids { get; init; }
}
