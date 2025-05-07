using System.Text.Json.Serialization;

namespace Brugsen.AabnSelv.Endpoints;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter<AccessMethod>))]
public enum AccessMethod
{
    App,
    Pin,
    Nfc,
}
