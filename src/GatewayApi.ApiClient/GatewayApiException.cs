namespace GatewayApi.ApiClient;

class GatewayApiException(int statusCode) : Exception
{
    public int StatusCode { get; } = statusCode;
}
