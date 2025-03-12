using Akiles.Api.Devices;
using Akiles.Api.Events;
using Akiles.Api.Gadgets;
using Akiles.Api.MemberGroups;
using Akiles.Api.Members;
using Akiles.Api.Schedules;
using Akiles.Api.Webhooks;

namespace Akiles.Api;

public interface IAkilesApiClient
{
    IDevices Devices { get; }
    IEvents Events { get; }
    IGadgets Gadgets { get; }
    IMembers Members { get; }
    IMemberGroups MemberGroups { get; }
    ISchedules Schedules { get; }
    IWebhooks Webhooks { get; }
}
