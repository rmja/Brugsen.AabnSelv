using System.Text.Json.Serialization;
using GatewayApi.Api.JsonConverters;

namespace GatewayApi.Api.Sms;

[JsonConverter(typeof(SnakeCaseUpperJsonConverter))]
public enum SmsPriority
{
    Bulk,
    Normal,
    Urgent,
    VeryUrgent,
}
