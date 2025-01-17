using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.Api;
using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Endpoints;
using Brugsen.AabnSelv.Gadgets;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<BrugsenAabnSelvOptions>(builder.Configuration);
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    )
);

builder.Services.AddSingleton<TimeProvider, DanishTimeProvider>();
builder.Services.AddHostedService<AlarmController>();
builder.Services.AddHostedService<LightController>();

builder.Services.AddSingleton<IFrontDoorGadget>(provider =>
{
    var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
    return ActivatorUtilities.CreateInstance<FrontDoorGadget>(
        provider,
        options.Value.FrontDoorGadgetId
    );
});
builder.Services.AddSingleton<IAlarmGadget>(provider =>
{
    var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
    return options.Value.AlarmGadgetId is not null
        ? ActivatorUtilities.CreateInstance<AlarmGadget>(provider, options.Value.AlarmGadgetId)
        : ActivatorUtilities.CreateInstance<NoopAlarmGadget>(provider);
});
builder.Services.AddSingleton<ILightGadget>(provider =>
{
    var options = provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>();
    return options.Value.LightGadgetId is not null
        ? ActivatorUtilities.CreateInstance<LightGadget>(provider, options.Value.LightGadgetId)
        : ActivatorUtilities.CreateInstance<NoopLightGadget>(provider);
});

builder.Services.AddAkilesApi();

builder
    .Services.AddKeyedSingleton(
        ServiceKeys.ApiKeyClient,
        (services, _) =>
        {
            var factory = services.GetRequiredService<IAkilesApiClientFactory>();
            var apiKey = builder.Configuration["AkilesApiKey"]!;
            return factory.Create(apiKey);
        }
    )
    .AddScoped(services =>
    {
        var factory = services.GetRequiredService<IAkilesApiClientFactory>();
        var request = services.GetRequiredService<IHttpContextAccessor>();
        var authorizationHeader = request
            .HttpContext!.Request.Headers[HeaderNames.Authorization]
            .Single()!;
        var accessToken = authorizationHeader.Replace("Bearer ", "");
        return factory.Create(accessToken);
    })
    .AddHttpContextAccessor();

var app = builder.Build();
app.UsePathBase(app.Configuration["PathBase"]);
app.UseRouting(); // Must be called explicitly for PathBase to have effect, see https://andrewlock.net/using-pathbase-with-dotnet-6-webapplicationbuilder/#option-1-controlling-the-location-of-userouting-
HistoryEndpoints.AddRoutes(app);
MembersEndpoints.AddRoutes(app);
OAuthEndpoints.AddRoutes(app);
app.MapFallbackToFile("index.html");
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();
