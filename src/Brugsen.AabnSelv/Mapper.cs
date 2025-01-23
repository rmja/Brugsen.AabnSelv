using Akiles.Api.Events;
using Akiles.Api.Members;
using Brugsen.AabnSelv.Endpoints;
using Brugsen.AabnSelv.Gadgets;
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

    public static AccessActivityDto ToDto(this AccessActivity activity)
    {
        var memberName = (activity.CheckInEvent ?? activity.CheckOutEvent)?.SubjectMember?.Name;
        return new()
        {
            MemberId = activity.MemberId,
            MemberName = memberName ?? "",
            CheckedInAt = activity.CheckInEvent?.CreatedAt,
            CheckedOutAt = activity.CheckOutEvent?.CreatedAt
        };
    }

    public static EventDto ToDto(this Event evnt) =>
        new() { Action = evnt.Object.GadgetActionId!, CreatedAt = evnt.CreatedAt };
}
