namespace Brugsen.AabnSelv.Devices;

public class NoopCheckOutPinpadDevice : ICheckOutPinpadDevice
{
    public string DeviceId { get; } = "noop-check-out-pinpad";

    public NoopCheckOutPinpadDevice(ILogger<NoopCheckOutPinpadDevice>? logger = null)
    {
        logger?.LogWarning("Using fake noop check-out pinpad");
    }
}
