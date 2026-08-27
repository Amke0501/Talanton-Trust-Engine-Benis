namespace Talanton.Api.Models;

public class ApplicationDecision
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid? CommitteeReviewId { get; set; }

    public CommitteeReview? CommitteeReview { get; set; }

    public Guid DecidedByUserId { get; set; }

    public User DecidedByUser { get; set; } = null!;

    public string FinalDecision { get; set; } = string.Empty;

    public string? DecisionReason { get; set; }

    public decimal? ApprovedPrincipal { get; set; }

    public decimal? ApprovedAnnualSimpleInterestRatePct { get; set; }

    public int? ApprovedTermMonths { get; set; }

    public decimal? ApprovedAdministrativeFeeAmount { get; set; }

    public DateTime DecisionAt { get; set; } = DateTime.UtcNow;
}