using Microsoft.EntityFrameworkCore;
using Talanton.Api.Models;

namespace Talanton.Api.Data;

public static class DemoUsersSeeder
{
    private sealed record DemoUserSeed(string Email, string Password, string FullName, string Role);

    private static readonly DemoUserSeed[] DemoUsers =
    {
        new("applicant@talanton.demo", "Demo123!", "Demo Applicant", "Applicant"),
        new("underwriter@talanton.demo", "Demo123!", "Demo Underwriter", "Underwriter"),
        new("committee@talanton.demo", "Demo123!", "Demo Committee", "Committee"),
    };

    private static readonly (string Name, string Description)[] Roles =
    {
        ("Applicant", "Applicant portal role"),
        ("Underwriter", "Underwriter portal role"),
        ("Committee", "Committee portal role"),
    };

    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        var roleMap = await EnsureRolesAsync(db, cancellationToken);

        foreach (var seed in DemoUsers)
        {
            var user = await db.Users.FirstOrDefaultAsync(
                u => u.Email.ToLower() == seed.Email.ToLower(),
                cancellationToken);

            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = seed.Email,
                    PasswordHash = seed.Password,
                    FullName = seed.FullName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                db.Users.Add(user);
            }
            else
            {
                user.PasswordHash = seed.Password;
                user.FullName = seed.FullName;
                user.IsActive = true;
            }

            var roleId = roleMap[seed.Role.ToLowerInvariant()];

            var existingAssignments = await db.UserRoleAssignments
                .Where(a => a.UserId == user.Id && a.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var assignment in existingAssignments.Where(a => a.RoleId != roleId))
            {
                assignment.IsActive = false;
                assignment.RevokedAt = DateTime.UtcNow;
            }

            if (!existingAssignments.Any(a => a.RoleId == roleId && a.IsActive))
            {
                db.UserRoleAssignments.Add(new UserRoleAssignment
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = roleId,
                    IsActive = true,
                    AssignedAt = DateTime.UtcNow,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        // Ensure default SACCO
        var sacco = await db.Saccos.FirstOrDefaultAsync(cancellationToken);
        if (sacco is null)
        {
            sacco = new Sacco
            {
                Id = Guid.NewGuid(),
                Name = "Talanton SACCO",
                RegistrationNumber = "SACCO-UG-001",
                Status = "Active",
                ContactEmail = "info@talanton.demo",
                CreatedAt = DateTime.UtcNow
            };
            db.Saccos.Add(sacco);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Ensure default Demo Applicant
        var demoUser = await db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == "applicant@talanton.demo", cancellationToken);
        var applicant = await db.Applicants.FirstOrDefaultAsync(a => a.DisplayName == "Amara Trading Ltd" || a.DisplayName == "Demo Applicant", cancellationToken);
        if (applicant is null)
        {
            applicant = new Applicant
            {
                Id = Guid.NewGuid(),
                ApplicantType = "cooperative",
                DisplayName = "Amara Trading Ltd",
                IsActive = true,
                SaccoId = sacco.Id,
                ApplicantUserId = demoUser?.Id,
                CreatedAt = DateTime.UtcNow
            };
            db.Applicants.Add(applicant);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Seed Ledger Accounts
        if (!await db.LedgerAccounts.AnyAsync(cancellationToken))
        {
            db.LedgerAccounts.AddRange(
                new LedgerAccount { Id = Guid.NewGuid(), Name = "SACCO Main Cash Vault", AccountType = "Cash_Vault", CurrentBalance = 30000000m },
                new LedgerAccount { Id = Guid.NewGuid(), Name = "Commercial Bank Account", AccountType = "Bank_Current", CurrentBalance = 85000000m },
                new LedgerAccount { Id = Guid.NewGuid(), Name = "Mobile Money Float Account", AccountType = "Mobile_Money_Float", CurrentBalance = 35000000m }
            );
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task<Dictionary<string, Guid>> EnsureRolesAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        var roleMap = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

        foreach (var (name, description) in Roles)
        {
            var existing = await db.Roles.FirstOrDefaultAsync(
                r => r.Name.ToLower() == name.ToLower(),
                cancellationToken);

            if (existing is null)
            {
                existing = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    IsSystemRole = true,
                };
                db.Roles.Add(existing);
            }

            roleMap[name.ToLowerInvariant()] = existing.Id;
        }

        await db.SaveChangesAsync(cancellationToken);
        return roleMap;
    }
}
