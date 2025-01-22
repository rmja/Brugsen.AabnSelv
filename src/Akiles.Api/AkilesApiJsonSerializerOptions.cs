using System.Text.Json;
using System.Text.Json.Serialization;

namespace Akiles.Api;

public static class AkilesApiJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Value =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters =
            {
                new JsonStringEnumConverter(
                    JsonNamingPolicy.SnakeCaseLower,
                    allowIntegerValues: false
                )
            }
        };
}
