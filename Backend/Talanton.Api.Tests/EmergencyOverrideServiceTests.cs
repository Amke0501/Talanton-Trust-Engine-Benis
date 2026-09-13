using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// The dual-key release. Two officers, both key holders, both different, with a written reason —
/// each of those is a separate way for a single person to release money on their own, so each
/// gets its own test.
/// </summary>
public class EmergencyOverrideServiceTests
{
    private const string GoodReason = "Board resolution 14/2026 authorises bridging finance.";

    [Fact]
    public void TwoDifferentKeyHoldersWithAReasonAreAuthorized()
    {
        var result = EmergencyOverrideService.Evaluate("Chairperson", "Treasurer", GoodReason);

        Assert.True(result.IsAuthorized);
        Assert.Equal("Chairperson", result.FirstSeat);
        Assert.Equal("Treasurer", result.SecondSeat);
        Assert.Equal(GoodReason, result.Reason);
    }

    [Fact]
    public void OneOfficerCannotTurnBothKeys()
    {
        var result = EmergencyOverrideService.Evaluate("Chairperson", "Chairperson", GoodReason);

        Assert.False(result.IsAuthorized);
        Assert.Contains("two different officers", result.Explanation);
    }

    [Fact]
    public void SameOfficerIsRefusedRegardlessOfCasing()
    {
        var result = EmergencyOverrideService.Evaluate("chairperson", "CHAIRPERSON", GoodReason);

        Assert.False(result.IsAuthorized);
        Assert.Contains("two different officers", result.Explanation);
    }

    [Theory]
    [InlineData("Secretary")]
    [InlineData("Credit Officer")]
    [InlineData("Board Member")]
    [InlineData("Underwriter")]
    public void SeatsWithoutAKeyAreRefused(string seat)
    {
        var result = EmergencyOverrideService.Evaluate("Chairperson", seat, GoodReason);

        Assert.False(result.IsAuthorized);
        Assert.Contains("does not hold an emergency release key", result.Explanation);
    }

    [Fact]
    public void BothSignaturesAreRequired()
    {
        Assert.False(EmergencyOverrideService.Evaluate("Chairperson", null, GoodReason).IsAuthorized);
        Assert.False(EmergencyOverrideService.Evaluate(null, "Treasurer", GoodReason).IsAuthorized);
        Assert.False(EmergencyOverrideService.Evaluate("  ", "Treasurer", GoodReason).IsAuthorized);
    }

    [Fact]
    public void ATokenReasonIsNotAReason()
    {
        var result = EmergencyOverrideService.Evaluate("Chairperson", "Treasurer", "urgent");

        Assert.False(result.IsAuthorized);
        Assert.Contains("written reason", result.Explanation);
    }

    [Fact]
    public void AMissingReasonIsRefused()
    {
        Assert.False(EmergencyOverrideService.Evaluate("Chairperson", "Treasurer", null).IsAuthorized);
        Assert.False(EmergencyOverrideService.Evaluate("Chairperson", "Treasurer", "   ").IsAuthorized);
    }

    [Fact]
    public void TheReasonIsTrimmedBeforeItIsRecorded()
    {
        var result = EmergencyOverrideService.Evaluate("Treasurer", "Chairperson", $"  {GoodReason}  ");

        Assert.True(result.IsAuthorized);
        Assert.Equal(GoodReason, result.Reason);
    }

    [Fact]
    public void OnlyTheChairmanAndTreasurerHoldKeys()
    {
        // The specification names these two and nobody else. The Secretary countersigns a big-loan
        // release, which is a different rule — it does not make them a key holder here.
        Assert.Equal(new[] { "Chairperson", "Treasurer" }, EmergencyOverrideService.KeyHolderSeats);

        var result = EmergencyOverrideService.Evaluate("Treasurer", "Secretary", GoodReason);
        Assert.False(result.IsAuthorized);
        Assert.Contains("does not hold an emergency release key", result.Explanation);
    }

    [Fact]
    public void TheOnlyValidPairingIsTheChairmanAndTheTreasurer()
    {
        Assert.True(EmergencyOverrideService.Evaluate("Chairperson", "Treasurer", GoodReason).IsAuthorized);
        Assert.True(EmergencyOverrideService.Evaluate("Treasurer", "Chairperson", GoodReason).IsAuthorized);
    }

    [Fact]
    public void AnySeatPairingAmongKeyHoldersIsAccepted()
    {
        foreach (var first in EmergencyOverrideService.KeyHolderSeats)
        {
            foreach (var second in EmergencyOverrideService.KeyHolderSeats)
            {
                var result = EmergencyOverrideService.Evaluate(first, second, GoodReason);
                Assert.Equal(first != second, result.IsAuthorized);
            }
        }
    }
}
