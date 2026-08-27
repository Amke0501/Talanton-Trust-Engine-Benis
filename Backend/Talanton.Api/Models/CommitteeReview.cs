namespace Talanton.Api.Models;

public class CommitteeReview
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid InitiatedByUserId { get; set; }

    public User InitiatedByUser { get; set; } = null!;

    public string ReviewStatus { get; set; } = "Open";

    public DateTime? ScheduledAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public int QuorumRequired { get; set; }
}