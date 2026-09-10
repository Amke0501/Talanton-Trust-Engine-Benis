namespace Talanton.Api.Models;

public class LoanApplication
{
    public Guid Id { get; set; }

    public string ApplicationNumber { get; set; } = string.Empty;

    public Guid ApplicantId { get; set; }

    public Applicant Applicant { get; set; } = null!;

    public Guid SaccoId { get; set; }

    public Sacco Sacco { get; set; } = null!;

    public Guid? AssignedUnderwriterUserId { get; set; }

    public User? AssignedUnderwriter { get; set; }

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public string CurrentStatus { get; set; } = "Draft";

    public string CurrentStage { get; set; } = "verification";

    public decimal PrincipalAmount { get; set; }

    public decimal AnnualSimpleInterestRatePct { get; set; }

    public int TermMonths { get; set; }

    public decimal AdministrativeFeeAmount { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public DateTime? SubmittedAt { get; set; }

    /// <summary>
    /// The applicant's SACCO membership number. Held on the file rather than derived, because the
    /// invoice hold, the member-savings check and the applicant's own "my applications" list are
    /// all keyed on it; every database-backed file used to report a hardcoded placeholder.
    /// </summary>
    public string MemberId { get; set; } = string.Empty;

    // ── Underwriting outcome ──────────────────────────────────────────────────────────────────
    //
    // These were computed on every request and thrown away. Because nothing stored them, a file
    // read back from the database always reported Verdict "IN_REVIEW" and a generic status note,
    // which is why an applicant who declined a revised offer was never shown as needing more
    // guarantors, and why a declined verdict could not survive a round trip.

    /// <summary>PENDING | APPROVED | DECLINED — the underwriting desk's verdict.</summary>
    public string Verdict { get; set; } = "PENDING";

    /// <summary>The sentence shown to whoever opens the file next.</summary>
    public string StatusNote { get; set; } = string.Empty;

    public decimal SavingsBalance { get; set; }

    public decimal MonthlyIncome { get; set; }

    public decimal MonthlyDebt { get; set; }

    public decimal Multiplier { get; set; } = 3.0m;

    public decimal DtiNetRatio { get; set; }

    public decimal NetTakeHome { get; set; }

    public bool GuardrailDepositMultiplierPassed { get; set; }

    public bool GuardrailOneThirdPayPassed { get; set; }

    public bool GuardrailGuarantorPassed { get; set; }

    // ── Counter-offer and applicant consent ───────────────────────────────────────────────────

    public decimal? CounterOfferPrincipalAmount { get; set; }

    public int? CounterOfferTermMonths { get; set; }

    public string? CounterOfferReason { get; set; }

    public string CounterOfferStatus { get; set; } = "NONE";

    public DateTime? ApplicantConsentAt { get; set; }

    public bool ApplicantConsentReceived { get; set; }

    // ── Liquidity deferral ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// When a release was refused because the SACCO's cash position was too thin. The file keeps
    /// its place in the queue and is re-released once cash recovers, rather than quietly failing.
    /// </summary>
    public DateTime? DeferredForLiquidityAt { get; set; }

    public string? DeferredForLiquidityReason { get; set; }

    // ── Emergency release (dual key) ──────────────────────────────────────────────────────────

    /// <summary>The two seats that jointly authorised a release against the liquidity lock.</summary>
    public string? EmergencyOverrideFirstSeat { get; set; }

    public string? EmergencyOverrideSecondSeat { get; set; }

    public string? EmergencyOverrideReason { get; set; }

    public DateTime? EmergencyOverrideAt { get; set; }

    // ── Repayment ─────────────────────────────────────────────────────────────────────────────

    public decimal AmountRepaid { get; set; }

    /// <summary>Set when the loan is settled in full and the guarantors' shares are released.</summary>
    public DateTime? RepaidAt { get; set; }

    public DateTime? DisbursedAt { get; set; }

    public DateTime? DecisionAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
