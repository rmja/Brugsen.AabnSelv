using Refit;

namespace Akiles.Api.Devices;

public interface IDevices
{
    [Get("/devices")]
    Task<PagedList<Device>> ListDevicesAsync(
        string? cursor,
        int? limit,
        string? sort,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<Device> ListDevicesAsync(string? sort = null) =>
        new PaginationEnumerable<Device>(
            (cursor, cancellationToken) =>
                ListDevicesAsync(cursor, Constants.DefaultPaginationLimit, sort, cancellationToken)
        );

    [Get("/devices/{deviceId}")]
    Task<Device> GetDeviceAsync(string deviceId, CancellationToken cancellationToken = default);
}
