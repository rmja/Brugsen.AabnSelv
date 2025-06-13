using Akiles.ApiClient.Events;
using Akiles.ApiClient.Members;
using Brugsen.AabnSelv.Endpoints;
using Brugsen.AabnSelv.Gadgets;
using Brugsen.AabnSelv.Models;
using Brugsen.AabnSelv.Services;

namespace Brugsen.AabnSelv;

public static class Mapper
{
    public static MemberDto ToDto(this Member member, MemberEmail email, bool isApproved) =>
        new()
        {
            Id = member.Id,
            Email = email.Email,
            Name = member.Name,
            Address = member.Metadata.GetValueOrDefault(MetadataKeys.Member.Address, string.Empty),
            Phone = member.Metadata.GetValueOrDefault(MetadataKeys.Member.Phone, string.Empty),
            CoopMembershipNumber = member.Metadata.GetValueOrDefault(
                MetadataKeys.Member.CoopMembershipNumber,
                string.Empty
            ),
            LaesoeCardNumber = member.Metadata.GetValueOrDefault(
                MetadataKeys.Member.LaesoeCardNumber
            ),
            LaesoeCardColor = member.Metadata.TryGetValue(
                MetadataKeys.Member.LaesoeCardColor,
                out var color
            )
                ? Enum.Parse<LaesoeCardColor>(color, true)
                : null,
            IsApproved = isApproved,
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
            CreatedAt = evnt.CreatedAt,
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
