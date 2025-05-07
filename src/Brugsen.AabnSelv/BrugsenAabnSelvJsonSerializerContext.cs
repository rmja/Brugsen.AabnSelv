using System.Text.Json;
using System.Text.Json.Serialization;
using Brugsen.AabnSelv.Models;
using Microsoft.AspNetCore.Mvc;

namespace Brugsen.AabnSelv;

[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(AccessActivityDto))]
[JsonSerializable(typeof(ActionEventDto))]
[JsonSerializable(typeof(List<AccessActivityDto>))]
[JsonSerializable(typeof(List<ActionEventDto>))]
[JsonSerializable(typeof(List<MemberDto>))]
[JsonSerializable(typeof(MemberDto))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class BrugsenAabnSelvJsonSerializerContext : JsonSerializerContext;

class CamelCaseJsonStringEnumConverter<TEnum>()
    : JsonStringEnumConverter<TEnum>(JsonNamingPolicy.CamelCase)
    where TEnum : struct, Enum;
