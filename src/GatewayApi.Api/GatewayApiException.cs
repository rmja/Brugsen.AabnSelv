namespace GatewayApi.Api;

class GatewayApiException(int statusCode) : Exception
{
    public int StatusCode { get; } = statusCode;
}
