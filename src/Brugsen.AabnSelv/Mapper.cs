using Akiles.ApiClient.Events;
using Akiles.ApiClient.Members;
using Brugsen.AabnSelv.Endpoints;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Models;
using Brugsen.AabnSelv.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            LaesoeCardNumber = member.Metadata[MetadataKeys.Member.LaesoeCardNumber],
            LaesoeCardColor = Enum.Parse<LaesoeCardColor>(
                member.Metadata[MetadataKeys.Member.LaesoeCardColor],
                true
            ),
            IsApproved = isApproved
        };

    public static AccessActivityDto ToDto(this AccessActivity activity)
    {
        var member = (activity.CheckInEvent ?? activity.CheckOutEvent)?.SubjectMember;
        return new()
        {
            MemberId = activity.MemberId,
            MemberName = member?.Name ?? "",
            CoopMembershipNumber = member?.Metadata.GetValueOrDefault(
                MetadataKeys.Member.CoopMembershipNumber
            ),
            CheckInEvent = activity.CheckInEvent?.ToDto(),
            CheckOutEvent = activity.CheckOutEvent?.ToDto(),
        };
    }

    public static ActionEventDto ToDto(this Event evnt) =>
        new()
        {
            Action = evnt.Object.GadgetActionId!,
            Method = GetAccessMethod(evnt),
            CreatedAt = evnt.CreatedAt
        };

    private static AccessMethod? GetAccessMethod(Event evnt)
    {
        if (
            evnt.Object.GadgetActionId == AppAccessGadget.Actions.CheckIn
            && evnt.Object.HardwareId is null
        )
        {
            return AccessMethod.App;
        }
        else if (
            evnt.Object.GadgetActionId == AppAccessGadget.Actions.CheckIn
            && evnt.Object.HardwareId is not null
        )
        {
            return AccessMethod.Nfc;
        }
        else if (
            evnt.Object.GadgetActionId == AppAccessGadget.Actions.CheckOut
            && evnt.Object.HardwareId is null
        )
        {
            return AccessMethod.App;
        }
        else if (
            evnt.Object.GadgetActionId == FrontDoorGadget.Actions.OpenOnce
            && evnt.Subject.MemberPinId is not null
        )
        {
            return AccessMethod.Pin;
        }
        else if (
            evnt.Object.GadgetActionId == FrontDoorGadget.Actions.OpenOnce
            && evnt.Subject.MemberPinId is null
        )
        {
            return AccessMethod.Nfc;
        }
        else
        {
            return null;
        }
    }
}
