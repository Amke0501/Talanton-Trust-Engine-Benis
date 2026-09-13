using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// The committee's fixed 2:1 member limit: nobody borrows more than twice the savings the SACCO
/// holds for them.
///
/// It is deliberately not the same rule as the underwriting multiplier. That one is a dial an
/// underwriter sets per applicant — three times savings by default — and a file can satisfy it and
/// still breach this. The gap between the two is the reason the committee checks again, so the
/// first test below pins exactly that case.
/// </summary>
public class MemberLimitTests
{
    [Fact]
    public void The_limit_is_two_to_one_not_the_underwriting_multiplier()
    {
        // 3,000,000 against 1,000,000 savings passes a 3x underwriting cap exactly…
        var underwriting = MemberSavingsService.Evaluate(3_000_000m, 1_000_000m, multiplier: 3.0m);
        Assert.True(underwriting.IsWithinCap);

        // …and still breaches the committee's 2:1 limit, which is the whole point of checking again.
        var committee = MemberSavingsService.EvaluateMemberLimit(3_000_000m, 1_000_000m);
        Assert.False(committee.IsWithinLimit);
        Assert.Equal(1_000_000m, committee.ShortfallAmount);
    }

    [Fact]
    public void Exactly_twice_savings_is_within_the_limit()
    {
        var result = MemberSavingsService.EvaluateMemberLimit(2_000_000m, 1_000_000m);

        Assert.True(result.IsWithinLimit);
        Assert.Equal(0m, result.ShortfallAmount);
        Assert.Equal(2_000_000m, result.MaximumBorrowable);
    }

    [Fact]
    public void A_shilling_over_twice_savings_is_not()
    {
        var result = MemberSavingsService.EvaluateMemberLimit(2_000_001m, 1_000_000m);

        Assert.False(result.IsWithinLimit);
        Assert.Equal(1m, result.ShortfallAmount);
    }

    [Fact]
    public void The_flag_names_what_would_close_the_gap()
    {
        var result = MemberSavingsService.EvaluateMemberLimit(5_000_000m, 1_000_000m);

        // The wording the committee asked for, to the letter.
        Assert.Equal("Exceeds 2:1 Member Limit. Requires 3,000,000 in Guarantor Deposits.", result.Message);
    }

    [Fact]
    public void A_member_with_no_savings_can_borrow_nothing_on_their_own()
    {
        var result = MemberSavingsService.EvaluateMemberLimit(1m, 0m);

        Assert.False(result.IsWithinLimit);
        Assert.Equal(1m, result.ShortfallAmount);
        Assert.Equal(0m, result.MaximumBorrowable);
    }

    [Fact]
    public void The_shortfall_is_what_guarantors_must_cover_not_the_whole_loan()
    {
        var result = MemberSavingsService.EvaluateMemberLimit(10_000_000m, 3_000_000m);

        // Limit is 6,000,000, so 4,000,000 needs covering — not the full 10,000,000.
        Assert.Equal(6_000_000m, result.MaximumBorrowable);
        Assert.Equal(4_000_000m, result.ShortfallAmount);
    }

    [Fact]
    public void The_ratio_is_stated_once_and_used_everywhere()
    {
        Assert.Equal(2.0m, MemberSavingsService.MemberLimitRatio);

        var result = MemberSavingsService.EvaluateMemberLimit(100m, 50m);
        Assert.Equal(50m * MemberSavingsService.MemberLimitRatio, result.MaximumBorrowable);
    }
}
