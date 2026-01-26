using Akiles.ApiClient;
using Akiles.ApiClient.Events;
using Brugsen.AabnSelv.Coop;
using Brugsen.AabnSelv.Models;
using Brugsen.AabnSelv.Services;
using InterpolatedParsing;

namespace Brugsen.AabnSelv.Endpoints;

public static class ReportsEndpoints
{
    public static void AddRoutes(IEndpointRouteBuilder builder)
    {
        var history = builder.MapGroup("/api/reports");

        history.MapPost("/sales", BuildSalesReportAsync).DisableAntiforgery();
    }

    private static async Task<IResult> BuildSalesReportAsync(
        IFormFile coopBonOpslag,
        IAccessService accessService,
        IAkilesApiClient akilesClient,
        TimeProvider timeProvider,
        CancellationToken cancellationToken = default
    )
    {
        await using var stream = coopBonOpslag.OpenReadStream();
        using var reader = new BonOpslagReader(stream);

        var allSlips = reader.ReadAsync().ToList();
        if (allSlips.Count == 0)
        {
            return Results.BadRequest();
        }

        var firstDate = allSlips.First().Purchased.Date;
        var lastDate = allSlips.Last().Purchased.Date;

        var activities = await accessService.GetActivityAsync(
            akilesClient,
            memberId: null,
            new()
            {
                GreaterThanOrEqual = timeProvider.GetLocalDateTimeOffset(firstDate),
                LessThan = timeProvider.GetLocalDateTimeOffset(lastDate.AddDays(1)),
            },
            EventsExpand.SubjectMember,
            cancellationToken
        );

        var slipsByCoopMembershipNumber = allSlips.ToLookup(GetCoopMembershipNumber);

        var lines = new List<SalesReportLineDto>();
        foreach (var activity in activities.AsEnumerable().Reverse())
        {
            if (activity.CheckInEvent is null)
            {
                continue;
            }

            var checkedIn = timeProvider.GetLocalDateTimeOffset(activity.CheckInEvent.CreatedAt);
            var checkedOut = timeProvider.GetLocalDateTimeOffset(
                activity.CheckOutEvent?.CreatedAt ?? checkedIn.AddHours(1)
            );

            if (
                activity.CheckInEvent.SubjectMember!.Metadata.TryGetValue(
                    MetadataKeys.Member.CoopMembershipNumber,
                    out var number
                )
            )
            {
                var coopMembershipNumber = int.Parse(number);

                var slips = slipsByCoopMembershipNumber[coopMembershipNumber]
                    .Where(x =>
                        x.Purchased > checkedIn.DateTime.AddMinutes(-5)
                        && x.Purchased < checkedOut.DateTime.AddMinutes(5)
                    )
                    .ToList();

                var total = slips.Select(GetAmount).Sum();

                lines.Add(
                    new()
                    {
                        MemberId = activity.CheckInEvent.SubjectMember.Id,
                        MemberName = activity.CheckInEvent.SubjectMember.Name,
                        CoopMembershipNumber = number,
                        CheckedInAt = checkedIn,
                        CheckedOutAt = checkedOut,
                        Slips = slips,
                        TotalAmount = total,
                    }
                );
            }
            else
            {
                lines.Add(
                    new()
                    {
                        MemberId = activity.CheckInEvent.SubjectMember.Id,
                        MemberName = activity.CheckInEvent.SubjectMember.Name,
                        CoopMembershipNumber = number,
                        CheckedInAt = checkedIn,
                        CheckedOutAt = checkedOut,
                    }
                );
            }
        }
        var report = new SalesReportDto()
        {
            FirstDate = DateOnly.FromDateTime(firstDate),
            LastDate = DateOnly.FromDateTime(lastDate),
            Lines = lines,
        };
        return Results.Ok(report);
    }

    private static int GetCoopMembershipNumber(Bon bon)
    {
        foreach (var line in bon.Lines)
        {
            if (line.Category != "KUNDEKORT")
            {
                continue;
            }

            if (line.Text.StartsWith("COOP Plus"))
            {
                string stregkode = "";
                int nummer = 0;
                InterpolatedParser.Parse(
                    line.Text,
                    $"COOP Plus - Stregkode: {stregkode}. Nummer: {nummer} ()"
                );
                return nummer;
            }
            else if (line.Text.StartsWith("Freemium"))
            {
                string stregkode = "";
                int nummer = 0;
                InterpolatedParser.Parse(
                    line.Text,
                    $"Freemium - Stregkode: {stregkode}. Nummer: {nummer} ()"
                );
                return nummer;
            }
        }

        return 0;
    }

    private static decimal GetAmount(Bon bon)
    {
        foreach (var line in bon.Lines)
        {
            if (line.Category != "TOTAL")
            {
                continue;
            }

            if (line.Text.StartsWith("At betale"))
            {
                decimal value = 0;
                decimal cents = 0;
                InterpolatedParser.Parse(line.Text, $"At betale {value},{cents}");
                return value + cents / 100m;
            }
        }

        return 0;
    }
}
