namespace Talanton.Api.DTOs;

public class LoanApplicationDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string ApplicantType { get; set; } = "individual"; // individual or cooperative
    public string Status { get; set; } = "in_review";
    public string Stage { get; set; } = "underwriting";
    public decimal Principal { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int TenureMonths { get; set; }
    public decimal SavingsBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyDebt { get; set; }
    public decimal Multiplier { get; set; } = 3.0m;
    public string SubmittedOn { get; set; } = string.Empty;
    public string StatusNote { get; set; } = string.Empty;
    public decimal DtiNetRatio { get; set; }
    public decimal NetTakeHome { get; set; }
    public bool GuardrailDepositMultiplierPassed { get; set; }
    public bool GuardrailOneThirdPayPassed { get; set; }
    public bool GuardrailGuarantorPassed { get; set; }
    public string Verdict { get; set; } = "DECLINED"; // DECLINED | APPROVED
    public decimal? CounterOfferPrincipal { get; set; }
    public int? CounterOfferTenureMonths { get; set; }
    public string? CounterOfferReason { get; set; }
    public string CounterOfferStatus { get; set; } = "NONE"; // NONE | PENDING | ACCEPTED | DECLINED
    public DateTime? ApplicantConsentAt { get; set; }
    public bool ApplicantConsentReceived { get; set; }

    /// <summary>
    /// How many *new* guarantors this file needs before it can be reconsidered — non-zero only
    /// after the applicant has declined a revised offer. The applicant portal reads this rather
    /// than parsing the status note, so "needs additional guarantors" is a state the screens can
    /// act on instead of a sentence a human has to notice.
    /// </summary>
    public int MinimumAdditionalGuarantorsRequired { get; set; }

    /// <summary>Set when a release was held back because the SACCO's cash position is too thin.</summary>
    public DateTime? DeferredForLiquidityAt { get; set; }
    public string? DeferredForLiquidityReason { get; set; }

    public string? EmergencyOverrideFirstSeat { get; set; }
    public string? EmergencyOverrideSecondSeat { get; set; }
    public string? EmergencyOverrideReason { get; set; }
    public DateTime? EmergencyOverrideAt { get; set; }

    public decimal AmountRepaid { get; set; }
    public DateTime? RepaidAt { get; set; }
    public DateTime? DisbursedAt { get; set; }

    public List<GuarantorDto> Guarantors { get; set; } = new();
    public List<CommitteeVoteDetailDto> CommitteeVotes { get; set; } = new();
    public string RepaymentProgress { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public decimal Arrears { get; set; }
    public string AppraisalOfficer { get; set; } = "Agaba Collins (Risk Division)";
    public string SecuritySignature { get; set; } = "OTP Signed (Verified)";
}

public class GuarantorDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public decimal PledgedShares { get; set; }
    public decimal AvailableShares { get; set; }

    /// <summary>Shares actually committed against a disbursed loan; zero once it is repaid.</summary>
    public decimal LockedShares { get; set; }
    public DateTime? SharesLockedAt { get; set; }
    public DateTime? SharesReleasedAt { get; set; }
}

public class CommitteeVoteDetailDto
{
    public string MemberName { get; set; } = string.Empty;
    public string MemberRole { get; set; } = string.Empty;
    public string Vote { get; set; } = "ABSTAIN"; // APPROVE | REJECT | ABSTAIN
}

/// <summary>
/// An applicant's second attempt after declining a revised offer, backed by additional guarantors.
/// </summary>
public class ResubmitWithGuarantorsDto
{
    public List<GuarantorDto> Guarantors { get; set; } = new();
}

