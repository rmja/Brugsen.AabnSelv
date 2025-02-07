using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class GadgetsExtensions
{
    public static IServiceCollection AddGadgets(this IServiceCollection services)
    {
        return services
            .AddAkilesEntity<IGadget, IAppAccessGadget, AppAccessGadget>(options =>
                options.AppAccessGadgetId
            )
            .AddAkilesEntity<IGadget, IFrontDoorGadget, FrontDoorGadget>(options =>
                options.FrontDoorGadgetId
            )
            .AddAkilesEntity<
                IGadget,
                IFrontDoorLockGadget,
                FrontDoorLockGadget,
                NoopFrontDoorLockGadget
            >(options => options.FrontDoorLockGadgetId)
            .AddAkilesEntity<IGadget, IAlarmGadget, AlarmGadget, NoopAlarmGadget>(options =>
                options.AlarmGadgetId
            )
            .AddAkilesEntity<IGadget, ILightGadget, LightGadget, NoopLightGadget>(options =>
                options.LightGadgetId
            )
            .AddAkilesEntity<
                IGadget,
                ICheckInPinpadGadget,
                CheckInPinpadGadget,
                NoopCheckInPinpadGadget
            >(options => options.CheckInPinpadGadgetId);
    }
}
