namespace Talanton.Api.Models;

public class CreditAssessment
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid? AssessedByUserId { get; set; }

    public User? AssessedByUser { get; set; }

    public DateTime AssessmentDate { get; set; } = DateTime.UtcNow;

    public int AssessmentVersion { get; set; }

    public string? ModelName { get; set; }

    public decimal? OverallCreditScore { get; set; }

    public string? RiskGrade { get; set; }

    public string? NarrativeSummary { get; set; }

    public bool IsFinal { get; set; }
}