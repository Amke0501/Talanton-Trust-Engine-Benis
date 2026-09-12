using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// Who may enrol whom.
///
/// The case that matters most is the one that reads like a formality: an underwriter creating a
/// committee account. Whoever registers an account also sets its first password, so being able to
/// create a Chairperson is being able to vote as the Chairperson — which walks straight past every
/// seat check on voting and release. That is the escalation these tests exist to hold shut.
/// </summary>
public class MemberRegistrationPolicyTests
{
    // ── The escalation path ───────────────────────────────────────────────────────────────────

    [Fact]
    public void An_underwriter_cannot_create_a_committee_account()
    {
        var result = MemberRegistrationPolicy.Evaluate("underwriter", "committee");

        Assert.False(result.IsAllowed);
        Assert.Contains("Only the committee may appoint", result.Reason);
    }

    [Fact]
    public void An_underwriter_cannot_create_another_underwriter()
    {
        // Less severe than a board seat, but still staff appointing staff.
        Assert.False(MemberRegistrationPolicy.Evaluate("underwriter", "underwriter").IsAllowed);
    }

    [Fact]
    public void The_refusal_explains_who_to_ask_instead()
    {
        var result = MemberRegistrationPolicy.Evaluate("underwriter", "committee");

        // A refusal that only says "forbidden" gets read as a broken system.
        Assert.Contains("board", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    // ── What each portal may do ───────────────────────────────────────────────────────────────

    [Fact]
    public void An_underwriter_may_enrol_borrowers()
    {
        var result = MemberRegistrationPolicy.Evaluate("underwriter", "applicant");

        Assert.True(result.IsAllowed);
        Assert.Equal("Applicant", result.TargetRole);
    }

    [Theory]
    [InlineData("applicant", "Applicant")]
    [InlineData("underwriter", "Underwriter")]
    [InlineData("committee", "Committee")]
    public void The_committee_may_enrol_anyone(string target, string expected)
    {
        var result = MemberRegistrationPolicy.Evaluate("committee", target);

        Assert.True(result.IsAllowed);
        Assert.Equal(expected, result.TargetRole);
    }

    [Theory]
    [InlineData("applicant")]
    [InlineData("underwriter")]
    [InlineData("committee")]
    public void An_applicant_may_enrol_nobody(string target)
    {
        Assert.False(MemberRegistrationPolicy.Evaluate("applicant", target).IsAllowed);
    }

    [Fact]
    public void An_unknown_or_missing_role_may_enrol_nobody()
    {
        Assert.False(MemberRegistrationPolicy.Evaluate(null, "applicant").IsAllowed);
        Assert.False(MemberRegistrationPolicy.Evaluate("", "applicant").IsAllowed);
        Assert.False(MemberRegistrationPolicy.Evaluate("auditor", "applicant").IsAllowed);
    }

    // ── Input handling ────────────────────────────────────────────────────────────────────────

    [Fact]
    public void Portal_names_are_matched_regardless_of_casing_or_padding()
    {
        Assert.True(MemberRegistrationPolicy.Evaluate("COMMITTEE", "  Applicant ").IsAllowed);
        Assert.True(MemberRegistrationPolicy.Evaluate("Underwriter", "APPLICANT").IsAllowed);
    }

    [Fact]
    public void An_unrecognised_target_portal_is_rejected_before_anything_else()
    {
        var result = MemberRegistrationPolicy.Evaluate("committee", "administrator");

        Assert.False(result.IsAllowed);
        Assert.Contains("applicant, underwriter or committee", result.Reason);
    }

    // ── What the registration form is told it may offer ───────────────────────────────────────

    [Fact]
    public void The_form_offers_only_what_the_caller_may_create()
    {
        Assert.Equal(new[] { "Applicant" },
            MemberRegistrationPolicy.PortalsRegisterableBy("underwriter"));

        Assert.Equal(new[] { "Applicant", "Underwriter", "Committee" },
            MemberRegistrationPolicy.PortalsRegisterableBy("committee"));

        Assert.Empty(MemberRegistrationPolicy.PortalsRegisterableBy("applicant"));
        Assert.Empty(MemberRegistrationPolicy.PortalsRegisterableBy(null));
    }

    [Fact]
    public void Only_staff_portals_are_offered_the_screen_at_all()
    {
        Assert.True(MemberRegistrationPolicy.CanRegisterAnyone("underwriter"));
        Assert.True(MemberRegistrationPolicy.CanRegisterAnyone("committee"));
        Assert.False(MemberRegistrationPolicy.CanRegisterAnyone("applicant"));
        Assert.False(MemberRegistrationPolicy.CanRegisterAnyone(null));
    }

    /// <summary>
    /// Whatever the form is told it may offer must be exactly what the server will accept. If these
    /// ever diverge, a staff member fills in a form that is refused on submission.
    /// </summary>
    [Theory]
    [InlineData("underwriter")]
    [InlineData("committee")]
    public void The_offered_list_matches_what_is_actually_permitted(string actor)
    {
        foreach (var portal in new[] { "applicant", "underwriter", "committee" })
        {
            var offered = MemberRegistrationPolicy.PortalsRegisterableBy(actor)
                .Contains(portal, StringComparer.OrdinalIgnoreCase);

            Assert.Equal(offered, MemberRegistrationPolicy.Evaluate(actor, portal).IsAllowed);
        }
    }
}
