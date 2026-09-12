using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.DTOs;
using Talanton.Api.Models;
using Talanton.Api.Services;

namespace Talanton.Api.Controllers;

/// <summary>
/// Identity, and the provisioning of it.
///
/// Passwords are Supabase's problem now: the browser signs in against Supabase directly and sends
/// the resulting token here. This API never sees a password, so it cannot leak one. The old
/// password-checking endpoint has been removed rather than left in place — a second way to obtain
/// a session is a second thing to get wrong, and nothing was calling it.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly CurrentUserService _currentUser;
    private readonly SupabaseAdminService _admin;
    private readonly IConfiguration _configuration;

    public AuthController(
        ApplicationDbContext db,
        CurrentUserService currentUser,
        SupabaseAdminService admin,
        IConfiguration configuration)
    {
        _db = db;
        _currentUser = currentUser;
        _admin = admin;
        _configuration = configuration;
    }

    /// <summary>
    /// Who the caller is, as the SACCO understands them. The browser asks this immediately after
    /// signing in, because the portal someone belongs to is not theirs to choose — it used to be
    /// picked from a dropdown on the login screen, along with the committee seat that decides who
    /// may release money.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var user = await _currentUser.ResolveAsync(cancellationToken);

        if (user is null)
        {
            // The token is valid but this address is not a member of this SACCO. Membership is
            // granted offline; authenticating does not confer it.
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "This account is not registered with the SACCO. Ask an administrator to " +
                          "register you before signing in.",
            });
        }

        await _currentUser.NoteSignInAsync(user, cancellationToken);

        return Ok(new CurrentUserDto
        {
            Email = user.Email,
            FullName = user.FullName,
            PortalRole = user.PortalRole,
            CommitteeSeat = user.CommitteeSeat,
            MemberId = await ResolveMemberIdAsync(user, cancellationToken),
        });
    }

    /// <summary>
    /// Whether this deployment can create logins, and how well. The registration screen asks
    /// before showing a form that cannot work.
    /// </summary>
    [HttpGet("provisioning")]
    public async Task<ActionResult<object>> ProvisioningStatus(CancellationToken cancellationToken)
    {
        var actor = await _currentUser.ResolveAsync(cancellationToken);
        if (actor is null || !MemberRegistrationPolicy.CanRegisterAnyone(actor.PortalRole))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "Only the underwriting desk or the committee may register members.",
            });
        }

        // The form offers only what this caller may actually create, so nobody is invited to fill
        // in a registration that will be refused on submission.
        var registerable = MemberRegistrationPolicy.PortalsRegisterableBy(actor.PortalRole)
            .Select(r => r.ToLowerInvariant())
            .ToArray();

        return Ok(new
        {
            canCreateAccounts = _admin.IsConfigured,
            usingAdminKey = _admin.HasAdminKey,
            registerablePortals = registerable,
            seats = CurrentUserService.CommitteeSeats,
            message = _admin.IsConfigured
                ? (_admin.HasAdminKey
                    ? "Logins are created already confirmed."
                    : "Logins are created by ordinary sign-up, so they depend on the project's email " +
                      "confirmation setting. Set SUPABASE_SERVICE_ROLE_KEY for the stronger route.")
                : _admin.NotConfiguredMessage,
        });
    }

    /// <summary>
    /// Registers a member and creates the login that goes with it.
    ///
    /// SACCO membership is vetted offline, so there is no public sign-up: an administrator creates
    /// the account, and the new member signs in with the password they are given. This is the path
    /// real members take; the seeded accounts are ordinary rows created the same way.
    ///
    /// Requires SUPABASE_SERVICE_ROLE_KEY. That key can do anything to the project, so it lives
    /// here on the server and never reaches a browser.
    /// </summary>
    [HttpPost("accounts")]
    public async Task<ActionResult<object>> CreateAccount(
        [FromBody] CreateAccountDto dto, CancellationToken cancellationToken)
    {
        var actor = await _currentUser.ResolveAsync(cancellationToken);
        if (actor is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Not a registered member." });
        }

        // Who may enrol whom, decided before anything else. An applicant may enrol nobody; the
        // underwriting desk may enrol borrowers; only the board may appoint a board seat.
        //
        // This runs ahead of the configuration check deliberately: someone with no authority to
        // register anyone should be told that, not told about the server's Supabase setup. Answering
        // "not configured" to an unauthorised caller both leaks the deployment's state and misleads
        // them into thinking they would be allowed if only a key were set.
        var permission = MemberRegistrationPolicy.Evaluate(actor.PortalRole, dto.PortalRole);
        if (!permission.IsAllowed)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = permission.Reason });
        }

        var role = permission.TargetRole!;

        if (!_admin.IsConfigured)
        {
            return StatusCode(StatusCodes.Status501NotImplemented, new { message = _admin.NotConfiguredMessage });
        }

        var email = dto.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return BadRequest(new { message = "A valid email address is required." });
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            return BadRequest(new { message = "The member's full name is required." });
        }

        if (role == "Committee"
            && !string.IsNullOrWhiteSpace(dto.CommitteeSeat)
            && !CurrentUserService.CommitteeSeats.Contains(dto.CommitteeSeat, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = $"Committee seat must be one of: {string.Join(", ", CurrentUserService.CommitteeSeats)}.",
            });
        }

        if (await _db.Users.AnyAsync(u => u.Email.ToLower() == email, cancellationToken))
        {
            return Conflict(new { message = $"{email} is already registered with the SACCO." });
        }

        // Create the login first. If the member record fails afterwards they can be registered
        // again; a member record with no login is a member who cannot sign in and does not know why.
        var created = await _admin.CreateUserAsync(email, dto.Password, cancellationToken);
        if (!created.Ok)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new
            {
                message = $"Supabase would not create the login: {created.Error}",
            });
        }

        // A committee member's seat is carried as their name, which is the convention the vote and
        // release rules read.
        var displayName = role == "Committee" && !string.IsNullOrWhiteSpace(dto.CommitteeSeat)
            ? dto.CommitteeSeat!.Trim()
            : dto.FullName.Trim();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = displayName,
            PasswordHash = string.Empty, // Supabase holds the credential; nothing to store here.
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Users.Add(user);

        var roleRow = await _db.Roles.FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
        if (roleRow is null)
        {
            roleRow = new Role { Id = Guid.NewGuid(), Name = role, Description = $"{role} portal role" };
            _db.Roles.Add(roleRow);
        }

        _db.UserRoleAssignments.Add(new UserRoleAssignment
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleId = roleRow.Id,
            IsActive = true,
            AssignedAt = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            email,
            fullName = displayName,
            portalRole = role.ToLowerInvariant(),
            committeeSeat = role == "Committee" ? displayName : null,
            // Returned once and never stored: whoever registered the member has to be able to
            // pass it on, and the member changes it afterwards.
            initialPassword = created.InitialPassword,
            canSignInImmediately = created.CanSignInImmediately,
            message = created.CanSignInImmediately
                ? $"{email} can now sign in."
                : $"{email} was registered, but the login still needs confirming. {created.Error}",
        });
    }

    /// <summary>
    /// Issues a token for the local harness. Only exists when the API was started against the
    /// throwaway in-memory database, and says so loudly in the startup log.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("dev-token")]
    public ActionResult<object> DevToken([FromBody] DevTokenRequestDto dto)
    {
        var enabled = string.Equals(
            _configuration["USE_INMEMORY_DB"], "true", StringComparison.OrdinalIgnoreCase);

        if (!enabled)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            return BadRequest(new { message = "An email address is required." });
        }

        return Ok(new
        {
            accessToken = LocalDevTokens.Issue(dto.Email.Trim(), TimeSpan.FromHours(2)),
            note = "Local development token. Valid only against a throwaway in-memory database.",
        });
    }

    /// <summary>
    /// The applicant's membership number, when they have one — the key the loan pipeline uses.
    /// </summary>
    private async Task<string?> ResolveMemberIdAsync(CurrentUser user, CancellationToken cancellationToken)
    {
        var applicant = await _db.Applicants
            .FirstOrDefaultAsync(a => a.ApplicantUserId == user.UserId, cancellationToken);

        if (applicant is null) return null;

        var membership = await _db.SaccoMemberships
            .FirstOrDefaultAsync(m => m.ApplicantId == applicant.Id, cancellationToken);

        return membership?.MembershipNumber;
    }

}
