using GatewayApi.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

public static class GatewayApiExtensions
{
    public static IServiceCollection AddGatewayApi(
        this IServiceCollection services,
        Action<GatewayApiOptions> configureOptions
    )
    {
        services.AddHttpClient<GatewayApiClient>();
        services.TryAddTransient<IGatewayApiClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<GatewayApiOptions>>();
            var token = options.Value.Token;
            return ActivatorUtilities.CreateInstance<GatewayApiClient>(provider, token);
        });

        services.Configure(configureOptions);
        return services;
    }
}