public class CreateLoanApplicationDto
{
    public string ApplicantName { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string ApplicantType { get; set; } = "individual";
    public decimal Principal { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public int TenureMonths { get; set; }
    public decimal SavingsBalance { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal MonthlyDebt { get; set; }
    public decimal Multiplier { get; set; } = 3.0m;
}

public class UpdateUnderwritingOverrideDto
{
    public string ApplicantType { get; set; } = "individual";
    public decimal Multiplier { get; set; } = 3.0m;
    public int TenureMonths { get; set; } = 12;
    public decimal RequestedPrincipal { get; set; }
    public decimal SavingsBalance { get; set; }
    public decimal BasicMonthlyPay { get; set; }
    public decimal MonthlyDeductions { get; set; }
    public string? AdjustmentReason { get; set; }
}

public class CounterOfferDecisionDto
{
    public string Decision { get; set; } = "ACCEPT"; // ACCEPT | DECLINE
}

/// <summary>Money received against a disbursed loan.</summary>
public class RecordRepaymentDto
{
    public decimal Amount { get; set; }

    /// <summary>Who recorded it — written to the audit trail.</summary>
    public string RecordedByRole { get; set; } = string.Empty;

    public string? Note { get; set; }
}

/// <summary>
/// The two signatures needed to release funds the liquidity gate has locked, and the reason,
/// which is recorded against both officers.
/// </summary>
public class EmergencyReleaseDto
{
    public string FirstSeat { get; set; } = string.Empty;
    public string SecondSeat { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class CastCommitteeVoteDto
{
    public string MemberRole { get; set; } = string.Empty;
    public string Vote { get; set; } = "APPROVE"; // APPROVE | REJECT | ABSTAIN
}

public class CreditPassportMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string MemberId { get; set; } = string.Empty;
    public string Classification { get; set; } = "Individual"; // Individual | SME
    public string Tier { get; set; } = "GOLD"; // PLATINUM | GOLD | SILVER
    public int TrustScore { get; set; }
    public int OnTimeRatePct { get; set; }
    public int LoansCompleted { get; set; }
    public decimal TotalRepaid { get; set; }
    public decimal CurrentLimit { get; set; }
    public string LastLoanDate { get; set; } = string.Empty;
}

public class QuorumCheckRequestDto
{
    /// <summary>
    /// Loan reference number
    /// </summary>
    public string Reference { get; set; } = string.Empty;
}

public class QuorumCheckResponseDto
{
    /// <summary>
    /// Whether the quorum requirement has been met.
    /// </summary>
    public bool IsQuorumPassed { get; set; }

    /// <summary>
    /// Human-readable explanation of the quorum status.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a big loan (≥ 5M UGX).
    /// </summary>
    public bool IsBigLoan { get; set; }

    /// <summary>
    /// Number of approvals required for this loan.
    /// </summary>
    public int RequiredApprovals { get; set; }

    /// <summary>
    /// Number of approvals currently received.
    /// </summary>
    public int ApprovalCount { get; set; }

    /// <summary>
    /// Whether the Chairperson has cast a veto (REJECT vote).
    /// </summary>
    public bool HasChairpersonVeto { get; set; }

    /// <summary>
    /// For big loans: whether both Chairman and Treasurer have approved.
    /// </summary>
    public bool HasRequiredMembers { get; set; }
}

public class DisbursementAuthorizationDto
{
    /// <summary>
    /// Role of the user requesting disbursement.
    /// </summary>
    public string RequestorRole { get; set; } = string.Empty;

    /// <summary>
    /// For big loans: Chairperson's cryptographic signature or authorization token.
    /// </summary>
    public string? ChairpersonSignature { get; set; }

    /// <summary>
    /// For big loans: Secretary's cryptographic signature or authorization token.
    /// </summary>
    public string? SecretarySignature { get; set; }

    /// <summary>
    /// Optional: Notes or reason for disbursement (logged to audit trail).
    /// </summary>
    public string? DisbursementNotes { get; set; }

    // ── Dual-key emergency release ────────────────────────────────────────────────────────────
    //
    // Supplied only when the board intends to release funds the liquidity gate has locked. Both
    // seats must be key holders and must differ, and the reason is recorded against both.

    public string? EmergencyFirstSeat { get; set; }
    public string? EmergencySecondSeat { get; set; }
    public string? EmergencyReason { get; set; }
}

public class DisbursementAuthorizationResponseDto
{
    /// <summary>
    /// Whether the disbursement was authorized.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Explanation of the authorization decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// If authorized: the updated loan application after disbursement.
    /// </summary>
    public LoanApplicationDto? UpdatedApplication { get; set; }

    /// <summary>
    /// Disbursement timestamp (UTC).
    /// </summary>
    public DateTime? DisbursementAt { get; set; }

    /// <summary>
    /// True when the release was held for cash rather than refused outright — the file keeps its
    /// place in the queue and is shown as "Deferred: awaiting liquidity".
    /// </summary>
    public bool IsDeferredForLiquidity { get; set; }

    /// <summary>The cash position at the moment of the decision, when one was read.</summary>
    public object? LiquidityStatus { get; set; }
}
