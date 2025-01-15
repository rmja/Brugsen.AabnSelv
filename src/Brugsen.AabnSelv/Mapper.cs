using Akiles.Api.Members;
using Brugsen.AabnSelv.Models;

namespace Brugsen.AabnSelv;

public static class Mapper
{
    public static MemberDto ToDto(this Member member, MemberEmail email, bool isApproved) =>
        new()
        {
            Id = member.Id,
            Email = email.Email,
            Name = member.Name,
            Address = member.Metadata[MetadataKeys.Member.Address],
            Phone = member.Metadata[MetadataKeys.Member.Phone],
            CoopMembershipNumber = member.Metadata[MetadataKeys.Member.CoopMembershipNumber],
            LaesoeCardNumber = member.Metadata[MetadataKeys.Member.CoopMembershipNumber],
            LaesoeCardColor = Enum.Parse<LaesoeCardColor>(
                member.Metadata[MetadataKeys.Member.LaesoeCardColor],
                true
            ),
            IsApproved = isApproved
        };
}
