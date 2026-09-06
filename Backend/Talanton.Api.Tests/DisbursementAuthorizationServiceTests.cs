using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// Who may release funds: the Treasurer alone for small loans, and both the Chairperson and the
/// Secretary for big ones. This is the rule the progress report flagged as untestable, so these
/// cover the refusals as carefully as the approvals.
/// </summary>
public class DisbursementAuthorizationServiceTests
{
    [Fact]
    public void Treasurer_may_release_a_small_loan()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(3_000_000m, "Treasurer");
        Assert.True(result.IsAuthorized);
    }

    [Theory]
    [InlineData("Chairperson")]
    [InlineData("Secretary")]
    [InlineData("Credit Officer")]
    [InlineData("Board Member")]
    public void Everyone_other_than_the_treasurer_is_refused_a_small_loan(string role)
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(3_000_000m, role);

        Assert.False(result.IsAuthorized);
        Assert.Contains("Treasurer", result.Reason);
    }

    [Fact]
    public void A_request_with_no_role_is_refused()
    {
        // Regression: `!role?.Equals(...) == true` lifted to bool?, so a null role produced null
        // rather than true, the deny branch was skipped, and the release was authorized.
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(3_000_000m, null);
        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void An_empty_role_is_refused()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(3_000_000m, "");
        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void Role_matching_ignores_case()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(3_000_000m, "treasurer");
        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void Big_loan_needs_both_signatures_present()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(
            15_000_000m, "Chairperson", chairpersonApprovalPresent: true, secretaryApprovalPresent: false);

        Assert.False(result.IsAuthorized);
        Assert.Contains("Secretary", result.Reason);
    }

    [Fact]
    public void Big_loan_with_both_signatures_is_released_by_the_chairperson()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(
            15_000_000m, "Chairperson", chairpersonApprovalPresent: true, secretaryApprovalPresent: true);

        Assert.True(result.IsAuthorized);
    }

    [Fact]
    public void Big_loan_with_both_signatures_is_still_refused_to_the_treasurer()
    {
        // Signatures being on file does not make any role the one who may execute the release.
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(
            15_000_000m, "Treasurer", chairpersonApprovalPresent: true, secretaryApprovalPresent: true);

        Assert.False(result.IsAuthorized);
    }

    [Fact]
    public void The_treasurer_rule_does_not_leak_into_big_loans()
    {
        var result = DisbursementAuthorizationService.EvaluateDisbursementAuthority(5_000_000m, "Treasurer");
        Assert.False(result.IsAuthorized);
    }
}
