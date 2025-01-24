using System.Runtime.CompilerServices;
using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopAlarmGadget : IAlarmGadget
{
    private readonly ILogger<NoopAlarmGadget>? _logger;

    public string GadgetId { get; } = "noop-alarm";
    public GadgetEntity GadgetEntity => GadgetEntity.Alarm;
    public AlarmState State { get; set; } = AlarmState.Unknown;
    public DateTime? LastArmed { get; set; }

    public NoopAlarmGadget(ILogger<NoopAlarmGadget>? logger = null)
    {
        logger?.LogWarning("Using fake noop alarm gadget");
        _logger = logger;
    }

    public Task<DateTime?> GetLastArmedAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken
    ) => Task.FromResult(LastArmed);

    public Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Arming alarm");
        State = AlarmState.Armed;
        LastArmed = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    public Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Disarming alarm");
        State = AlarmState.Disarmed;
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<Event> GetRecentEventsAsync(
        IAkilesApiClient client,
        DateTimeOffset notBefore,
        EventsExpand expand,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await Task.CompletedTask;
        yield break;
    }
}
