using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.Api;
using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Controllers;
using Brugsen.AabnSelv.Endpoints;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Logging.Console;
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
builder.Services.AddHostedService<LockdownController>();
builder.Services.AddSingleton<WebhookEventValidator>();

builder.Services.AddAkilesApi();
builder.Services.AddGadgets();

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
WebhooksEndpoints.AddRoutes(app);
app.MapFallbackToFile("index.html");
app.UseDefaultFiles();
app.UseStaticFiles();
app.Run();

public partial class Program { }
