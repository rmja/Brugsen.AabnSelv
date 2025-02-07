using Brugsen.AabnSelv.Devices;

namespace Microsoft.Extensions.DependencyInjection;

public static class DevicesExtensions
{
    public static IServiceCollection AddDevices(this IServiceCollection services)
    {
        return services.AddAkilesEntity<
            IDevice,
            ICheckOutPinpadDevice,
            CheckOutPinpadDevice,
            NoopCheckOutPinpadDevice
        >(options => options.CheckOutPinpadDeviceId);
    }
}
