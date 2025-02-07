namespace Brugsen.AabnSelv;

public class BrugsenAabnSelvOptions
{
    public required string AkilesClientSecret { get; set; }
    public required string ApprovedMemberGroupId { get; set; }
    public required string AppAccessGadgetId { get; set; }
    public required string FrontDoorGadgetId { get; set; }
    public string? FrontDoorLockGadgetId { get; set; }
    public string? LightGadgetId { get; set; }
    public string? AlarmGadgetId { get; set; }
    public string? CheckInPinpadGadgetId { get; set; }
    public string? CheckOutPinpadDeviceId { get; set; }
    public required string ExtendedOpeningHoursScheduleId { get; set; }
    public string? WebhookId { get; set; }
    public string? WebhookSecret { get; set; }
}
