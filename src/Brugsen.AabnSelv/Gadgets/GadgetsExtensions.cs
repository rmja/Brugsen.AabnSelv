using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class GadgetsExtensions
{
    public static IServiceCollection AddGadgets(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAccessGadget>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                return ActivatorUtilities.CreateInstance<AccessGadget>(
                    provider,
                    options.Value.AccessGadgetId
                );
            })
            .AddSingleton<IFrontDoorGadget>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                return ActivatorUtilities.CreateInstance<FrontDoorGadget>(
                    provider,
                    options.Value.FrontDoorGadgetId
                );
            })
            .AddSingleton<IFrontDoorLockGadget>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                return options.Value.FrontDoorLockGadgetId is not null
                    ? ActivatorUtilities.CreateInstance<FrontDoorLockGadget>(
                        provider,
                        options.Value.FrontDoorLockGadgetId
                    )
                    : ActivatorUtilities.CreateInstance<NoopFrontDoorLockGadget>(provider);
            })
            .AddSingleton<IAlarmGadget>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                return options.Value.AlarmGadgetId is not null
                    ? ActivatorUtilities.CreateInstance<AlarmGadget>(
                        provider,
                        options.Value.AlarmGadgetId
                    )
                    : ActivatorUtilities.CreateInstance<NoopAlarmGadget>(provider);
            })
            .AddSingleton<ILightGadget>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                return options.Value.LightGadgetId is not null
                    ? ActivatorUtilities.CreateInstance<LightGadget>(
                        provider,
                        options.Value.LightGadgetId
                    )
                    : ActivatorUtilities.CreateInstance<NoopLightGadget>(provider);
            });
    }
}
