using Akiles.ApiClient;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Services;

namespace Brugsen.AabnSelv.Controllers;

public sealed class AccessController(
    IAccessService accessService,
    IAlarmGadget alarmGadget,
    ILightGadget lightGadget,
    IFrontDoorGadget doorGadget,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    TimeProvider timeProvider,
    ILogger<AccessController> logger
) : BackgroundService, IAccessController
{
    private ITimer? _blackoutTimer;
    private ITimer? _lockdownTimer;
    private bool _blackoutSignalled = false;
    private bool _lockdownSignalled = false;
    private readonly SemaphoreSlim _signal = new(0);

    public TimeSpan CheckoutTimeout { get; } = TimeSpan.FromHours(1);
    public TimeSpan BlackoutDelay { get; } = TimeSpan.FromSeconds(10);
    public TimeSpan LockdownDelay { get; } = TimeSpan.FromSeconds(60);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var anyCheckedIn = await accessService.IsAnyCheckedInAsync(
            client,
            notBefore: timeProvider.GetLocalNow().Subtract(CheckoutTimeout),
            CancellationToken.None
        );

        _blackoutTimer = timeProvider.CreateTimer(
            SignalBlackout,
            null,
            anyCheckedIn ? CheckoutTimeout + BlackoutDelay : TimeSpan.Zero,
            Timeout.InfiniteTimeSpan
        );
        _lockdownTimer = timeProvider.CreateTimer(
            SignalLockdown,
            null,
            anyCheckedIn ? CheckoutTimeout + LockdownDelay : TimeSpan.Zero,
            Timeout.InfiniteTimeSpan
        );

        await base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _blackoutTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        _lockdownTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await _signal
                .WaitAsync(stoppingToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

            if (_blackoutSignalled)
            {
                _blackoutSignalled = false;
                if (lightGadget.State != LightState.Off)
                {
                    try
                    {
                        await lightGadget.TurnOffAsync(client, CancellationToken.None);
                    }
                    catch (AkilesApiException ex)
                        when (ex.ErrorType == AkilesErrorTypes.HardwareOffline)
                    {
                        logger.LogError(ex, "Unable to perform blackout as hardware is offline");
                    }
                }
            }

            if (_lockdownSignalled)
            {
                _lockdownSignalled = false;

                if (alarmGadget.State != AlarmState.Armed)
                {
                    try
                    {
                        await alarmGadget.ArmAsync(client, CancellationToken.None);
                    }
                    catch (AkilesApiException ex)
                        when (ex.ErrorType == AkilesErrorTypes.HardwareOffline)
                    {
                        logger.LogError(ex, "Unable to perform lockdown as hardware is offline");
                    }
                }
            }
        }
    }

    public async Task ProcessCheckInAsync(string eventId, string memberId, bool openDoor)
    {
        logger.LogInformation(
            "Processing check-in event {EventId} for member {MemberId}",
            eventId,
            memberId
        );

        // Disarm timers by setting them to after checkout timeout
        // This effectively ensures that we always blackout and lockdown automatically for members that do not explicitly check-out
        _blackoutTimer!.Change(CheckoutTimeout + BlackoutDelay, Timeout.InfiniteTimeSpan);
        _lockdownTimer!.Change(CheckoutTimeout + LockdownDelay, Timeout.InfiniteTimeSpan);
        _blackoutSignalled = false;
        _lockdownSignalled = false;

        try
        {
            // The alarm and light might already be disarmed and on (performed by the controller),
            // but we do the actions here anyway to ensure that any internal gadget states and also logging remains correct

            if (alarmGadget.State != AlarmState.Disarmed)
            {
                await alarmGadget.DisarmAsync(client);
            }

            if (lightGadget.State != LightState.On)
            {
                await lightGadget.TurnOnAsync(client);
            }

            if (openDoor)
            {
                await doorGadget.OpenOnceAsync(client);
            }
        }
        catch (AkilesApiException ex) when (ex.ErrorType == AkilesErrorTypes.HardwareOffline)
        {
            logger.LogError(ex, "Unable to process check-in as hardware is offline");
        }
    }

    public async Task ProcessCheckOutAsync(string eventId, string memberId, bool openDoor)
    {
        logger.LogInformation(
            "Processing check-out event {EventId} for member {MemberId}",
            eventId,
            memberId
        );

        var now = timeProvider.GetLocalNow();

        if (openDoor)
        {
            // Ensure that we are checked-in, otherwise we might open the door without turning off the alarm.
            // We are actually checked out at this point, as this processing happens after the "check-out" event.
            // We therefore explicity ignore the current check-out event when determining if we are currently checked in.
            var memberIsCheckedIn = await accessService.IsMemberCheckedInAsync(
                client,
                memberId,
                notBefore: now.Subtract(CheckoutTimeout),
                ignoreEventId: eventId
            );
            if (!memberIsCheckedIn)
            {
                logger.LogWarning(
                    "Member {MemberId} tried to check-out but was not checked in. Ignored event {IgnoreEventId}",
                    memberId,
                    eventId
                );
                return;
            }

            try
            {
                logger.LogInformation(
                    "Opening door during check-out for member {MemberId}",
                    memberId
                );
                await doorGadget.OpenOnceAsync(client);
            }
            catch (AkilesApiException ex) when (ex.ErrorType == AkilesErrorTypes.HardwareOffline)
            {
                logger.LogError(ex, "Unable to process check-out as hardware is offline");
            }
        }

        var anyCheckedIn = await accessService.IsAnyCheckedInAsync(
            client,
            notBefore: timeProvider.GetLocalNow().Subtract(CheckoutTimeout),
            CancellationToken.None
        );
        if (anyCheckedIn)
        {
            logger.LogInformation("There are other members checked-in - keep the lights on");
        }
        else
        {
            logger.LogInformation(
                "Member {MemberId} is the last checked-in. Scheduling blackout and lockdown",
                memberId
            );

            _blackoutTimer!.Change(BlackoutDelay, Timeout.InfiniteTimeSpan);
            _lockdownTimer!.Change(LockdownDelay, Timeout.InfiniteTimeSpan);
        }
    }

    private void SignalBlackout(object? state)
    {
        _blackoutSignalled = true;
        _signal.Release();
    }

    private void SignalLockdown(object? state)
    {
        _lockdownSignalled = true;
        _signal.Release();
    }

    public override void Dispose()
    {
        base.Dispose();
        _blackoutTimer?.Dispose();
        _lockdownTimer?.Dispose();
    }
}
