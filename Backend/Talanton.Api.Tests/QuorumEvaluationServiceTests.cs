using Talanton.Api.DTOs;
using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// The committee voting rules the founder confirmed on 6 August:
/// under 5M UGX needs one approval; 5M and above needs three including both the Chairperson and
/// the Treasurer; and a Chairperson REJECT blocks the file however many others approved.
/// </summary>
public class QuorumEvaluationServiceTests
{
    private static CommitteeVoteDetailDto Vote(string role, string vote) =>
        new() { MemberName = role, MemberRole = role, Vote = vote };

    [Theory]
    [InlineData(4_999_999, 1)]
    [InlineData(5_000_000, 3)]
    [InlineData(15_000_000, 3)]
    public void RequiredApprovals_turn_on_the_five_million_threshold(decimal principal, int expected)
    {
        Assert.Equal(expected, QuorumEvaluationService.GetRequiredApprovals(principal));
    }

    [Fact]
    public void Five_million_exactly_counts_as_a_big_loan()
    {
        // The boundary the rule hinges on: "under 5M" is small, 5M itself is not.
        Assert.False(QuorumEvaluationService.IsBigLoan(4_999_999m));
        Assert.True(QuorumEvaluationService.IsBigLoan(5_000_000m));
    }

    [Fact]
    public void Small_loan_passes_on_a_single_approval()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(
            new List<CommitteeVoteDetailDto> { Vote("Credit Officer", "APPROVE") }, 3_000_000m);

        Assert.True(result.IsQuorumPassed);
        Assert.Equal(1, result.ApprovalCount);
    }

    [Fact]
    public void Small_loan_with_no_approvals_is_blocked()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(
            new List<CommitteeVoteDetailDto> { Vote("Credit Officer", "ABSTAIN") }, 3_000_000m);

        Assert.False(result.IsQuorumPassed);
    }

    [Fact]
    public void Big_loan_is_blocked_with_three_approvals_that_exclude_the_treasurer()
    {
        // The case the report's test steps describe: enough votes, wrong people.
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>
        {
            Vote("Chairperson", "APPROVE"),
            Vote("Credit Officer", "APPROVE"),
            Vote("Board Member", "APPROVE"),
            Vote("Treasurer", "ABSTAIN"),
        }, 15_000_000m);

        Assert.False(result.IsQuorumPassed);
        Assert.False(result.HasRequiredMembers);
        Assert.Equal(3, result.ApprovalCount);
    }

    [Fact]
    public void Big_loan_passes_with_three_approvals_including_chairperson_and_treasurer()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>
        {
            Vote("Chairperson", "APPROVE"),
            Vote("Treasurer", "APPROVE"),
            Vote("Credit Officer", "APPROVE"),
        }, 15_000_000m);

        Assert.True(result.IsQuorumPassed);
        Assert.True(result.HasRequiredMembers);
    }

    [Fact]
    public void Big_loan_with_chairperson_and_treasurer_but_only_two_approvals_is_blocked()
    {
        // Both required members approved, but the count is still short of three.
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>
        {
            Vote("Chairperson", "APPROVE"),
            Vote("Treasurer", "APPROVE"),
        }, 15_000_000m);

        Assert.False(result.IsQuorumPassed);
    }

    [Fact]
    public void Chairperson_rejection_vetoes_a_file_that_otherwise_has_quorum()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>
        {
            Vote("Chairperson", "REJECT"),
            Vote("Treasurer", "APPROVE"),
            Vote("Credit Officer", "APPROVE"),
            Vote("Board Member", "APPROVE"),
            Vote("Risk Head", "APPROVE"),
        }, 15_000_000m);

        Assert.False(result.IsQuorumPassed);
        Assert.True(result.HasChairpersonVeto);
    }

    [Fact]
    public void Chairperson_veto_also_blocks_a_small_loan_that_would_otherwise_pass()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>
        {
            Vote("Chairperson", "REJECT"),
            Vote("Credit Officer", "APPROVE"),
        }, 1_000_000m);

        Assert.False(result.IsQuorumPassed);
        Assert.True(result.HasChairpersonVeto);
    }

    [Fact]
    public void Votes_are_matched_without_regard_to_case()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(
            new List<CommitteeVoteDetailDto> { Vote("chairperson", "approve") }, 1_000_000m);

        Assert.True(result.IsQuorumPassed);
    }

    [Fact]
    public void No_votes_at_all_blocks_the_file()
    {
        var result = QuorumEvaluationService.EvaluateQuorum(new List<CommitteeVoteDetailDto>(), 1_000_000m);
        Assert.False(result.IsQuorumPassed);
    }
}
