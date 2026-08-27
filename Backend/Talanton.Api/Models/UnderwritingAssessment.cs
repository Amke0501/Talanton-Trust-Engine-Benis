namespace Talanton.Api.Models;

public class UnderwritingAssessment
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid UnderwriterUserId { get; set; }

    public User UnderwriterUser { get; set; } = null!;

    public Guid? CreditAssessmentId { get; set; }

    public CreditAssessment? CreditAssessment { get; set; }

    public string Recommendation { get; set; } = string.Empty;

    public decimal? RecommendedPrincipal { get; set; }

    public decimal? RecommendedAnnualSimpleInterestRatePct { get; set; }

    public int? RecommendedTermMonths { get; set; }

    public decimal? RecommendedAdministrativeFeeAmount { get; set; }

    public string? Conditions { get; set; }

    public string? Rationale { get; set; }

    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}