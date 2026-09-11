using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Models;

namespace Talanton.Api.Services;

/// <summary>
/// Turns a verified Supabase token into the person the SACCO knows about.
///
/// Supabase answers one question — "is this really the holder of this email address?" — and it
/// answers it cryptographically. It knows nothing about portals, committee seats or membership.
/// Those live in this database and are resolved here, so a caller cannot assert their own
/// authority: previously the committee seat arrived in the request body, which meant anyone could
/// claim to be the Treasurer and release funds.
///
/// Membership is vetted offline, so a Supabase account with no matching user record is refused.
/// Authenticating is not the same as being a member.
/// </summary>
public class CurrentUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(ApplicationDbContext context, IHttpContextAccessor accessor)
    {
        _context = context;
        _accessor = accessor;
    }

    /// <summary>The seats that exist on the board; a committee user holds exactly one.</summary>
    public static readonly string[] CommitteeSeats =
        { "Chairperson", "Treasurer", "Secretary", "Credit Officer", "Board Member" };

    public async Task<CurrentUser?> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var principal = _accessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var email = ReadEmail(principal);
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var roleName = await _context.UserRoleAssignments
            .Where(a => a.UserId == user.Id && a.IsActive)
            .Join(_context.Roles, a => a.RoleId, r => r.Id, (_, r) => r.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return null;
        }

        // A committee member's seat is their account name — the same convention the vote and
        // release rules already use. Anyone outside the committee has no seat.
        var seat = CommitteeSeats.FirstOrDefault(
            s => s.Equals(user.FullName, StringComparison.OrdinalIgnoreCase));

        return new CurrentUser
        {
            UserId = user.Id,
            SupabaseUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            Email = user.Email,
            FullName = user.FullName,
            PortalRole = roleName.ToLowerInvariant(),
            CommitteeSeat = roleName.Equals("Committee", StringComparison.OrdinalIgnoreCase) ? seat : null,
        };
    }

    /// <summary>
    /// Records that this account signed in, and links it to its Supabase identity the first time.
    /// </summary>
    public async Task NoteSignInAsync(CurrentUser user, CancellationToken cancellationToken = default)
    {
        try
        {
            var row = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.UserId, cancellationToken);
            if (row is null) return;

            row.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Never fail a sign-in over bookkeeping.
            Console.WriteLine($"[WARN] Could not record the sign-in for {user.Email} " +
                              $"({ex.GetType().Name}): {ex.Message}");
        }
    }

    private static string? ReadEmail(ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Email)
        ?? principal.FindFirstValue("email")
        ?? principal.FindFirstValue("user_email");
}

/// <summary>Who is making this request, as the SACCO understands them.</summary>
public class CurrentUser
{
    public Guid UserId { get; init; }
    public string SupabaseUserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;

    /// <summary>applicant | underwriter | committee</summary>
    public string PortalRole { get; init; } = string.Empty;

    /// <summary>The board seat, for committee members only. Null for everyone else.</summary>
    public string? CommitteeSeat { get; init; }

    public bool IsCommittee => PortalRole.Equals("committee", StringComparison.OrdinalIgnoreCase);
}
