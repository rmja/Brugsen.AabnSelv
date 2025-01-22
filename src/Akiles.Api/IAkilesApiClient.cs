using Akiles.Api.Events;
using Akiles.Api.Gadgets;
using Akiles.Api.Members;
using Akiles.Api.Schedules;
using Akiles.Api.Webhooks;

namespace Akiles.Api;

public interface IAkilesApiClient
{
    IEvents Events { get; }
    IGadgets Gadgets { get; }
    IMembers Members { get; }
    ISchedules Schedules { get; }
    IWebhooks Webhooks { get; }
}
