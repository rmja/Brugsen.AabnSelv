using System.Text.Json;
using System.Text.Json.Serialization;
using Akiles.Api;
using Akiles.Api.Members;
using Brugsen.AabnSelv;
using Brugsen.AabnSelv.Models;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

const string ApiKeyClient = "ApiKeyClient";

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<BrugsenAabnSelvOptions>(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
    )
);
builder.Services.AddAkilesApi();

builder
    .Services.AddKeyedSingleton(
        ApiKeyClient,
        (services, _) =>
        {
            var factory = services.GetRequiredService<IAkilesApiClientFactory>();
            var apiKey = builder.Configuration["AkilesApiKey"]!;
            return factory.Create(apiKey);
        }
    )
    .AddScoped(services =>
    {
        var factory = services.GetRequiredService<IAkilesApiClientFactory>();
        var request = services.GetRequiredService<IHttpContextAccessor>();
        var authorizationHeader = request
            .HttpContext!.Request.Headers[HeaderNames.Authorization]
            .Single()!;
        var accessToken = authorizationHeader.Replace("Bearer ", "");
        return factory.Create(accessToken);
    })
    .AddHttpContextAccessor();

var app = builder.Build();
app.UsePathBase(app.Configuration["PathBase"]);
app.UseRouting(); // Must be called explicitly for PathBase to have effect, see https://andrewlock.net/using-pathbase-with-dotnet-6-webapplicationbuilder/#option-1-controlling-the-location-of-userouting-

app.MapGet(
    "/oauth2/auth",
    (HttpRequest request) =>
    {
        return Results.Redirect("https://auth.akiles.app/oauth2/auth" + request.QueryString);
    }
);
app.MapPost(
    "/oauth2/token",
    async (
        HttpRequest request,
        HttpResponse response,
        HttpClient httpClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    ) =>
    {
        var form = await request.ReadFormAsync(cancellationToken);
        var entries = form.Select(x => KeyValuePair.Create(x.Key, x.Value.First()!)).ToList();
        entries.Add(KeyValuePair.Create("client_secret", options.Value.AkilesClientSecret));
        var content = new FormUrlEncodedContent(entries);
        var result = await httpClient.PostAsync(
            "https://auth.akiles.app/oauth2/token",
            content,
            cancellationToken
        );

        await CopyToResponseAsync(result, response, cancellationToken);
    }
);

