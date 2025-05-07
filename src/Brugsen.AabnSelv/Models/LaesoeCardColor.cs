using System.Text.Json.Serialization;

namespace Brugsen.AabnSelv.Models;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter<LaesoeCardColor>))]
public enum LaesoeCardColor
{
    Red,
    Blue,
    Green,
}
