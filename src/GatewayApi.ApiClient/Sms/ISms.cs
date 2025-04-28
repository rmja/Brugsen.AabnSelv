namespace GatewayApi.ApiClient.Sms;

public interface ISms
{
    Task<SendSmsResponse> SendSmsAsync(
        SmsMessage message,
        CancellationToken cancellationToken = default
    );
}
