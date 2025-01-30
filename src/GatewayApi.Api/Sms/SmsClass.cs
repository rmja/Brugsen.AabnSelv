using System.Text.Json.Serialization;
using GatewayApi.Api.JsonConverters;

namespace GatewayApi.Api.Sms;

[JsonConverter(typeof(SnakeCaseLowerJsonConverter))]
public enum SmsClass
{
    Standard,
    Premium,
    Secret,
}
