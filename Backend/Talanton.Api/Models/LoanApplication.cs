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

    public decimal? CounterOfferPrincipalAmount { get; set; }

    public int? CounterOfferTermMonths { get; set; }

    public string? CounterOfferReason { get; set; }

    public string CounterOfferStatus { get; set; } = "NONE";

    public DateTime? ApplicantConsentAt { get; set; }

    public bool ApplicantConsentReceived { get; set; }

    public DateTime? DecisionAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}