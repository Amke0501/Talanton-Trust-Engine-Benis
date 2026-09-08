namespace Talanton.Api.Services;

/// <summary>
/// Checks a requested loan against the individual member's own savings.
///
/// This is separate from the SACCO-wide cash check: liquidity asks whether the institution can
/// afford to lend at all, while this asks whether *this* member has earned the right to borrow
/// this much. A SACCO flush with cash still should not lend eight times a member's savings.
///
/// The distinction that matters is whose number is being trusted. The underwriting guardrail
/// measures the loan against a savings figure entered on the application — the applicant's claim.
/// This measures it against the balance the SACCO holds on the membership record, and reports when
/// the two disagree, because a declared figure well above the record is exactly the case the rule
/// exists to catch.
/// </summary>
public class MemberSavingsService
{
    public static MemberSavingsResult Evaluate(
        decimal requestedPrincipal,
        decimal recordedSavings,
        decimal multiplier,
        decimal? declaredSavings = null)
    {
        if (multiplier <= 0)
        {
            return new MemberSavingsResult
            {
                IsWithinCap = false,
                Reason = "No savings multiplier is set for this member, so a borrowing cap cannot be worked out.",
                RecordedSavings = recordedSavings,
                Multiplier = multiplier,
                MaximumBorrowable = 0m,
                RequestedPrincipal = requestedPrincipal,
                ExcessAmount = requestedPrincipal,
            };
        }

        var cap = recordedSavings * multiplier;
        var within = requestedPrincipal <= cap;
        var excess = within ? 0m : requestedPrincipal - cap;

        var result = new MemberSavingsResult
        {
            IsWithinCap = within,
            RecordedSavings = recordedSavings,
            Multiplier = multiplier,
            MaximumBorrowable = cap,
            RequestedPrincipal = requestedPrincipal,
            ExcessAmount = excess,
            DeclaredSavings = declaredSavings,
        };

        if (declaredSavings.HasValue && declaredSavings.Value != recordedSavings)
        {
            result.DeclaredSavingsMismatch = true;
            result.DeclaredSavingsDifference = declaredSavings.Value - recordedSavings;
        }

        result.Reason = within
            ? $"Within the member's borrowing cap: {requestedPrincipal:N0} requested against a cap of {cap:N0} " +
              $"({recordedSavings:N0} savings × {multiplier:0.##})."
            : $"Exceeds the member's borrowing cap by {excess:N0}: {requestedPrincipal:N0} requested against a cap of " +
              $"{cap:N0} ({recordedSavings:N0} savings × {multiplier:0.##}).";

        if (result.DeclaredSavingsMismatch)
        {
            result.Reason += $" The application declares {declaredSavings!.Value:N0} in savings, " +
                             $"{Math.Abs(result.DeclaredSavingsDifference):N0} " +
                             $"{(result.DeclaredSavingsDifference > 0 ? "more" : "less")} than the SACCO's record.";
        }

        return result;
    }
}

public class MemberSavingsResult
{
    /// <summary>Whether the requested principal is within what this member's savings support.</summary>
    public bool IsWithinCap { get; set; }

    public string Reason { get; set; } = string.Empty;

    /// <summary>Savings the SACCO holds for this member.</summary>
    public decimal RecordedSavings { get; set; }

    public decimal Multiplier { get; set; }

    /// <summary>Recorded savings × multiplier.</summary>
    public decimal MaximumBorrowable { get; set; }

    public decimal RequestedPrincipal { get; set; }

    /// <summary>How far past the cap the request goes; zero when within it.</summary>
    public decimal ExcessAmount { get; set; }

    /// <summary>The savings figure entered on the application, when one was supplied.</summary>
    public decimal? DeclaredSavings { get; set; }

    /// <summary>True when the application's figure disagrees with the SACCO's record.</summary>
    public bool DeclaredSavingsMismatch { get; set; }

    /// <summary>Declared minus recorded. Positive means the application overstates savings.</summary>
    public decimal DeclaredSavingsDifference { get; set; }
}
