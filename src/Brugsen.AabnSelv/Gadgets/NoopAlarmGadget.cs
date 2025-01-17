using System.Runtime.CompilerServices;
using Akiles.Api;
using Akiles.Api.Events;

namespace Brugsen.AabnSelv.Gadgets;

public class NoopAlarmGadget : IAlarmGadget
{
    private readonly ILogger<NoopAlarmGadget> _logger;

    public AlarmGadgetArmState ArmState { get; private set; } = AlarmGadgetArmState.Unknown;

    public NoopAlarmGadget(ILogger<NoopAlarmGadget> logger)
    {
        logger.LogWarning("Using fake noop alarm gadget");

        _logger = logger;
    }

    public Task ArmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        lock (this)
        {
            _logger.LogInformation("FAKE: Arming alarm");
            ArmState = AlarmGadgetArmState.Armed;
            return Task.CompletedTask;
        }
    }

    public Task DisarmAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        lock (this)
        {
            _logger.LogInformation("FAKE: Disarming alarm");
            ArmState = AlarmGadgetArmState.Disarmed;
            return Task.CompletedTask;
        }
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
