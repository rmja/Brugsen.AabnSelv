using System.Text.Json;

namespace GatewayApi.ApiClient;

public static class GatewayApiJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Value =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };
}
