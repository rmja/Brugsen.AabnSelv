namespace GatewayApi.ApiClient.Sms;

public record SmsMessage
{
    public SmsClass Class { get; init; } = SmsClass.Standard;
    public required string Message { get; init; }
    public required string Sender { get; init; }
    public SmsPriority Priority { get; init; } = SmsPriority.Normal;
    public List<SmsRecipient> Recipients { get; init; } = [];
}
