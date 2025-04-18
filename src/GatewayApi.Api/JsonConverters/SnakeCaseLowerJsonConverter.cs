﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayApi.Api.JsonConverters;

internal class SnakeCaseLowerJsonConverter : JsonStringEnumConverter
{
    public SnakeCaseLowerJsonConverter()
        : base(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false) { }
}
