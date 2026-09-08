using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// The per-member borrowing cap: a loan measured against that member's own savings, separate from
/// whether the SACCO as a whole holds enough cash.
/// </summary>
public class MemberSavingsServiceTests
{
    [Fact]
    public void A_loan_within_the_multiple_of_savings_passes()
    {
        var r = MemberSavingsService.Evaluate(requestedPrincipal: 9_000_000m, recordedSavings: 3_000_000m, multiplier: 3m);

        Assert.True(r.IsWithinCap);
        Assert.Equal(9_000_000m, r.MaximumBorrowable);
        Assert.Equal(0m, r.ExcessAmount);
    }

    [Fact]
    public void Borrowing_exactly_the_cap_is_allowed()
    {
        var r = MemberSavingsService.Evaluate(9_000_000m, 3_000_000m, 3m);
        Assert.True(r.IsWithinCap);
    }

    [Fact]
    public void A_loan_beyond_the_cap_is_refused_and_the_excess_reported()
    {
        var r = MemberSavingsService.Evaluate(12_000_000m, 3_000_000m, 3m);

        Assert.False(r.IsWithinCap);
        Assert.Equal(3_000_000m, r.ExcessAmount);
        Assert.Contains("Exceeds", r.Reason);
    }

    [Fact]
    public void A_member_with_no_savings_can_borrow_nothing_on_this_rule()
    {
        var r = MemberSavingsService.Evaluate(1m, 0m, 3m);

        Assert.False(r.IsWithinCap);
        Assert.Equal(0m, r.MaximumBorrowable);
    }

    [Fact]
    public void A_missing_multiplier_refuses_rather_than_computing_a_zero_cap_silently()
    {
        var r = MemberSavingsService.Evaluate(5_000_000m, 3_000_000m, 0m);

        Assert.False(r.IsWithinCap);
        Assert.Contains("cannot be worked out", r.Reason);
    }

    [Fact]
    public void An_application_overstating_savings_is_flagged()
    {
        // The case the rule exists to catch: the applicant claims more savings than the SACCO holds.
        var r = MemberSavingsService.Evaluate(
            requestedPrincipal: 9_000_000m, recordedSavings: 1_000_000m, multiplier: 3m, declaredSavings: 3_000_000m);

        Assert.True(r.DeclaredSavingsMismatch);
        Assert.Equal(2_000_000m, r.DeclaredSavingsDifference);
        Assert.False(r.IsWithinCap);
        Assert.Contains("more than the SACCO's record", r.Reason);
    }

    [Fact]
    public void The_cap_is_measured_against_the_record_not_the_declared_figure()
    {
        // Declared savings would allow this; the SACCO's record does not. The record wins.
        var r = MemberSavingsService.Evaluate(9_000_000m, 1_000_000m, 3m, declaredSavings: 3_000_000m);

        Assert.False(r.IsWithinCap);
        Assert.Equal(3_000_000m, r.MaximumBorrowable);
    }

    [Fact]
    public void Understating_savings_is_flagged_too()
    {
        var r = MemberSavingsService.Evaluate(1_000_000m, 3_000_000m, 3m, declaredSavings: 1_000_000m);

        Assert.True(r.DeclaredSavingsMismatch);
        Assert.Equal(-2_000_000m, r.DeclaredSavingsDifference);
        Assert.True(r.IsWithinCap);
        Assert.Contains("less than the SACCO's record", r.Reason);
    }

    [Fact]
    public void Matching_figures_raise_no_mismatch()
    {
        var r = MemberSavingsService.Evaluate(3_000_000m, 3_000_000m, 3m, declaredSavings: 3_000_000m);
        Assert.False(r.DeclaredSavingsMismatch);
    }
}
