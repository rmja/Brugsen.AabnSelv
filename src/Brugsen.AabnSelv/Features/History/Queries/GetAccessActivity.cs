using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Services;
using Microsoft.Extensions.Options;
using StaticEndpoints;

namespace Brugsen.AabnSelv.Features.History.Queries;

public class GetAccessActivity : IEndpoint
{
    public static void AddRoute(IEndpointRouteBuilder builder) =>
        builder.MapGet("/api/history/access-activity", HandleAsync);

    private static async Task<IResult> HandleAsync(
        DateTime? notBefore,
        IAccessService accessService,
        IAkilesApiClient client,
        IOptions<BrugsenAabnSelvOptions> options,
        TimeProvider timeProvider,
        CancellationToken cancellationToken
    )
    {
        var activity = await accessService.GetActivityAsync(
            client,
            memberId: null,
            new()
            {
                GreaterThanOrEqual = timeProvider.GetLocalDateTimeOffset(
                    notBefore ?? timeProvider.GetLocalNow().Date.AddDays(-1)
                ),
            },
            EventsExpand.SubjectMember,
            cancellationToken
        );

        return Results.Ok(activity.Select(x => x.ToDto()).ToList());
    }
}
