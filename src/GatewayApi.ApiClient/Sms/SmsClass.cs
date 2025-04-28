using System.Text.Json.Serialization;
using GatewayApi.ApiClient.JsonConverters;

namespace GatewayApi.ApiClient.Sms;

[JsonConverter(typeof(SnakeCaseLowerJsonConverter))]
public enum SmsClass
{
    Standard,
    Premium,
    Secret,
}
