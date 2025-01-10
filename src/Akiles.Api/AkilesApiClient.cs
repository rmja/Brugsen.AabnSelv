using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.Api.Members;
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
            UrlParameterKeyFormatter = new ParameterKeyFormatter(JsonNamingPolicy.SnakeCaseLower),
            UrlParameterFormatter = new StringEnumUrlParameterFormatter(
                new DefaultUrlParameterFormatter(),
                JsonNamingPolicy.SnakeCaseLower
            ),
            ExceptionFactory = (response) =>
            {
                if (response.IsSuccessStatusCode)
                {
                    return Task.FromResult<Exception?>(null);
                }

                return Task.FromResult<Exception?>(
                    new AkilesApiException() { StatusCode = response.StatusCode }
                );
            }
        };

    public IMembers Members { get; }

    public AkilesApiClient(HttpClient httpClient, AkilesApiOptions options)
    {
        httpClient.BaseAddress = new("https://api.akiles.app/v2");

        Members = RestService.For<IMembers>(httpClient, _refitSettings);
    }

    class ParameterKeyFormatter(JsonNamingPolicy namingPolicy) : IUrlParameterKeyFormatter
    {
        public string Format(string key) => namingPolicy.ConvertName(key);
    }

    class StringEnumUrlParameterFormatter(
        IUrlParameterFormatter next,
        JsonNamingPolicy namingPolicy
    ) : IUrlParameterFormatter
    {
        public string? Format(object? value, ICustomAttributeProvider attributeProvider, Type type)
        {
            if (value is not null)
            {
                var valueType = value.GetType();
                if (valueType.IsEnum)
                {
                    var name = Enum.GetName(valueType, value);
                    if (name is not null)
                    {
                        return namingPolicy.ConvertName(name);
                    }
                }
            }

            return next.Format(value, attributeProvider, type);
        }
    }
}
