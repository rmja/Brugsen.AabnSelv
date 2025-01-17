﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.Api.Events;
using Akiles.Api.Gadgets;
using Akiles.Api.Members;
using Akiles.Api.Schedules;
using Refit;

namespace Akiles.Api;

public class AkilesApiClient : IAkilesApiClient
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions =
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
    private static readonly RefitSettings _refitSettings =
        new()
        {
            ContentSerializer = new SystemTextJsonContentSerializer(_jsonSerializerOptions),
            UrlParameterKeyFormatter = new ParameterKeyFormatter(),
            UrlParameterFormatter = new ParameterFormatter(),
            ExceptionFactory = (response) =>
            {
                if (response.IsSuccessStatusCode)
                {
                    return Task.FromResult<Exception?>(null);
                }

                return Task.FromResult<Exception?>(
                    new AkilesApiException()
                    {
                        RequestUri = response.RequestMessage!.RequestUri!,
                        StatusCode = response.StatusCode
                    }
                );
            }
        };

    public IEvents Events { get; }
    public IGadgets Gadgets { get; }
    public IMembers Members { get; }
    public ISchedules Schedules { get; }

    public AkilesApiClient(HttpClient httpClient, string accessToken)
    {
        httpClient.BaseAddress = new("https://api.akiles.app/v2");

        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            "Authorization",
            "Bearer " + accessToken
        );

        Events = RestService.For<IEvents>(httpClient, _refitSettings);
        Gadgets = RestService.For<IGadgets>(httpClient, _refitSettings);
        Members = RestService.For<IMembers>(httpClient, _refitSettings);
        Schedules = RestService.For<ISchedules>(httpClient, _refitSettings);
    }

    class ParameterKeyFormatter(JsonNamingPolicy namingPolicy) : IUrlParameterKeyFormatter
    {
        public ParameterKeyFormatter()
            : this(JsonNamingPolicy.SnakeCaseLower) { }

        public string Format(string key) => namingPolicy.ConvertName(key);
    }

    class ParameterFormatter(JsonNamingPolicy namingPolicy) : IUrlParameterFormatter
    {
        public IUrlParameterFormatter Next { get; set; } = new DefaultUrlParameterFormatter();

        public ParameterFormatter()
            : this(JsonNamingPolicy.SnakeCaseLower) { }

        public string? Format(object? value, ICustomAttributeProvider attributeProvider, Type type)
        {
            if (value is not null)
            {
                var valueType = value.GetType();

                if (valueType.IsEnum && valueType.IsDefined(typeof(FlagsAttribute)))
                {
                    var enumValue = (Enum)value;
                    var flags = Enum.GetValues(valueType)
                        .Cast<Enum>()
                        .Where(x => !IsDefaultValue(x))
                        .Where(enumValue.HasFlag);

                    if (!flags.Any())
                    {
                        return null;
                    }

                    var names = flags.Select(x =>
                        namingPolicy.ConvertName(Enum.GetName(valueType, x)!)
                    );
                    return string.Join(",", names);

                    static bool IsDefaultValue(Enum value) =>
                        value.Equals(Activator.CreateInstance(value.GetType()));
                }
                else if (valueType.IsEnum)
                {
                    var name = Enum.GetName(valueType, value);
                    if (name is not null)
                    {
                        return namingPolicy.ConvertName(name);
                    }
                }
            }

            return Next.Format(value, attributeProvider, type);
        }
    }
}
