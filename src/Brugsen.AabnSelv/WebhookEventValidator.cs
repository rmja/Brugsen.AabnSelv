using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Akiles.Api;
using Akiles.Api.Events;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv;

internal class WebhookEventValidator(
    IOptions<BrugsenAabnSelvOptions> options,
    ILogger<WebhookEventValidator> logger
)
{
    public async Task<Event?> ReadSignedEventOrNullAsync(
        HttpRequest request,
        CancellationToken cancellationToken
    )
    {
        var keyHex = options.Value.WebhookSecret;
        if (keyHex is null)
        {
            logger.LogWarning("No webhook secret provided - webhook signature is not validate!");
            return await JsonSerializer.DeserializeAsync<Event>(
                request.Body,
                AkilesApiJsonSerializerOptions.Value,
                cancellationToken
            );
        }
        var key = Encoding.UTF8.GetBytes(keyHex);

        var expectedSignatureHex = request.Headers["x-akiles-sig-sha256"].FirstOrDefault();
        if (expectedSignatureHex is null)
        {
            logger.LogWarning("No signature found in webhook request");
            return null;
        }
        var expectedSignature = Convert.FromHexString(expectedSignatureHex);

        var (evnt, actualSignature) = await DeserializeWithSignatureAsync<Event>(
            request.Body,
            key,
            cancellationToken
        );

        if (!actualSignature.SequenceEqual(expectedSignature))
        {
            logger.LogWarning(
                "Signature mismatch! Expected: {ExpectedSignature}, actual: {ActualSignature}",
                Convert.ToHexString(expectedSignature),
                Convert.ToHexString(actualSignature)
            );
            return null;
        }

        logger.LogDebug("Webhook event was found to be correctly signed");

        return evnt;
    }

    private static async Task<(T? Value, byte[] Signature)> DeserializeWithSignatureAsync<T>(
        Stream utf8Stream,
        byte[] key,
        CancellationToken cancellationToken = default
    )
    {
        using var lease = await utf8Stream.ReadToMemoryAsync(cancellationToken);
        var signature = HMACSHA256.HashData(key, lease.Memory.Span);
        var value = JsonSerializer.Deserialize<T>(
            lease.Memory.Span,
            AkilesApiJsonSerializerOptions.Value
        );
        return (value, signature);
    }
}
