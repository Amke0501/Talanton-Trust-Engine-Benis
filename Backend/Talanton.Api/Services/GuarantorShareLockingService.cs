using Talanton.Api.DTOs;

namespace Talanton.Api.Services;

/// <summary>
/// GuarantorShareLockingService manages the locking and unlocking of guarantor shares.
/// 
/// RULES:
/// - When a loan is disbursed, pledged guarantor shares are locked and deducted from available balance
/// - Locked shares cannot be used as collateral for other loans
/// - When loan is completed/repaid, locked shares are re-credited to available balance
/// - Prevents double-pledging of the same shares across multiple loans
/// </summary>
public class GuarantorShareLockingService
{
    /// <summary>
    /// Locks guarantor shares when a loan is disbursed.
    /// Deducts pledged shares from available_shares balance.
    /// </summary>
    /// <param name="guarantors">List of guarantors with pledged shares</param>
    /// <returns>Updated list of guarantors with locked shares</returns>
    public static List<GuarantorDto> LockGuarantorShares(List<GuarantorDto> guarantors)
    {
        return guarantors.Select(g => new GuarantorDto
        {
            Id = g.Id,
            Name = g.Name,
            MemberId = g.MemberId,
            PledgedShares = g.PledgedShares,
            // Deduct pledged shares from available balance
            AvailableShares = Math.Max(0, g.AvailableShares - g.PledgedShares)
        }).ToList();
    }

    /// <summary>
    /// Unlocks guarantor shares when a loan is completed/repaid.
    /// Re-credits locked shares back to available_shares balance.
    /// </summary>
    /// <param name="guarantors">List of guarantors with currently locked shares</param>
    /// <returns>Updated list of guarantors with unlocked shares</returns>
    public static List<GuarantorDto> UnlockGuarantorShares(List<GuarantorDto> guarantors)
    {
        return guarantors.Select(g => new GuarantorDto
        {
            Id = g.Id,
            Name = g.Name,
            MemberId = g.MemberId,
            PledgedShares = g.PledgedShares,
            // Re-credit pledged shares back to available balance
            AvailableShares = g.AvailableShares + g.PledgedShares
        }).ToList();
    }

    /// <summary>
    /// Validates that a guarantor has sufficient available shares to pledge.
    /// </summary>
    /// <param name="guarantor">Guarantor to validate</param>
    /// <param name="requestedPledge">Amount of shares to pledge</param>
    /// <returns>Validation result with pass/fail and reason</returns>
    public static ShareLockingValidationResult ValidateGuarantorCapacity(GuarantorDto guarantor, decimal requestedPledge)
    {
        if (guarantor.AvailableShares < requestedPledge)
        {
            return new ShareLockingValidationResult
            {
                IsValid = false,
                Reason = $"{guarantor.Name} has insufficient available shares. " +
                        $"Requested: {requestedPledge:N0}, Available: {guarantor.AvailableShares:N0}, " +
                        $"Deficit: {requestedPledge - guarantor.AvailableShares:N0}"
            };
        }

        return new ShareLockingValidationResult
        {
            IsValid = true,
            Reason = $"{guarantor.Name} has sufficient shares. Available: {guarantor.AvailableShares:N0}, Pledging: {requestedPledge:N0}"
        };
    }

    /// <summary>
    /// Checks if guarantor shares are sufficient to cover the loan gap
    /// (difference between requested principal and savings).
    /// </summary>
    /// <param name="guarantors">List of guarantors</param>
    /// <param name="requestedPrincipal">Loan principal amount</param>
    /// <param name="savingsBalance">Applicant's savings balance</param>
    /// <returns>Coverage evaluation result</returns>
    public static GuarantorCoverageResult EvaluateGuarantorCoverage(
        List<GuarantorDto> guarantors,
        decimal requestedPrincipal,
        decimal savingsBalance)
    {
        decimal gap = Math.Max(0, requestedPrincipal - savingsBalance);
        decimal totalAvailablePledge = guarantors.Sum(g => g.AvailableShares);
        decimal totalPledged = guarantors.Sum(g => g.PledgedShares);

        bool isCovered = totalPledged >= gap;
        decimal deficit = isCovered ? 0 : gap - totalPledged;

        return new GuarantorCoverageResult
        {
            IsCovered = isCovered,
            LoanGap = gap,
            TotalPledgedShares = totalPledged,
            TotalAvailableShares = totalAvailablePledge,
            Deficit = deficit,
            Reason = isCovered
                ? $"Guarantor coverage sufficient: {totalPledged:N0} pledged covers gap of {gap:N0}"
                : $"Guarantor coverage insufficient. Gap: {gap:N0}, Pledged: {totalPledged:N0}, Deficit: {deficit:N0}"
        };
    }
}

/// <summary>
/// Result of guarantor share validation.
/// </summary>
public class ShareLockingValidationResult
{
    /// <summary>
    /// Whether the guarantor has sufficient shares.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Explanation of the validation result.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Result of guarantor coverage evaluation.
/// </summary>
public class GuarantorCoverageResult
{
    /// <summary>
    /// Whether pledged shares are sufficient to cover the gap.
    /// </summary>
    public bool IsCovered { get; set; }

    /// <summary>
    /// The gap between requested principal and savings.
    /// </summary>
    public decimal LoanGap { get; set; }

    /// <summary>
    /// Total shares pledged by all guarantors.
    /// </summary>
    public decimal TotalPledgedShares { get; set; }

    /// <summary>
    /// Total available shares (not yet pledged) by all guarantors.
    /// </summary>
    public decimal TotalAvailableShares { get; set; }

    /// <summary>
    /// If not covered: the deficit amount needed.
    /// </summary>
    public decimal Deficit { get; set; }

    /// <summary>
    /// Explanation of the coverage result.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
