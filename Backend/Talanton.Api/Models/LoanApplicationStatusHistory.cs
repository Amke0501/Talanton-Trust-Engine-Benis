namespace Talanton.Api.Models;

public class LoanApplicationStatusHistory
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public string? Reason { get; set; }

    public Guid ChangedByUserId { get; set; }

    public User ChangedByUser { get; set; } = null!;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}