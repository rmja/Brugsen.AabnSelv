using System.Runtime.CompilerServices;
using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopAlarmGadget : IAlarmGadget
{
    private readonly ILogger<NoopAlarmGadget>? _logger;

    public AlarmState State { get; set; } = AlarmState.Unknown;

    public NoopAlarmGadget(ILogger<NoopAlarmGadget>? logger = null)
    {
        logger?.LogWarning("Using fake noop alarm gadget");
        _logger = logger;
    }

    public Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        _logger?.LogInformation("FAKE: Arming alarm");
        State = AlarmState.Armed;
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
