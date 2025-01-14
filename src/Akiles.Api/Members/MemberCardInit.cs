using System.ComponentModel.DataAnnotations;

namespace Akiles.Api.Members;

public record MemberCardInit : IValidatableObject
{
    public string? CardId { get; init; }
    public string? PrintedCode { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CardId is null && PrintedCode is null)
        {
            yield return new ValidationResult(
                "One of CardId or PrintedCode must be present",
                [nameof(CardId), nameof(PrintedCode)]
            );
        }
    }
}
