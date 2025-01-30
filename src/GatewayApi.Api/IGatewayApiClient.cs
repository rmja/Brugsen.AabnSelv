using GatewayApi.Api.Sms;

namespace GatewayApi.Api;

public interface IGatewayApiClient
{
    ISms Sms { get; }
}
