using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayApi.Api.JsonConverters;

internal class SnakeCaseUpperJsonConverter : JsonStringEnumConverter
{
    public SnakeCaseUpperJsonConverter()
        : base(JsonNamingPolicy.SnakeCaseUpper, allowIntegerValues: false) { }
}
