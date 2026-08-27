using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.DTOs;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AuthController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var portalRole = NormalizeRole(request.PortalRole);
        if (portalRole is null)
        {
            return BadRequest(new { message = "Invalid portal selection." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(
            u => u.Email.ToLower() == email,
            cancellationToken);

        if (user is null || !user.IsActive || user.PasswordHash != request.Password)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var assignedRoleName = await _db.UserRoleAssignments
            .Where(a => a.UserId == user.Id && a.IsActive)
            .Join(_db.Roles, a => a.RoleId, r => r.Id, (_, r) => r.Name)
            .OrderByDescending(role => role)
            .FirstOrDefaultAsync(cancellationToken);

        var assignedRole = NormalizeRole(assignedRoleName);
        if (assignedRole is null)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "This account does not have an active portal role assignment."
            });
        }

        if (!string.Equals(assignedRole, portalRole, StringComparison.OrdinalIgnoreCase))
        {
            var displayRole = ToDisplayRole(assignedRole);
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = $"This account is registered as an {displayRole}. Please sign in through the {displayRole} Portal."
            });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponseDto
        {
            Email = user.Email,
            FullName = user.FullName,
            Role = assignedRole,
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out." });
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "applicant" => "applicant",
            "underwriter" => "underwriter",
            "committee" => "committee",
            _ => null,
        };
    }

    private static string ToDisplayRole(string role)
    {
        return role switch
        {
            "applicant" => "Applicant",
            "underwriter" => "Underwriter",
            "committee" => "Committee",
            _ => role,
        };
    }
}
