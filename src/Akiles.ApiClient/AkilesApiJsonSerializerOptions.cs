﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace Akiles.ApiClient;

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
                ),
                new TimeOnlyJsonConverter()
            }
        };
}

file class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        var seconds = reader.GetInt32();
        return new TimeOnly(seconds * TimeSpan.TicksPerSecond);
    }

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        var seconds = value.Ticks / TimeSpan.TicksPerSecond;
        writer.WriteNumberValue(seconds);
    }
}
