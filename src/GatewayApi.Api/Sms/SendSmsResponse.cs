namespace GatewayApi.Api.Sms;

public record SendSmsResponse
{
    public required List<long> Ids { get; init; }
}