app.MapPost(
    "/api/members/signup",
    async (
        MemberDto init,
        [FromKeyedServices(ApiKeyClient)] IAkilesApiClient apiClient,
        CancellationToken cancellationToken
    ) =>
    {
        var existingMemberByEmail = await apiClient
            .Members.ListMembersAsync(null, new ListMembersFilter() { Email = init.Email })
            .FirstOrDefaultAsync(cancellationToken);
        if (existingMemberByEmail is not null)
        {
            return Results.Problem(
                type: "api://members/signup/email-conflict",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var existingMemberByCard = await apiClient
            .Members.ListMembersAsync(
                null,
                new ListMembersFilter()
                {
                    Metadata = new()
                    {
                        [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber.ToString()
                    }
                }
            )
            .FirstOrDefaultAsync(cancellationToken);
        if (existingMemberByCard is not null)
        {
            return Results.Problem(
                type: "api://members/signup/laesoe-card-number-conflict",
                statusCode: StatusCodes.Status409Conflict
            );
        }

        var member = await apiClient.Members.CreateMemberAsync(
            new()
            {
                Name = init.Name,
                Metadata =
                {
                    [MetadataKeys.Member.Phone] = init.Phone,
                    [MetadataKeys.Member.Address] = init.Address,
                    [MetadataKeys.Member.CoopMembershipNumber] =
                        init.CoopMembershipNumber.ToString(),
                    [MetadataKeys.Member.LaesoeCardNumber] = init.LaesoeCardNumber.ToString(),
                    [MetadataKeys.Member.LaesoeCardColor] = init
                        .LaesoeCardColor.ToString()
                        .ToLowerInvariant()
                }
            },
            cancellationToken
        );

        var email = await apiClient.Members.CreateEmailAsync(
            member.Id,
            new() { Email = init.Email },
            CancellationToken.None
        );

        return Results.Ok(member.ToDto(email, isApproved: false));
    }
);

app.MapGet(
    "/api/members/approved",
    async (
        [FromKeyedServices(ApiKeyClient)] IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    ) =>
    {
        var members = await apiClient
            .Members.ListMembersAsync(
                filter: new() { IsDeleted = IsDeleted.False },
                expand: MembersExpand.GroupAssociations | MembersExpand.Emails
            )
            .WhereAsync(
                x => x.IsApproved(options.Value) && x.Emails?.Any() == true,
                cancellationToken
            )
            .ToListAsync(cancellationToken);
        return Results.Ok(members.Select(x => x.ToDto(x.Emails!.First(), isApproved: true)));
    }
);

app.MapGet(
    "/api/members/pending-approval",
    async (
        [FromKeyedServices(ApiKeyClient)] IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    ) =>
    {
        var members = await apiClient
            .Members.ListMembersAsync(
                filter: new() { IsDeleted = IsDeleted.False },
                expand: MembersExpand.GroupAssociations | MembersExpand.Emails
            )
            .WhereAsync(
                x => !x.IsApproved(options.Value) && x.Emails?.Any() == true,
                cancellationToken
            )
            .ToListAsync(cancellationToken);
        return Results.Ok(members.Select(x => x.ToDto(x.Emails!.First(), isApproved: false)));
    }
);

app.MapGet(
    "/api/members/{memberId}",
    async (
        string memberId,
        IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    ) =>
    {
        var member = await apiClient.Members.GetMemberAsync(
            memberId,
            //MembersExpand.Emails | MembersExpand.GroupAssociations,
            cancellationToken
        );
        if (member.IsDeleted)
        {
            return Results.NotFound();
        }

        //return Results.Ok(member.ToDto(member.Emails!.First(), member.GroupAssociations));

        var email = await apiClient.Members.ListEmailsAsync(memberId).FirstAsync(cancellationToken);
        var groupAssociations = await apiClient
            .Members.ListGroupAssociationsAsync(memberId)
            .ToListAsync(cancellationToken);
        return Results.Ok(
            member.ToDto(email, isApproved: member.IsApproved(groupAssociations, options.Value))
        );
    }
);

app.MapGet(
        "/api/members/{memberId}/is-approved",
        async (
            string memberId,
            [FromKeyedServices(ApiKeyClient)] IAkilesApiClient apiClient,
            IOptions<BrugsenAabnSelvOptions> options,
            CancellationToken cancellationToken
        ) =>
        {
            var member = await apiClient.Members.GetMemberAsync(memberId, cancellationToken);
            if (member.IsDeleted)
            {
                return Results.NotFound();
            }
            var groupAssociations = await apiClient
                .Members.ListGroupAssociationsAsync(memberId)
                .ToListAsync(cancellationToken);

            return Results.Ok(member.IsApproved(groupAssociations, options.Value));
        }
    )
    .AllowAnonymous();

app.MapPost(
    "/api/members/{memberId}/approve",
    async (
        string memberId,
        IAkilesApiClient apiClient,
        IOptions<BrugsenAabnSelvOptions> options,
        CancellationToken cancellationToken
    ) =>
    {
        await apiClient.Members.CreateGroupAssociationAsync(
            memberId,
            new() { MemberGroupId = options.Value.ApprovedMemberGroupId },
            cancellationToken
        );
        return Results.NoContent();
    }
);

app.MapDelete(
    "/api/members/{memberId}",
    async (string memberId, IAkilesApiClient apiClient, CancellationToken cancellationToken) =>
    {
        await apiClient.Members.DeleteMemberAsync(memberId, cancellationToken);
        return Results.NoContent();
    }
);

app.MapFallbackToFile("index.html");
app.UseDefaultFiles(new DefaultFilesOptions() { }).UseStaticFiles();
app.Run();

static async Task CopyToResponseAsync(
    HttpResponseMessage clientResponse,
    HttpResponse apiResponse,
    CancellationToken cancellationToken
)
{
    apiResponse.StatusCode = (int)clientResponse.StatusCode;
    foreach (var header in clientResponse.Content.Headers)
    {
        foreach (var value in header.Value)
        {
            apiResponse.Headers.Append(header.Key, value);
        }
    }
    using var stream = await clientResponse.Content.ReadAsStreamAsync(cancellationToken);
    await stream.CopyToAsync(apiResponse.Body, cancellationToken);
}
