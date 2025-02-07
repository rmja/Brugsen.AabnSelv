namespace Brugsen.AabnSelv.Devices;

public class CheckOutPinpadDevice(string deviceId) : ICheckOutPinpadDevice
{
    public string DeviceId { get; } = deviceId;
}
