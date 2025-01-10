using Akiles.Api;
using Brugsen.AabnSelv;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.Configure<AabnSelvOptions>(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddAkilesApi(options =>
{
    options.ClientId = "";
    options.ClientSecret = "";
});

var app = builder.Build();

app.UseHttpsRedirection();

app.MapPost(
    "/signup",
    async (SignupModel model, IAkilesApiClient apiClient, IOptions<AabnSelvOptions> options) =>
    {
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
    }
);

app.MapControllers();
app.Run();

record SignupModel
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string Phone { get; init; }
}
