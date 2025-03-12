namespace GatewayApi.Api.Sms;

public record SmsRecipient(long Msisdn)
{
    public static implicit operator SmsRecipient(long msisdn) => new(msisdn);
}
