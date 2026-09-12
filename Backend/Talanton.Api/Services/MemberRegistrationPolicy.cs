namespace Talanton.Api.Services;

/// <summary>
/// Who may enrol whom.
///
/// Enrolling a borrower is registry work and belongs at the desk that handles files. Deciding who
/// sits on the board is a governance act and belongs to the board. That split is not bureaucracy:
/// whoever registers an account also sets its first password, so the power to create a committee
/// seat is the power to vote from it.
///
/// Without this, an underwriter could register a new Chairperson, sign in as them with the
/// password they had just chosen, and approve loans — walking straight through the seat checks on
/// voting and release. The controls are only worth as much as the enrolment behind them.
/// </summary>
public static class MemberRegistrationPolicy
{
    public const string Applicant = "Applicant";
    public const string Underwriter = "Underwriter";
    public const string Committee = "Committee";

    /// <summary>The portals each role may enrol someone into. Anyone absent may enrol nobody.</summary>
    private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        // The back office enrols borrowers, and only borrowers.
        ["underwriter"] = new[] { Applicant },

        // The board appoints staff and its own seats.
        ["committee"] = new[] { Applicant, Underwriter, Committee },
    };

    /// <summary>Whether this role may enrol anyone at all — used to decide whether to offer the screen.</summary>
    public static bool CanRegisterAnyone(string? actorPortalRole) =>
        !string.IsNullOrWhiteSpace(actorPortalRole) && Allowed.ContainsKey(actorPortalRole);

    /// <summary>The portals this role may enrol someone into, in the order a form should offer them.</summary>
    public static IReadOnlyList<string> PortalsRegisterableBy(string? actorPortalRole) =>
        actorPortalRole is not null && Allowed.TryGetValue(actorPortalRole, out var portals)
            ? portals
            : Array.Empty<string>();

    public static RegistrationPermission Evaluate(string? actorPortalRole, string? targetPortalRole)
    {
        if (string.IsNullOrWhiteSpace(actorPortalRole) || !Allowed.TryGetValue(actorPortalRole, out var allowed))
        {
            return Refused(
                "Only the underwriting desk or the committee may register members.");
        }

        var target = Normalize(targetPortalRole);
        if (target is null)
        {
            return Refused("Portal must be applicant, underwriter or committee.");
        }

        if (!allowed.Contains(target, StringComparer.OrdinalIgnoreCase))
        {
            // Name the actual boundary rather than a generic refusal, so the person reading it
            // knows who to ask instead of assuming the system is broken.
            return Refused(target.Equals(Committee, StringComparison.OrdinalIgnoreCase)
                ? "Only the committee may appoint a member to a board seat. A seat carries the power to " +
                  "approve loans and release funds, so it is the board's to grant."
                : $"The {actorPortalRole} portal may only register: " +
                  $"{string.Join(", ", allowed.Select(a => a.ToLowerInvariant()))}.");
        }

        return new RegistrationPermission { IsAllowed = true, TargetRole = target };
    }

    private static string? Normalize(string? portalRole) => portalRole?.Trim().ToLowerInvariant() switch
    {
        "applicant" => Applicant,
        "underwriter" => Underwriter,
        "committee" => Committee,
        _ => null,
    };

    private static RegistrationPermission Refused(string reason) =>
        new() { IsAllowed = false, Reason = reason };
}

public class RegistrationPermission
{
    public bool IsAllowed { get; init; }
    public string Reason { get; init; } = string.Empty;

    /// <summary>The normalised role name to store, set only when the registration is allowed.</summary>
    public string? TargetRole { get; init; }
}
