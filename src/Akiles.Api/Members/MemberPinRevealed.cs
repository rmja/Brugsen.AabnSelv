namespace Akiles.Api.Members;

public record MemberPinRevealed : MemberPin
{
    public required string Pin { get; set; }
}
