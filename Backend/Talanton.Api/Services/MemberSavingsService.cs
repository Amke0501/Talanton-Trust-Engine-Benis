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
    /// <summary>
    /// The committee's member-level limit: a member may not borrow more than twice the savings the
    /// SACCO holds for them.
    ///
    /// This is a different rule from the underwriting multiplier above it, and deliberately so.
    /// The multiplier is a policy dial an underwriter may set per applicant — three times savings
    /// by default. This is a fixed ceiling the credit committee applies at the point of approval,
    /// and it does not move. A file can satisfy a 3x underwriting cap and still breach it, which is
    /// the whole reason the committee checks again.
    /// </summary>
    public const decimal MemberLimitRatio = 2.0m;

    private static readonly System.Globalization.CultureInfo Invariant =
        System.Globalization.CultureInfo.InvariantCulture;

    /// <summary>
    /// Whether this member's own savings support the amount requested, at the committee's fixed
    /// 2:1 limit. Shown against each row on the loan processing screen.
    /// </summary>
    public static MemberLimitResult EvaluateMemberLimit(decimal requestedPrincipal, decimal recordedSavings)
    {
        var limit = recordedSavings * MemberLimitRatio;
        var within = requestedPrincipal <= limit;
        var shortfall = within ? 0m : requestedPrincipal - limit;

        return new MemberLimitResult
        {
            IsWithinLimit = within,
            RecordedSavings = recordedSavings,
            MaximumBorrowable = limit,
            RequestedPrincipal = requestedPrincipal,
            ShortfallAmount = shortfall,
            // The wording the committee asked for, naming what would close the gap rather than
            // only that there is one.
            //
            // Formatted against the invariant culture explicitly, not left to whatever the host
            // happens to be set to. The exact text is specified by the client, so it must not
            // become "3 000 000" on one machine and "3,000,000" on another — and a process-wide
            // culture setting is the kind of ambient state that silently differs between the app
            // and anything else that calls this.
            Message = within
                ? string.Format(Invariant,
                    "Within the 2:1 member limit: {0:N0} against {1:N0} ({2:N0} savings × 2).",
                    requestedPrincipal, limit, recordedSavings)
                : string.Format(Invariant,
                    "Exceeds 2:1 Member Limit. Requires {0:N0} in Guarantor Deposits.", shortfall),
        };
    }

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


/// <summary>
/// The committee's fixed 2:1 check on one member, as shown beside their loan on the processing
/// screen.
/// </summary>
public class MemberLimitResult
{
    public bool IsWithinLimit { get; set; }

    /// <summary>Savings the SACCO holds for this member.</summary>
    public decimal RecordedSavings { get; set; }

    /// <summary>Recorded savings × 2.</summary>
    public decimal MaximumBorrowable { get; set; }

    public decimal RequestedPrincipal { get; set; }

    /// <summary>What the member would need covered in guarantor deposits; zero when within limit.</summary>
    public decimal ShortfallAmount { get; set; }

    public string Message { get; set; } = string.Empty;
}
