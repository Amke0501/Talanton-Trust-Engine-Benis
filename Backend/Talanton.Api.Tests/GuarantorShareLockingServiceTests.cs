using Talanton.Api.DTOs;
using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// Pledged shares are set aside when a loan is released, so the same shares cannot back a second
/// loan at the same time, and are re-credited when it is repaid.
/// </summary>
public class GuarantorShareLockingServiceTests
{
    private static GuarantorDto Guarantor(string name, decimal pledged, decimal available) =>
        new() { Id = name, Name = name, MemberId = name, PledgedShares = pledged, AvailableShares = available };

    [Fact]
    public void Locking_deducts_the_pledge_from_what_remains_available()
    {
        var locked = GuarantorShareLockingService.LockGuarantorShares(
            new List<GuarantorDto> { Guarantor("Kato", 5_000_000m, 8_000_000m) });

        Assert.Equal(3_000_000m, locked[0].AvailableShares);
        Assert.Equal(5_000_000m, locked[0].PledgedShares);
    }

    [Fact]
    public void Locking_never_drives_the_available_balance_below_zero()
    {
        var locked = GuarantorShareLockingService.LockGuarantorShares(
            new List<GuarantorDto> { Guarantor("Kato", 9_000_000m, 4_000_000m) });

        Assert.Equal(0m, locked[0].AvailableShares);
    }

    [Fact]
    public void Unlocking_restores_what_locking_removed()
    {
        var original = new List<GuarantorDto> { Guarantor("Kato", 5_000_000m, 8_000_000m) };
        var restored = GuarantorShareLockingService.UnlockGuarantorShares(
            GuarantorShareLockingService.LockGuarantorShares(original));

        Assert.Equal(8_000_000m, restored[0].AvailableShares);
    }

    [Fact]
    public void A_guarantor_cannot_pledge_more_than_they_have()
    {
        var result = GuarantorShareLockingService.ValidateGuarantorCapacity(
            Guarantor("Kato", 0m, 2_000_000m), 5_000_000m);

        Assert.False(result.IsValid);
        Assert.Contains("insufficient", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Pledging_exactly_the_available_balance_is_allowed()
    {
        var result = GuarantorShareLockingService.ValidateGuarantorCapacity(
            Guarantor("Kato", 0m, 5_000_000m), 5_000_000m);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Coverage_measures_pledges_against_the_gap_left_by_savings()
    {
        // 15M requested against 4M saved leaves an 11M gap; 13M pledged covers it.
        var coverage = GuarantorShareLockingService.EvaluateGuarantorCoverage(
            new List<GuarantorDto> { Guarantor("Kato", 8_000_000m, 8_000_000m), Guarantor("Sarah", 5_000_000m, 9_500_000m) },
            15_000_000m, 4_000_000m);

        Assert.True(coverage.IsCovered);
        Assert.Equal(11_000_000m, coverage.LoanGap);
        Assert.Equal(13_000_000m, coverage.TotalPledgedShares);
        Assert.Equal(0m, coverage.Deficit);
    }

    [Fact]
    public void Coverage_reports_the_shortfall_when_pledges_fall_short()
    {
        var coverage = GuarantorShareLockingService.EvaluateGuarantorCoverage(
            new List<GuarantorDto> { Guarantor("Kato", 2_000_000m, 2_000_000m) },
            15_000_000m, 4_000_000m);

        Assert.False(coverage.IsCovered);
        Assert.Equal(9_000_000m, coverage.Deficit);
    }

    [Fact]
    public void Savings_covering_the_whole_principal_leaves_no_gap_to_guarantee()
    {
        var coverage = GuarantorShareLockingService.EvaluateGuarantorCoverage(
            new List<GuarantorDto>(), 4_000_000m, 10_000_000m);

        Assert.True(coverage.IsCovered);
        Assert.Equal(0m, coverage.LoanGap);
    }
}
