using System.Text.Json;

namespace GatewayApi.Api;

public static class GatewayApiJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Value =
        new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower, };
}
