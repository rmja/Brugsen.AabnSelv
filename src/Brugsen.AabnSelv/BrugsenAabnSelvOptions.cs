namespace Brugsen.AabnSelv;

public class BrugsenAabnSelvOptions
{
    public required string AkilesClientSecret { get; set; }
    public required string ApprovedMemberGroupId { get; set; }
    public required string FrontDoorGadgetId { get; set; }
    public string? LightGadgetId { get; set; }
    public string? AlarmGadgetId { get; set; }
    public required string ExitKeypadDeviceId { get; set; }
}
