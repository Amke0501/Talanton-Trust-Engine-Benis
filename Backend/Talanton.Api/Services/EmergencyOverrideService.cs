namespace Talanton.Api.Services;

/// <summary>
/// The dual-key release: two different officers, together, may release funds the liquidity gate
/// has locked.
///
/// The point of two keys is that neither person can act alone, so the rules below are about who
/// is turning them rather than about the money. A single officer supplying both signatures is
/// the failure mode this exists to prevent, and it is rejected explicitly rather than by
/// accident of comparison.
/// </summary>
public static class EmergencyOverrideService
{
    /// <summary>
    /// The seats trusted with a key. Deliberately narrow: the Credit Officer and Board Member
    /// seats vote on files but do not hold the SACCO's cash.
    /// </summary>
    public static readonly string[] KeyHolderSeats = { "Chairperson", "Treasurer", "Secretary" };

    /// <summary>A reason short enough to be meaningless is not a reason.</summary>
    public const int MinimumReasonLength = 15;

    public static OverrideAuthorizationResult Evaluate(
        string? firstSeat, string? secondSeat, string? reason)
    {
        var first = Normalize(firstSeat);
        var second = Normalize(secondSeat);

        if (first is null || second is null)
        {
            return Refused(
                "An emergency release needs two authorising officers. " +
                $"Supply both signatures from: {string.Join(", ", KeyHolderSeats)}.");
        }

        if (!IsKeyHolder(first))
        {
            return Refused($"'{firstSeat}' does not hold an emergency release key. " +
                           $"Keys are held by: {string.Join(", ", KeyHolderSeats)}.");
        }

        if (!IsKeyHolder(second))
        {
            return Refused($"'{secondSeat}' does not hold an emergency release key. " +
                           $"Keys are held by: {string.Join(", ", KeyHolderSeats)}.");
        }

        if (first.Equals(second, StringComparison.OrdinalIgnoreCase))
        {
            return Refused($"Both signatures were given by the {first}. " +
                           "An emergency release requires two different officers.");
        }

        var trimmedReason = reason?.Trim() ?? string.Empty;
        if (trimmedReason.Length < MinimumReasonLength)
        {
            return Refused(
                $"A written reason of at least {MinimumReasonLength} characters is required, and it " +
                "is recorded in the audit trail against both officers.");
        }

        return new OverrideAuthorizationResult
        {
            IsAuthorized = true,
            FirstSeat = first,
            SecondSeat = second,
            Reason = trimmedReason,
            Explanation = $"Emergency release authorised jointly by the {first} and the {second}.",
        };
    }

    private static bool IsKeyHolder(string seat) =>
        KeyHolderSeats.Contains(seat, StringComparer.OrdinalIgnoreCase);

    private static string? Normalize(string? seat)
    {
        if (string.IsNullOrWhiteSpace(seat)) return null;
        var trimmed = seat.Trim();
        return KeyHolderSeats.FirstOrDefault(s => s.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
               ?? trimmed;
    }

    private static OverrideAuthorizationResult Refused(string explanation) =>
        new() { IsAuthorized = false, Explanation = explanation };
}

public class OverrideAuthorizationResult
{
    public bool IsAuthorized { get; set; }
    public string? FirstSeat { get; set; }
    public string? SecondSeat { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
