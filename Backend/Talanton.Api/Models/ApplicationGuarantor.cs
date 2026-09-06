namespace Talanton.Api.Models;

/// <summary>
/// A guarantor's pledge against one loan application.
///
/// Guarantors previously existed only in an in-memory list, so a pledge — and the share lock
/// placed on it at disbursement — was lost on restart and invisible to any other instance. That
/// made the founder's rule unenforceable in practice: shares locked against one loan could be
/// pledged again to another simply by restarting the service.
/// </summary>
public class ApplicationGuarantor
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>The guarantor's SACCO membership number.</summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>Shares this guarantor has pledged against this application.</summary>
    public decimal PledgedShares { get; set; }

    /// <summary>Shares still free to pledge elsewhere, after any lock below.</summary>
    public decimal AvailableShares { get; set; }

    /// <summary>
    /// Shares actually locked at disbursement. Zero until the loan is released, and reset when
    /// the loan is repaid — the difference between "promised" and "committed".
    /// </summary>
    public decimal LockedShares { get; set; }

    public DateTime? SharesLockedAt { get; set; }

    public DateTime? SharesReleasedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
