using System.Text.Json.Serialization;
using GatewayApi.ApiClient.JsonConverters;

namespace GatewayApi.ApiClient.Sms;

[JsonConverter(typeof(SnakeCaseUpperJsonConverter))]
public enum SmsPriority
{
    Bulk,
    Normal,
    Urgent,
    VeryUrgent,
}
