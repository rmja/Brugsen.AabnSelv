using Akiles.ApiClient;

namespace Brugsen.AabnSelv.Gadgets;

public class LightGadget(string gadgetId, ILogger<LightGadget>? logger = null) : ILightGadget
{
    private readonly SemaphoreSlim _lock = new(1);

    public string GadgetId { get; } = gadgetId;
    public GadgetEntity GadgetEntity => GadgetEntity.Light;
    public LightState State { get; private set; } = LightState.Unknown;

    public async Task TurnOnAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DoTurnOnAction(client, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task TurnOffAsync(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await DoTurnOffAction(client, cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> TurnOnIfNotAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (State == LightState.On)
            {
                return false;
            }

            await DoTurnOnAction(client, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<bool> TurnOffIfNotAsync(
        IAkilesApiClient client,
        CancellationToken cancellationToken = default
    )
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (State == LightState.Off)
            {
                return false;
            }
            await DoTurnOffAction(client, cancellationToken);
            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task DoTurnOnAction(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning on the light");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.LightOn, cancellationToken);
        State = LightState.On;
    }

    private async Task DoTurnOffAction(IAkilesApiClient client, CancellationToken cancellationToken)
    {
        logger?.LogInformation("Turning off the light");
        await client.Gadgets.DoGadgetActionAsync(GadgetId, Actions.LightOff, cancellationToken);
        State = LightState.Off;
    }

    public static class Actions
    {
        public const string LightOn = "on";
        public const string LightOff = "off";
    }
}
