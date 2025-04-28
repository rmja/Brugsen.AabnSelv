using System.Collections.Concurrent;
using Akiles.ApiClient;
using GatewayApi.ApiClient;
using GatewayApi.ApiClient.Sms;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Services;

public sealed class DeviceHealthService(
    TimeProvider timeProvider,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient akilesClient,
    IGatewayApiClient gatewayApiClient,
    IOptions<BrugsenAabnSelvOptions> options,
    ILogger<DeviceHealthService> logger
) : BackgroundService, IDisposable
{
    private ITimer? _clearBatteryPercentageTimer;
    private readonly Dictionary<string, DateTimeOffset> _offlineRegistered = [];
    private readonly ConcurrentDictionary<string, double> _batteryPercentage = [];

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow();
        var tomorrowAtEight = now.AddDays(1).Date.AddHours(8);
        var due = tomorrowAtEight - now;
        _clearBatteryPercentageTimer = timeProvider.CreateTimer(
            ClearBatteryPercentage,
            null,
            due,
            TimeSpan.FromHours(24)
        );

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _clearBatteryPercentageTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60), timeProvider);

        logger.LogInformation(
            "Alerts will be sent to the following recipients: {Recipients}",
            options.Value.AlertRecipients
        );

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var devices = await akilesClient
                .Devices.ListDevicesAsync()
                .WhereAsync(x => x.HardwareId is not null, stoppingToken)
                .ToListAsync(stoppingToken);

            foreach (var device in devices)
            {
                if (!device.Status.Online)
                {
                    logger.LogWarning("Device {DeviceName} was found to be offline", device.Name);

                    if (_offlineRegistered.TryAdd(device.Id, timeProvider.GetUtcNow()))
                    {
                        await SendSmsAsync($"Ingen forbindelse til {device.Name}.", stoppingToken);
                    }
                }
                else if (_offlineRegistered.TryGetValue(device.Id, out var offlineRegistered))
                {
                    var offlineDuration = timeProvider.GetUtcNow() - offlineRegistered;
                    await SendSmsAsync(
                        $"Forbindelse genetableret til {device.Name}. Forbindelsen var afbrudt {offlineDuration}.",
                        stoppingToken
                    );
                }

                if (device.Status.BatteryPresent && device.Status.BatteryPercent < 20)
                {
                    logger.LogWarning(
                        "Battery powered device {DeviceName} was found to have battery percentage {BatteryPercent} which is below the desired threshold",
                        device.Name,
                        device.Status.BatteryPercent
                    );

                    if (_batteryPercentage.TryAdd(device.Id, device.Status.BatteryPercent))
                    {
                        await SendSmsAsync(
                            $"Batteriet på enheden {device.Name} er ved at løbe tørt og bør udskiftes. Resterende batteri: {device.Status.BatteryPercent}%.",
                            stoppingToken
                        );
                    }
                }
            }
        }
    }

    private async Task SendSmsAsync(string message, CancellationToken cancellationToken)
    {
        var recipients = options.Value.AlertRecipients.Select(x => new SmsRecipient(x)).ToList();

        logger.LogInformation(
            "Sending alert message {AlertMessage} to {RecipientCount} recipients",
            message,
            recipients.Count
        );

        await gatewayApiClient.Sms.SendSmsAsync(
            new()
            {
                Message = message,
                Sender = "AabnSelv",
                Recipients = recipients,
            },
            cancellationToken
        );
    }

    private void ClearBatteryPercentage(object? state)
    {
        _batteryPercentage.Clear();
    }

    public override void Dispose()
    {
        base.Dispose();
        _clearBatteryPercentageTimer?.Dispose();
    }
}
