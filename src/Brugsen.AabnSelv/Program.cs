using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.ApiClient;
using Akiles.ApiClient.Webhooks;
using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Endpoints;
using Brugsen.AabnSelv.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateSlimBuilder(args);

// Use a reasonable console logger that includes timestamp
// This is useful when inspecting raw console output in kubernetes
builder.Logging.AddSimpleConsole(options =>
{
    options.ColorBehavior = LoggerColorBehavior.Enabled;
    options.SingleLine = false;
    options.TimestampFormat = "yyyy-MM-dd'T'HH:mm:ss ";
});

builder.Services.Configure<BrugsenAabnSelvOptions>(builder.Configuration);
builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    )
);

builder.Services.AddSingleton<TimeProvider, DanishTimeProvider>();

builder.Services.AddSingleton<IAccessService, AccessService>();
builder.Services.AddSingleton<OpeningHoursService>();
builder.Services.AddSingleton<IOpeningHoursService>(provider =>
    provider.GetRequiredService<OpeningHoursService>()
);
builder.Services.AddSingleton<IHostedService>(provider =>
    provider.GetRequiredService<OpeningHoursService>()
);

builder.Services.AddSingleton<AccessController>();
builder.Services.AddSingleton<IAccessController>(provider =>
    provider.GetRequiredService<AccessController>()
);
builder.Services.AddSingleton<IHostedService>(provider =>
    provider.GetRequiredService<AccessController>()
);

builder.Services.AddHostedService<LockdownController>();

builder.Services.AddHostedService<DeviceHealthService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IWebhookBinder, NoValidationWebhookEventBinder>();
}
else
{
    builder.Services.AddSingleton<IWebhookBinder>(provider =>
    {
        var webhookSecret =
            provider.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>().Value.WebhookSecret
            ?? throw new Exception("No webhook secret configured");
        return ActivatorUtilities.CreateInstance<WebhookEventBinder>(provider, webhookSecret);
    });
}

builder.Services.AddAkilesApiClient();
builder.Services.AddGatewayApiClient(options =>
    options.Token = builder.Configuration["GatewayApiToken"]!
);
builder.Services.AddGadgets().AddDevices();

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

var options = app.Services.GetRequiredService<IOptions<BrugsenAabnSelvOptions>>().Value;
if (options.CheckInPinpadGadgetId is not null)
{
    var client = app.Services.GetRequiredKeyedService<IAkilesApiClient>(ServiceKeys.ApiKeyClient);
    var group = await client.MemberGroups.GetMemberGroupAsync(options.ApprovedMemberGroupId);
    var permission = group.Permissions.FirstOrDefault(x =>
        x.GadgetId == options.CheckInPinpadGadgetId
    );
    if (permission?.AccessMethods is not null)
    {
        if (permission.AccessMethods.Pin)
        {
            app.Logger.LogWarning(
                "External facing check-in pinpad accepts insecure phone number pins"
            );
        }
    }
}

app.UsePathBase(app.Configuration["PathBase"]);
app.UseRouting(); // Must be called explicitly for PathBase to have effect, see https://andrewlock.net/using-pathbase-with-dotnet-6-webapplicationbuilder/#option-1-controlling-the-location-of-userouting-
HistoryEndpoints.AddRoutes(app);
MembersEndpoints.AddRoutes(app);
OAuthEndpoints.AddRoutes(app);
WebhooksEndpoints.AddRoutes(app);
app.MapWhen(
    x => !x.Request.Path.StartsWithSegments("/api"),
    notApi =>
        notApi
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseRouting()
            .UseEndpoints(endpoints => endpoints.MapFallbackToFile("index.html"))
);
app.Run();

public partial class Program { }
