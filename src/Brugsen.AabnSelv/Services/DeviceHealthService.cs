using Akiles.Api;
using GatewayApi.Api;
using GatewayApi.Api.Sms;
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
    private ITimer? _clearThrottleTimer;
    private readonly HashSet<string> _throttle = [];

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetLocalNow();
        var startOfNextHour = now.AddHours(1).AddTicks(-now.Ticks % TimeSpan.TicksPerHour);
        var due = startOfNextHour - now;
        _clearThrottleTimer = timeProvider.CreateTimer(
            ClearThrottle,
            null,
            due,
            TimeSpan.FromHours(1)
        );

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _clearThrottleTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60), timeProvider);

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
                    await SendAlertAsync(
                        device.Id,
                        $"Ingen forbindelse til {device.Name}",
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
                    await SendAlertAsync(
                        device.Id,
                        $"Batteriet på enheden {device.Name} er ved at løbe tørt og skal udskiftes, resterende batteri: {device.Status.BatteryPercent}%",
                        stoppingToken
                    );
                }
            }
        }
    }

    private async Task SendAlertAsync(
        string deviceId,
        string message,
        CancellationToken cancellationToken
    )
    {
        lock (_throttle)
        {
            if (_throttle.Contains(deviceId))
            {
                return;
            }
        }

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

        lock (_throttle)
        {
            _throttle.Add(deviceId);
        }
    }

    private void ClearThrottle(object? state)
    {
        lock (_throttle)
        {
            _throttle.Clear();
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _clearThrottleTimer?.Dispose();
    }
}
