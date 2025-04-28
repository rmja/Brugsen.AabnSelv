using System.Net.Http.Json;
using GatewayApi.ApiClient.Sms;

namespace GatewayApi.ApiClient;

public class GatewayApiClient : IGatewayApiClient, ISms
{
    private readonly HttpClient _httpClient;

    public ISms Sms => this;

    public GatewayApiClient(HttpClient httpClient, string token)
    {
        httpClient.BaseAddress = new("https://gatewayapi.com");
        httpClient.DefaultRequestHeaders.Authorization = new("Token", token);

        _httpClient = httpClient;
    }

    async Task<SendSmsResponse> ISms.SendSmsAsync(
        SmsMessage sms,
        CancellationToken cancellationToken
    )
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "/rest/mtsms",
            sms,
            GatewayApiJsonSerializerOptions.Value,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new GatewayApiException((int)response.StatusCode);
        }

        var result = await response.Content.ReadFromJsonAsync<SendSmsResponse>(
            GatewayApiJsonSerializerOptions.Value,
            cancellationToken
        );
        return result!;
    }
}
