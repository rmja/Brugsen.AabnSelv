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
    public Schedule ExtendedSchedule { get; set; } = null!;

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadSchedulesAsync();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextMidnight = timeProvider.GetLocalNow().AddDays(1).Date;
            var duration = nextMidnight - timeProvider.GetLocalNow();
            await Task.Delay(duration, timeProvider, stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await LoadSchedulesAsync();
        }
    }

    private async Task LoadSchedulesAsync()
    {
        ExtendedSchedule = await client.Schedules.GetScheduleAsync(
            options.Value.ExtendedOpeningHoursScheduleId
        );

        logger.LogInformation(
            "Extended schedule {ExtendedSchedule} was loaded",
            ExtendedSchedule.Name
        );
    }
}
