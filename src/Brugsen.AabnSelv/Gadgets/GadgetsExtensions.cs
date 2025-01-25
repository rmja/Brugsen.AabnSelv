using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class GadgetsExtensions
{
    public static IServiceCollection AddGadgets(this IServiceCollection services)
    {
        return services
            .AddGadget<IAccessGadget, AccessGadget>(options => options.AccessGadgetId)
            .AddGadget<IFrontDoorGadget, FrontDoorGadget>(options => options.FrontDoorGadgetId)
            .AddGadget<IFrontDoorLockGadget, FrontDoorLockGadget, NoopFrontDoorLockGadget>(
                options => options.FrontDoorLockGadgetId
            )
            .AddGadget<IAlarmGadget, AlarmGadget, NoopAlarmGadget>(options => options.AlarmGadgetId)
            .AddGadget<ILightGadget, LightGadget, NoopLightGadget>(options => options.LightGadgetId)
            .AddGadget<ICheckInPinpadGadget, CheckInPinpadGadget, NoopCheckInPinpadGadget>(
                options => options.CheckInPinpadGadgetId
            )
            .AddGadget<ICheckOutPinpadGadget, CheckOutPinpadGadget, NoopCheckOutPinpadGadget>(
                options => options.CheckOutPinpadGadgetId
            );
    }

    private static IServiceCollection AddGadget<TService, TImplementation>(
        this IServiceCollection services,
        Func<BrugsenAabnSelvOptions, string> getGadgetId
    )
        where TService : class, IGadget
        where TImplementation : class, TService
    {
        return services
            .AddSingleton<IGadget>(provider => provider.GetRequiredService<TService>())
            .AddSingleton<TService>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                var gadgetId = getGadgetId(options.Value);
                return ActivatorUtilities.CreateInstance<TImplementation>(provider, gadgetId);
            });
    }

    private static IServiceCollection AddGadget<TService, TImplementation, TNoopImplementation>(
        this IServiceCollection services,
        Func<BrugsenAabnSelvOptions, string?> getGadgetId
    )
        where TService : class, IGadget
        where TImplementation : class, TService
        where TNoopImplementation : class, TService
    {
        return services
            .AddSingleton<IGadget>(provider => provider.GetRequiredService<TService>())
            .AddSingleton<TService>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                var gadgetId = getGadgetId(options.Value);
                if (gadgetId is null)
                {
                    return ActivatorUtilities.CreateInstance<TNoopImplementation>(provider);
                }

                return ActivatorUtilities.CreateInstance<TImplementation>(provider, gadgetId);
            });
    }
}
