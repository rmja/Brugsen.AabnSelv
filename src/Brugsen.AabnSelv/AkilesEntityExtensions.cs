using System.Diagnostics.CodeAnalysis;
using Brugsen.AabnSelv;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

internal static class AkilesEntityExtensions
{
    public static IServiceCollection AddAkilesEntity<
        TEntity,
        TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TImplementation
    >(this IServiceCollection services, Func<BrugsenAabnSelvOptions, string> getGadgetId)
        where TEntity : class, IAkilesEntity
        where TService : class, TEntity
        where TImplementation : class, TService
    {
        return services
            .AddSingleton<TEntity>(provider => provider.GetRequiredService<TService>())
            .AddSingleton<TService>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
                var gadgetId = getGadgetId(options.Value);
                return ActivatorUtilities.CreateInstance<TImplementation>(provider, gadgetId);
            });
    }

    public static IServiceCollection AddAkilesEntity<
        TEntity,
        TService,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TImplementation,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
            TNoopImplementation
    >(this IServiceCollection services, Func<BrugsenAabnSelvOptions, string?> getGadgetId)
        where TEntity : class, IAkilesEntity
        where TService : class, TEntity
        where TImplementation : class, TService
        where TNoopImplementation : class, TService
    {
        return services
            .AddSingleton<TEntity>(provider => provider.GetRequiredService<TService>())
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
