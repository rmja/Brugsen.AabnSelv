using Akiles.Api;
using Brugsen.AabnSelv.Gadgets;

namespace Brugsen.AabnSelv;

public class AccessProcessor(
    IAccessGadget access,
    IAlarmGadget alarm,
    ILightGadget light,
    IFrontDoorLockGadget doorLock,
    IFrontDoorGadget door,
    TimeProvider timeProvider,
    [FromKeyedServices(ServiceKeys.ApiKeyClient)] IAkilesApiClient client,
    ILogger<AccessProcessor> logger
)
{
    public IAccessGadget AccessGadget => access;

    public async Task ProcessCheckInAsync(string eventId, string memberId)
    {
        logger.LogInformation("Processing check-in member {MemberId}", memberId);

        if (alarm.State != AlarmState.Disarmed)
        {
            await alarm.DisarmAsync(client, CancellationToken.None);
        }

        if (light.State != LightState.On)
        {
            await light.TurnOnAsync(client, CancellationToken.None);
        }

        if (doorLock.State != LockState.Unlocked)
        {
            await doorLock.UnlockAsync(client, CancellationToken.None);
        }

        await door.OpenOnceAsync(client, CancellationToken.None);
    }

    public async Task ProcessCheckOutAsync(string eventId, string memberId)
    {
        logger.LogInformation("Processing check-out for member {MemberId}", memberId);

        var now = timeProvider.GetLocalNow();

        // Ensure that we are checked-in, otherwise we might open the door without turning off the alarm.
        // We are actually checked out at this point, as this processing happens after the "check-out" event.
        // We therefore explicity ignore the current check-out event when determining if we are currently checked in.
        var memberIsCheckedIn = await access.IsMemberCheckedInAsync(
            client,
            memberId,
            notBefore: now.AddHours(-1),
            ignoreEventId: eventId
        );
        if (!memberIsCheckedIn)
        {
            var activity = await access.GetActivityAsync(
                client,
                memberId,
                notBefore: now.AddHours(-1)
            );
            var recentActivity = activity.FirstOrDefault();

            logger.LogWarning(
                "Member {MemberId} tried to check-out but was not checked in. Ignored event {IgnoreEventId} ({CheckOutEventIgnored}). Most recent check-in/check-out: {RecentCheckIn} / {RecentCheckOut}",
                memberId,
                eventId,
                recentActivity?.CheckOutEvent?.Id == eventId,
                recentActivity?.CheckInEvent?.CreatedAt,
                recentActivity?.CheckOutEvent?.CreatedAt
            );
            return;
        }

        logger.LogInformation("Checking-out member {MemberId}", memberId);

        await door.OpenOnceAsync(client, CancellationToken.None);

        var anyCheckedIn = await access.IsAnyCheckedInAsync(
            client,
            notBefore: timeProvider.GetLocalNow().AddHours(-1),
            CancellationToken.None
        );
        if (anyCheckedIn)
        {
            logger.LogInformation("There are other members checked-in - keep the lights on");
        }
        else
        {
            logger.LogInformation(
                "Member {MemberId} is the last checked-in - turn off the light",
                memberId
            );

            await light.TurnOffAsync(client, CancellationToken.None);
        }
    }
}
