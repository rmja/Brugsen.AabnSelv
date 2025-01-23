﻿using Akiles.Api;

namespace Brugsen.AabnSelv.Gadgets;

public interface IFrontDoorLockGadget
{
    LockState State { get; }
    TimeSpan LockOperationDuration { get; }
    TimeSpan UnlockOperationDuration { get; }
    Task LockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
    Task UnlockAsync(IAkilesApiClient client, CancellationToken cancellationToken);
}
