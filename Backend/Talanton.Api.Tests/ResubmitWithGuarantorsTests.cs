using Talanton.Api.DTOs;
using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// The rule for counting additional guarantors when an applicant asks for a declined offer to be
/// reconsidered: only guarantors not already on the file count, since re-listing existing ones
/// adds no security.
/// </summary>
public class ResubmitWithGuarantorsTests
{
    private static GuarantorDto G(string memberId, decimal pledged = 1_000_000m) =>
        new() { Id = memberId, Name = memberId, MemberId = memberId, PledgedShares = pledged, AvailableShares = pledged };

    /// <summary>Mirrors the filtering in LoanApplicationService.ResubmitWithGuarantorsAsync.</summary>
    private static List<GuarantorDto> CountableAdditions(List<GuarantorDto> existing, List<GuarantorDto> supplied)
    {
        var existingIds = existing.Select(g => g.MemberId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return supplied
            .Where(g => !string.IsNullOrWhiteSpace(g.MemberId) && !existingIds.Contains(g.MemberId))
            .GroupBy(g => g.MemberId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();
    }

    [Fact]
    public void The_requirement_is_two_additional_guarantors()
    {
        Assert.Equal(2, LoanApplicationService.MinimumAdditionalGuarantors);
    }

    [Fact]
    public void Two_genuinely_new_guarantors_satisfy_the_requirement()
    {
        var additions = CountableAdditions(new List<GuarantorDto> { G("M-1") }, new List<GuarantorDto> { G("M-2"), G("M-3") });
        Assert.Equal(2, additions.Count);
    }

    [Fact]
    public void Re_listing_an_existing_guarantor_does_not_count()
    {
        // Adding no security must not satisfy a rule that exists to add security.
        var additions = CountableAdditions(
            new List<GuarantorDto> { G("M-1"), G("M-2") },
            new List<GuarantorDto> { G("M-1"), G("M-2"), G("M-3") });

        Assert.Single(additions);
        Assert.Equal("M-3", additions[0].MemberId);
    }

    [Fact]
    public void The_same_new_guarantor_listed_twice_counts_once()
    {
        var additions = CountableAdditions(new List<GuarantorDto>(), new List<GuarantorDto> { G("M-9"), G("M-9") });
        Assert.Single(additions);
    }

    [Fact]
    public void Existing_guarantors_are_matched_without_regard_to_case()
    {
        var additions = CountableAdditions(new List<GuarantorDto> { G("m-1") }, new List<GuarantorDto> { G("M-1"), G("M-2") });

        Assert.Single(additions);
        Assert.Equal("M-2", additions[0].MemberId);
    }

    [Fact]
    public void Guarantors_without_a_member_id_are_ignored()
    {
        var additions = CountableAdditions(new List<GuarantorDto>(), new List<GuarantorDto> { G(""), G("   "), G("M-4") });
        Assert.Single(additions);
    }

    [Fact]
    public void Coverage_is_recomputed_across_old_and_new_guarantors_together()
    {
        var all = new List<GuarantorDto> { G("M-1", 2_000_000m), G("M-2", 3_000_000m), G("M-3", 4_000_000m) };
        var coverage = GuarantorShareLockingService.EvaluateGuarantorCoverage(all, 12_000_000m, 3_000_000m);

        Assert.Equal(9_000_000m, coverage.LoanGap);
        Assert.Equal(9_000_000m, coverage.TotalPledgedShares);
        Assert.True(coverage.IsCovered);
    }
}
