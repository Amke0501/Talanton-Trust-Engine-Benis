using Talanton.Api.DTOs;

namespace Talanton.Api.Services;

/// <summary>
/// QuorumEvaluationService enforces the founder's business rules for committee voting.
/// 
/// RULES:
/// - Small loans (< 5M UGX): require only 1 approval
/// - Big loans (≥ 5M UGX): require 3 approvals, MUST include Chairman and Treasurer
/// - Chairperson's rejection is an absolute veto — blocks the file regardless of other approvals
/// </summary>
public class QuorumEvaluationService
{
    private const decimal BIG_LOAN_THRESHOLD = 5_000_000m;
    private const int SMALL_LOAN_REQUIRED_APPROVALS = 1;
    private const int BIG_LOAN_REQUIRED_APPROVALS = 3;

    /// <summary>
    /// Determines the number of approvals required based on loan size.
    /// </summary>
    /// <param name="principalAmount">Loan principal in UGX</param>
    /// <returns>Number of approvals required</returns>
    public static int GetRequiredApprovals(decimal principalAmount)
    {
        return principalAmount < BIG_LOAN_THRESHOLD 
            ? SMALL_LOAN_REQUIRED_APPROVALS 
            : BIG_LOAN_REQUIRED_APPROVALS;
    }

    /// <summary>
    /// Determines if the loan is classified as "big" (≥ 5M UGX).
    /// </summary>
    public static bool IsBigLoan(decimal principalAmount) 
        => principalAmount >= BIG_LOAN_THRESHOLD;

    /// <summary>
    /// Checks if the Chairperson has cast a REJECT vote (absolute veto).
    /// </summary>
    /// <param name="votes">List of committee votes</param>
    /// <returns>True if Chairperson voted REJECT</returns>
    public static bool HasChairpersonVeto(List<CommitteeVoteDetailDto> votes)
    {
        return votes.Any(v => 
            v.MemberRole?.Equals("Chairperson", StringComparison.OrdinalIgnoreCase) == true &&
            v.Vote?.Equals("REJECT", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// Checks if the required approvals include both Chairman and Treasurer (for big loans).
    /// </summary>
    /// <param name="votes">List of committee votes</param>
    /// <returns>True if both Chairman and Treasurer have approved</returns>
    public static bool HasRequiredMembersApproved(List<CommitteeVoteDetailDto> votes)
    {
        var chairmanApproved = votes.Any(v => 
            v.MemberRole?.Equals("Chairperson", StringComparison.OrdinalIgnoreCase) == true &&
            v.Vote?.Equals("APPROVE", StringComparison.OrdinalIgnoreCase) == true);

        var treasurerApproved = votes.Any(v => 
            v.MemberRole?.Equals("Treasurer", StringComparison.OrdinalIgnoreCase) == true &&
            v.Vote?.Equals("APPROVE", StringComparison.OrdinalIgnoreCase) == true);

        return chairmanApproved && treasurerApproved;
    }

    /// <summary>
    /// Checks if the committee has met the quorum requirement for the given loan.
    /// Returns detailed evaluation result.
    /// </summary>
    /// <param name="votes">List of committee votes cast</param>
    /// <param name="principalAmount">Loan principal amount</param>
    /// <returns>QuorumEvaluationResult with pass/fail status and reason</returns>
    public static QuorumEvaluationResult EvaluateQuorum(List<CommitteeVoteDetailDto> votes, decimal principalAmount)
    {
        // Check for Chairperson veto first (absolute block)
        if (HasChairpersonVeto(votes))
        {
            return new QuorumEvaluationResult
            {
                IsQuorumPassed = false,
                Reason = "Chairperson has voted REJECT — absolute veto applied regardless of other approvals.",
                IsBigLoan = IsBigLoan(principalAmount),
                RequiredApprovals = GetRequiredApprovals(principalAmount),
                ApprovalCount = votes.Count(v => v.Vote?.Equals("APPROVE", StringComparison.OrdinalIgnoreCase) == true),
                HasChairpersonVeto = true,
                HasRequiredMembers = false
            };
        }

        int requiredApprovals = GetRequiredApprovals(principalAmount);
        int approvalCount = votes.Count(v => v.Vote?.Equals("APPROVE", StringComparison.OrdinalIgnoreCase) == true);
        bool isBigLoan = IsBigLoan(principalAmount);

        // For big loans, ensure both Chairman and Treasurer approved
        if (isBigLoan)
        {
            if (approvalCount < requiredApprovals)
            {
                return new QuorumEvaluationResult
                {
                    IsQuorumPassed = false,
                    Reason = $"Big loan requires {requiredApprovals} approvals (currently {approvalCount}). Additionally, both Chairman and Treasurer must approve.",
                    IsBigLoan = true,
                    RequiredApprovals = requiredApprovals,
                    ApprovalCount = approvalCount,
                    HasChairpersonVeto = false,
                    HasRequiredMembers = HasRequiredMembersApproved(votes)
                };
            }

            if (!HasRequiredMembersApproved(votes))
            {
                return new QuorumEvaluationResult
                {
                    IsQuorumPassed = false,
                    Reason = "Big loan requires approval from both Chairman AND Treasurer. Not all required members have approved.",
                    IsBigLoan = true,
                    RequiredApprovals = requiredApprovals,
                    ApprovalCount = approvalCount,
                    HasChairpersonVeto = false,
                    HasRequiredMembers = false
                };
            }

            return new QuorumEvaluationResult
            {
                IsQuorumPassed = true,
                Reason = $"Big loan quorum passed: {approvalCount} approvals (≥{requiredApprovals} required), with Chairman and Treasurer approval.",
                IsBigLoan = true,
                RequiredApprovals = requiredApprovals,
                ApprovalCount = approvalCount,
                HasChairpersonVeto = false,
                HasRequiredMembers = true
            };
        }

        // For small loans, only check approval count
        bool passed = approvalCount >= requiredApprovals;
        return new QuorumEvaluationResult
        {
            IsQuorumPassed = passed,
            Reason = passed 
                ? $"Small loan quorum passed: {approvalCount} approval(s) received (1 required)."
                : $"Small loan requires 1 approval. Currently {approvalCount} approvals received.",
            IsBigLoan = false,
            RequiredApprovals = requiredApprovals,
            ApprovalCount = approvalCount,
            HasChairpersonVeto = false,
            HasRequiredMembers = false
        };
    }
}

/// <summary>
/// Result of quorum evaluation containing pass/fail status and detailed diagnostic information.
/// </summary>
public class QuorumEvaluationResult
{
    /// <summary>
    /// Whether the quorum requirement has been met.
    /// </summary>
    public bool IsQuorumPassed { get; set; }

    /// <summary>
    /// Human-readable explanation of the quorum status.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a big loan (≥ 5M UGX).
    /// </summary>
    public bool IsBigLoan { get; set; }

    /// <summary>
    /// Number of approvals required for this loan.
    /// </summary>
    public int RequiredApprovals { get; set; }

    /// <summary>
    /// Number of approvals currently received.
    /// </summary>
    public int ApprovalCount { get; set; }

    /// <summary>
    /// Whether the Chairperson has cast a veto (REJECT vote).
    /// </summary>
    public bool HasChairpersonVeto { get; set; }

    /// <summary>
    /// For big loans: whether both Chairman and Treasurer have approved.
    /// </summary>
    public bool HasRequiredMembers { get; set; }
}
