using Akiles.Api;
using Akiles.Api.Schedules;
using Microsoft.Extensions.Options;

namespace Brugsen.AabnSelv.Services;

public class OpeningHoursService(
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    IOptions<BrugsenAabnSelvOptions> options,
    ILogger<OpeningHoursService> logger
) : BackgroundService, IOpeningHoursService
{
    public Schedule RegularSchedule { get; set; } = null!;
    public Schedule ExtendedSchedule { get; set; } = null!;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadSchedulesAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextMidnight = timeProvider.GetLocalNow().AddDays(1).Date;
            var duration = nextMidnight - timeProvider.GetLocalNow();
            await Task.Delay(duration, timeProvider, stoppingToken);

            await LoadSchedulesAsync(stoppingToken);
        }
    }

    private async Task LoadSchedulesAsync(CancellationToken cancellationToken)
    {
        RegularSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.RegularOpeningHoursScheduleId,
            cancellationToken
        );
        ExtendedSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.ExtendedOpeningHoursScheduleId,
            cancellationToken
        );

        logger.LogInformation(
            "Regular schedule {RegularSchedule} and extended schedule {ExtendedSchedule} was loaded",
            RegularSchedule.Name,
            ExtendedSchedule.Name
        );
    }

    public AccessMode GetAccessMode(DateTimeOffset? at = null)
    {
        var time = at.HasValue
            ? timeProvider.GetLocalDateTimeOffset(at.Value)
            : timeProvider.GetLocalNow();
        var regularPeriod = RegularSchedule.GetCurrentPeriod(time.DateTime);
        if (regularPeriod is not null)
        {
            return AccessMode.RegularAccess;
        }

        var extendedPeriod = ExtendedSchedule.GetCurrentPeriod(time.DateTime);
        if (extendedPeriod is not null)
        {
            return AccessMode.ExtendedAccess;
        }

        return AccessMode.NoAccess;
    }
}
