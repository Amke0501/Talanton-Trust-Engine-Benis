namespace Talanton.Api.Models;

public class SaccoMembership
{
    public Guid Id { get; set; }

    public Guid ApplicantId { get; set; }

    public Applicant Applicant { get; set; } = null!;

    public Guid SaccoId { get; set; }

    public Sacco Sacco { get; set; } = null!;

    public string MembershipNumber { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }

    public string Status { get; set; } = "Active";

    public bool IsPrimaryMember { get; set; }

    /// <summary>
    /// The member's savings held by the SACCO, as recorded on their membership.
    ///
    /// The underwriting guardrail previously measured a loan against a savings figure typed into
    /// the application, which is the applicant's own claim rather than the SACCO's record. This is
    /// the figure the SACCO actually holds, so the borrowing cap can be checked against something
    /// the applicant does not control.
    /// </summary>
    public decimal SavingsBalance { get; set; }

    /// <summary>When the savings figure above was last reconciled with the ledger.</summary>
    public DateTime? SavingsUpdatedAt { get; set; }
}