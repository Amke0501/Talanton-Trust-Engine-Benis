using Talanton.Api.DTOs;

namespace Talanton.Api.Services;

/// <summary>
/// DisbursementAuthorizationService enforces access controls for loan disbursement.
/// 
/// RULES:
/// - Small loans (< 5M UGX): Only Treasurer can disburse
/// - Big loans (≥ 5M UGX): Requires both Chairperson and Secretary authorization (dual-signature)
/// - All disbursements must be logged to an immutable audit trail
/// </summary>
public class DisbursementAuthorizationService
{
    private const decimal BIG_LOAN_THRESHOLD = 5_000_000m;
    private const string ROLE_TREASURER = "Treasurer";
    private const string ROLE_CHAIRPERSON = "Chairperson";
    private const string ROLE_SECRETARY = "Secretary";

    /// <summary>
    /// Evaluates whether the requestor is authorized to disburse the specified loan.
    /// </summary>
    /// <param name="principalAmount">Loan principal in UGX</param>
    /// <param name="requestorRole">Role of the user requesting disbursement</param>
    /// <param name="chairpersonApprovalPresent">For big loans: whether Chairperson has authorized</param>
    /// <param name="secretaryApprovalPresent">For big loans: whether Secretary has authorized</param>
    /// <returns>DisbursementAuthorizationResult with allow/deny status and reason</returns>
    public static DisbursementAuthorizationResult EvaluateDisbursementAuthority(
        decimal principalAmount,
        string? requestorRole,
        bool chairpersonApprovalPresent = false,
        bool secretaryApprovalPresent = false)
    {
        bool isBigLoan = principalAmount >= BIG_LOAN_THRESHOLD;

        if (isBigLoan)
        {
            // Big loans require both Chairperson AND Secretary
            if (!chairpersonApprovalPresent || !secretaryApprovalPresent)
            {
                return new DisbursementAuthorizationResult
                {
                    IsAuthorized = false,
                    Reason = "Big loan disbursement requires dual authorization from both Chairperson AND Secretary. " +
                             $"Chairperson: {(chairpersonApprovalPresent ? "✓" : "✗")}, Secretary: {(secretaryApprovalPresent ? "✓" : "✗")}",
                    IsBigLoan = true,
                    RequiredRole = "Chairperson + Secretary",
                    RequestorRole = requestorRole ?? string.Empty,
                    DisbursementType = DisbursementType.DualSignatureBigLoan
                };
            }

            // For big loans, the requestor should typically be Chairperson or Secretary
            bool isAuthorizedRole = requestorRole?.Equals(ROLE_CHAIRPERSON, StringComparison.OrdinalIgnoreCase) == true ||
                                   requestorRole?.Equals(ROLE_SECRETARY, StringComparison.OrdinalIgnoreCase) == true;

            if (!isAuthorizedRole)
            {
                return new DisbursementAuthorizationResult
                {
                    IsAuthorized = false,
                    Reason = "Big loan disbursement can only be executed by Chairperson or Secretary. " +
                             $"Current user role: {requestorRole}",
                    IsBigLoan = true,
                    RequiredRole = "Chairperson + Secretary",
                    RequestorRole = requestorRole ?? string.Empty,
                    DisbursementType = DisbursementType.DualSignatureBigLoan
                };
            }

            return new DisbursementAuthorizationResult
            {
                IsAuthorized = true,
                Reason = "Big loan disbursement authorized by Chairperson and Secretary signatures.",
                IsBigLoan = true,
                RequiredRole = "Chairperson + Secretary",
                RequestorRole = requestorRole ?? string.Empty,
                DisbursementType = DisbursementType.DualSignatureBigLoan
            };
        }

        // Small loans require only Treasurer.
        // Written as a plain comparison: `!requestorRole?.Equals(...) == true` lifts to bool?, and
        // a null role made that expression null rather than true — so a request carrying no role
        // at all skipped the deny branch and was authorized.
        if (!string.Equals(requestorRole, ROLE_TREASURER, StringComparison.OrdinalIgnoreCase))
        {
            return new DisbursementAuthorizationResult
            {
                IsAuthorized = false,
                Reason = $"Small loan disbursement requires Treasurer authorization only. Current user role: {requestorRole}",
                IsBigLoan = false,
                RequiredRole = ROLE_TREASURER,
                RequestorRole = requestorRole ?? string.Empty,
                DisbursementType = DisbursementType.TreasurerSmallLoan
            };
        }

        return new DisbursementAuthorizationResult
        {
            IsAuthorized = true,
            Reason = "Small loan disbursement authorized by Treasurer.",
            IsBigLoan = false,
            RequiredRole = ROLE_TREASURER,
            RequestorRole = requestorRole ?? string.Empty,
            DisbursementType = DisbursementType.TreasurerSmallLoan
        };
    }

    /// <summary>
    /// Determines if the loan is classified as "big" (≥ 5M UGX).
    /// </summary>
    public static bool IsBigLoan(decimal principalAmount) 
        => principalAmount >= BIG_LOAN_THRESHOLD;
}

/// <summary>
/// Result of disbursement authorization evaluation.
/// </summary>
public class DisbursementAuthorizationResult
{
    /// <summary>
    /// Whether the requestor is authorized to disburse.
    /// </summary>
    public bool IsAuthorized { get; set; }

    /// <summary>
    /// Human-readable explanation of the authorization decision.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a big loan (≥ 5M UGX).
    /// </summary>
    public bool IsBigLoan { get; set; }

    /// <summary>
    /// Required role(s) for disbursement.
    /// </summary>
    public string RequiredRole { get; set; } = string.Empty;

    /// <summary>
    /// The role of the requestor.
    /// </summary>
    public string RequestorRole { get; set; } = string.Empty;

    /// <summary>
    /// Type of disbursement authorization required.
    /// </summary>
    public DisbursementType DisbursementType { get; set; }
}

/// <summary>
/// Enum to categorize disbursement authorization types.
/// </summary>
public enum DisbursementType
{
    /// <summary>
    /// Small loan: Treasurer authorization only
    /// </summary>
    TreasurerSmallLoan,

    /// <summary>
    /// Big loan: Dual signature from Chairperson and Secretary
    /// </summary>
    DualSignatureBigLoan
}
