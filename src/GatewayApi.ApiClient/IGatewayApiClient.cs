using GatewayApi.ApiClient.Sms;

namespace GatewayApi.ApiClient;

public interface IGatewayApiClient
{
    ISms Sms { get; }
}
