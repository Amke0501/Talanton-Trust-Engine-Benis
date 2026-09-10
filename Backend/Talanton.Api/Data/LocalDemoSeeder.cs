using Microsoft.EntityFrameworkCore;
using Talanton.Api.Models;

namespace Talanton.Api.Data;

/// <summary>
/// Fills a throwaway local database with just enough to exercise the whole pipeline: a SACCO,
/// ledger balances for the liquidity gate to read, and a few applications at different stages.
///
/// Only ever called when USE_INMEMORY_DB is set, so it cannot seed a real deployment.
/// </summary>
public static class LocalDemoSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.LoanApplications.AnyAsync(cancellationToken))
        {
            return;
        }

        await DemoUsersSeeder.SeedAsync(db, cancellationToken);

        var sacco = new Sacco
        {
            Id = Guid.NewGuid(),
            Name = "Talanton SACCO",
            RegistrationNumber = "SACCO-UG-001",
            Status = "Active",
            ContactEmail = "info@talanton.demo",
            CreatedAt = DateTime.UtcNow,
        };
        db.Saccos.Add(sacco);

        // Ledger balances for the liquidity gate come from DemoUsersSeeder above; adding more
        // here would double the SACCO's apparent cash.

        var creator = await db.Users.FirstAsync(u => u.Email == "applicant@talanton.demo", cancellationToken);

        var grace = NewApplicant(sacco.Id, creator.Id, "Nakamya Grace", "individual");
        var ssemakula = NewApplicant(sacco.Id, creator.Id, "Ssemakula Enterprises Ltd", "cooperative");
        db.Applicants.AddRange(grace, ssemakula);

        db.SaccoMemberships.AddRange(
            NewMembership(sacco.Id, grace.Id, "M-8842", 4_000_000m),
            NewMembership(sacco.Id, ssemakula.Id, "SME-0412", 15_000_000m));

        var now = DateTime.UtcNow;

        db.LoanApplications.AddRange(
            new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-2026-1001",
                ApplicantId = grace.Id,
                SaccoId = sacco.Id,
                CreatedByUserId = creator.Id,
                MemberId = "M-8842",
                CurrentStatus = "SUBMITTED",
                CurrentStage = "underwriting",
                PrincipalAmount = 12_000_000m,
                AnnualSimpleInterestRatePct = 12m,
                TermMonths = 12,
                AdministrativeFeeAmount = 50_000m,
                Purpose = "Working capital & store upgrade",
                SavingsBalance = 4_000_000m,
                MonthlyIncome = 2_500_000m,
                MonthlyDebt = 500_000m,
                Multiplier = 3.0m,
                Verdict = "PENDING",
                StatusNote = "Application LA-2026-1001 submitted. Verification in progress.",
                SubmittedAt = now.AddDays(-6),
                CreatedAt = now.AddDays(-6),
            },
            new LoanApplication
            {
                Id = Guid.NewGuid(),
                ApplicationNumber = "LA-2026-1002",
                ApplicantId = ssemakula.Id,
                SaccoId = sacco.Id,
                CreatedByUserId = creator.Id,
                MemberId = "SME-0412",
                CurrentStatus = "in_review",
                CurrentStage = "committee",
                PrincipalAmount = 42_000_000m,
                AnnualSimpleInterestRatePct = 12m,
                TermMonths = 24,
                AdministrativeFeeAmount = 50_000m,
                Purpose = "Agricultural machinery purchase",
                SavingsBalance = 15_000_000m,
                MonthlyIncome = 8_500_000m,
                MonthlyDebt = 1_200_000m,
                Multiplier = 3.0m,
                Verdict = "APPROVED",
                StatusNote = "File LA-2026-1002 routed to Committee Board for authorization.",
                GuardrailDepositMultiplierPassed = true,
                GuardrailOneThirdPayPassed = true,
                GuardrailGuarantorPassed = true,
                SubmittedAt = now.AddDays(-9),
                CreatedAt = now.AddDays(-9),
            });

        await db.SaveChangesAsync(cancellationToken);
        Console.WriteLine("[STARTUP] Local demo data seeded (2 applications, 2 members)");
    }

    private static Applicant NewApplicant(Guid saccoId, Guid userId, string name, string type) => new()
    {
        Id = Guid.NewGuid(),
        SaccoId = saccoId,
        ApplicantUserId = userId,
        DisplayName = name,
        ApplicantType = type,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
    };

    private static SaccoMembership NewMembership(Guid saccoId, Guid applicantId, string number, decimal savings) => new()
    {
        Id = Guid.NewGuid(),
        SaccoId = saccoId,
        ApplicantId = applicantId,
        MembershipNumber = number,
        Status = "Active",
        SavingsBalance = savings,
        JoinedAt = DateTime.UtcNow.AddYears(-2),
    };
}
