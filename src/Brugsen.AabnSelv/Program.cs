using Akiles.Api;
using Akiles.Api.Members;
using Brugsen.AabnSelv;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<AabnSelvOptions>(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddAkilesApi(options =>
{
    options.ApiKey = "...";
});

var app = builder.Build();

app.MapPost(
    "/signup",
    async (SignupModel model, IAkilesApiClient apiClient, IOptions<AabnSelvOptions> options) =>
    {
        var existingMember = await apiClient
            .Members.ListMembersAsync(null, new ListMembersFilter() { Email = model.Email })
            .FirstOrDefaultAsync();
        if (existingMember is not null)
        {
            return Results.Conflict();
        }

        var member = await apiClient.Members.CreateMemberAsync(
            new()
            {
                Name = model.Name,
                Metadata = { ["phone"] = model.Phone, ["address"] = model.Address }
            }
        );

        foreach (var groupId in options.Value.SignupMemberGroupIds)
        {
            await apiClient.Members.CreateGroupAssociationAsync(
                member.Id,
                new() { MemberGroupId = groupId }
            );
        }

        await apiClient.Members.CreateEmailAsync(member.Id, new() { Email = model.Email });

        return Results.NoContent();
    }
);
app.UseDefaultFiles().UseStaticFiles();
app.Run();

record SignupModel
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string Phone { get; init; }
}
