using Akiles.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class AkilesApiExtensions
{
    public static IServiceCollection AddAkilesApi(
        this IServiceCollection services,
        Action<AkilesApiOptions> configureOptions
    )
    {
        services.AddHttpClient<AkilesApiClient>();
        services.TryAddTransient<IAkilesApiClient>(provider =>
        {
            var options = provider.GetRequiredService<AkilesApiOptions>();
            return ActivatorUtilities.CreateInstance<AkilesApiClient>(provider, options);
        });
        services.Configure(configureOptions);
        return services;
    }
}
