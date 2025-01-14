using Akiles.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

public static class AkilesApiExtensions
{
    public static IServiceCollection AddAkilesApi(
        this IServiceCollection services,
        Action<AkilesApiOptions>? configureOptions = null
    )
    {
        services.AddHttpClient<AkilesApiClient>();

        services.TryAddSingleton<IAkilesApiClientFactory, AkilesApiClientFactory>();

        if (configureOptions is not null)
        {
            services.TryAddTransient<IAkilesApiClient>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AkilesApiOptions>>();
                var apiKey =
                    options.Value.ApiKey
                    ?? throw new InvalidOperationException(
                        "No API key configured. Use IAkilesApiClientFactory instead to create a client from an access token"
                    );
                return ActivatorUtilities.CreateInstance<AkilesApiClient>(provider, apiKey);
            });

            services.Configure(configureOptions);
        }

        return services;
    }
}
