using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.DTOs;
using Talanton.Api.Models;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Services;

public class LoanApplicationService : ILoanApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;
    private readonly NotificationService _notify;

    public LoanApplicationService(ApplicationDbContext context, AuditService audit, NotificationService notify)
    {
        _context = context;
        _audit = audit;
        _notify = notify;
    }
    private static readonly List<LoanApplicationDto> Applications = new()
    {
        new LoanApplicationDto
        {
            Id = Guid.Parse("941a0000-0000-0000-0000-000000000001"),
            Reference = "LA-2026-0941A",
            ApplicantName = "Nakamya Grace",
            MemberId = "M-8842",
            ApplicantType = "individual",
            Status = "in_review",
            Stage = "verification",
            Principal = 15000000m,
            Purpose = "Working capital & store upgrade",
            TenureMonths = 12,
            SavingsBalance = 4000000m,
            MonthlyIncome = 2500000m,
            MonthlyDebt = 500000m,
            Multiplier = 3.0m,
            SubmittedOn = "Aug 04, 2026",
            StatusNote = "File LA-2026-0941A is declined. Individual multiplier breach; Payslip take-home deficit.",
            DtiNetRatio = 82.0m,
            NetTakeHome = 450000m,
            GuardrailDepositMultiplierPassed = false,
            GuardrailOneThirdPayPassed = false,
            GuardrailGuarantorPassed = true,
            Verdict = "DECLINED",
            Guarantors = new List<GuarantorDto>
            {
                new GuarantorDto { Id = "g1", Name = "Kato Joseph", MemberId = "M-1104", PledgedShares = 8000000m, AvailableShares = 8000000m },
                new GuarantorDto { Id = "g2", Name = "Namatovu Sarah", MemberId = "M-2309", PledgedShares = 5000000m, AvailableShares = 9500000m }
            },
            CommitteeVotes = new List<CommitteeVoteDetailDto>
            {
                new CommitteeVoteDetailDto { MemberName = "Chairman", MemberRole = "Chairperson", Vote = "APPROVE" },
                new CommitteeVoteDetailDto { MemberName = "Sec. General", MemberRole = "Risk Head", Vote = "APPROVE" },
                new CommitteeVoteDetailDto { MemberName = "Mrs. Nabukenya", MemberRole = "Credit Officer", Vote = "APPROVE" },
                new CommitteeVoteDetailDto { MemberName = "Dr. Ochieng", MemberRole = "Treasurer", Vote = "ABSTAIN" },
                new CommitteeVoteDetailDto { MemberName = "Eng. Museveni", MemberRole = "Board Member", Vote = "ABSTAIN" }
            }
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("938b0000-0000-0000-0000-000000000002"),
            Reference = "LA-2026-0938B",
            ApplicantName = "Ssemakula Enterprises Ltd",
            MemberId = "SME-0412",
            ApplicantType = "cooperative",
            Status = "in_review",
            Stage = "underwriting",
            Principal = 42000000m,
            Purpose = "Agricultural machinery purchase",
            TenureMonths = 24,
            SavingsBalance = 15000000m,
            MonthlyIncome = 8500000m,
            MonthlyDebt = 1200000m,
            Multiplier = 3.0m,
            SubmittedOn = "Aug 02, 2026",
            StatusNote = "Underwriting review in progress.",
            DtiNetRatio = 38.5m,
            NetTakeHome = 4200000m,
            GuardrailDepositMultiplierPassed = true,
            GuardrailOneThirdPayPassed = true,
            GuardrailGuarantorPassed = true,
            Verdict = "APPROVED"
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("912c0000-0000-0000-0000-000000000003"),
            Reference = "LA-2026-0912C",
            ApplicantName = "Kato Joseph",
            MemberId = "M-1104",
            ApplicantType = "individual",
            Status = "disbursed",
            Stage = "disbursed",
            Principal = 8000000m,
            Purpose = "Poultry farm expansion",
            TenureMonths = 10,
            SavingsBalance = 3500000m,
            MonthlyIncome = 2100000m,
            MonthlyDebt = 300000m,
            Multiplier = 3.0m,
            SubmittedOn = "Jun 15, 2026",
            StatusNote = "Disbursed. Active repayment status.",
            RepaymentProgress = "4/10 paid",
            DueDate = "Feb 05, 2026",
            Arrears = 0m
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("899d0000-0000-0000-0000-000000000004"),
            Reference = "LA-2026-0899D",
            ApplicantName = "Auma Florence",
            MemberId = "M-4511",
            ApplicantType = "individual",
            Status = "disbursed",
            Stage = "disbursed",
            Principal = 6500000m,
            Purpose = "Tailoring shop upgrade",
            TenureMonths = 8,
            SavingsBalance = 3100000m,
            MonthlyIncome = 1900000m,
            MonthlyDebt = 210000m,
            Multiplier = 3.0m,
            SubmittedOn = "Jul 28, 2026",
            StatusNote = "Active loan with minor arrears.",
            RepaymentProgress = "6/8 paid",
            DueDate = "Feb 12, 2026",
            Arrears = 320000m
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("871e0000-0000-0000-0000-000000000005"),
            Reference = "LA-2026-0871E",
            ApplicantName = "Mukasa Agro Supplies",
            MemberId = "SME-9022",
            ApplicantType = "cooperative",
            Status = "approved",
            Stage = "committee",
            Principal = 28000000m,
            Purpose = "Fertilizer inventory restocking",
            TenureMonths = 18,
            SavingsBalance = 10000000m,
            MonthlyIncome = 6200000m,
            MonthlyDebt = 900000m,
            Multiplier = 3.0m,
            SubmittedOn = "Jul 20, 2026",
            StatusNote = "Approved by Board. Pending disbursement release.",
            Verdict = "APPROVED"
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("842f0000-0000-0000-0000-000000000006"),
            Reference = "LA-2025-0842F",
            ApplicantName = "Namatovu Sarah",
            MemberId = "M-2309",
            ApplicantType = "individual",
            Status = "disbursed",
            Stage = "disbursed",
            Principal = 4000000m,
            Purpose = "School fees payment",
            TenureMonths = 6,
            SavingsBalance = 2500000m,
            MonthlyIncome = 1800000m,
            MonthlyDebt = 150000m,
            Multiplier = 3.0m,
            SubmittedOn = "Dec 10, 2025",
            StatusNote = "Loan completed and fully paid.",
            RepaymentProgress = "6/6 paid",
            DueDate = "Jun 10, 2026",
            Arrears = 0m
        },
        new LoanApplicationDto
        {
            Id = Guid.Parse("80300000-0000-0000-0000-000000000007"),
            Reference = "LA-2025-0803G",
            ApplicantName = "Okello Trading Co.",
            MemberId = "SME-1189",
            ApplicantType = "cooperative",
            Status = "declined",
            // Declined files terminate at the underwriting desk. This row previously shipped with
            // Stage = "committee", putting a declined file in the committee queue out of the box.
            Stage = "underwriting",
            Principal = 55000000m,
            Purpose = "Fleet vehicle acquisition",
            TenureMonths = 36,
            SavingsBalance = 12000000m,
            MonthlyIncome = 7000000m,
            MonthlyDebt = 3500000m,
            Multiplier = 3.0m,
            SubmittedOn = "Nov 05, 2025",
            StatusNote = "Declined due to excessive debt service coverage ratio.",
            Verdict = "DECLINED"
        }
    };

    private static readonly List<CreditPassportMemberDto> PassportMembers = new()
    {
        new CreditPassportMemberDto
        {
            Id = "cp1",
            Name = "Namatovu Sarah",
            MemberId = "M-2309",
            Classification = "Individual",
            Tier = "PLATINUM",
            TrustScore = 92,
            OnTimeRatePct = 100,
            LoansCompleted = 4,
            TotalRepaid = 18500000m,
            CurrentLimit = 25000000m,
            LastLoanDate = "Dec 2025"
        },
        new CreditPassportMemberDto
        {
            Id = "cp2",
            Name = "Ssemakula Agro Ltd",
            MemberId = "SME-0412",
            Classification = "SME",
            Tier = "PLATINUM",
            TrustScore = 88,
            OnTimeRatePct = 97,
            LoansCompleted = 3,
            TotalRepaid = 76000000m,
            CurrentLimit = 90000000m,
            LastLoanDate = "Nov 2025"
        },
        new CreditPassportMemberDto
        {
            Id = "cp3",
            Name = "Kato Joseph",
            MemberId = "M-1104",
            Classification = "Individual",
            Tier = "GOLD",
            TrustScore = 84,
            OnTimeRatePct = 95,
            LoansCompleted = 2,
            TotalRepaid = 9200000m,
            CurrentLimit = 15000000m,
            LastLoanDate = "Oct 2025"
        },
        new CreditPassportMemberDto
        {
            Id = "cp4",
            Name = "Auma Florence",
            MemberId = "M-4511",
            Classification = "Individual",
            Tier = "GOLD",
            TrustScore = 76,
            OnTimeRatePct = 89,
            LoansCompleted = 3,
            TotalRepaid = 11400000m,
            CurrentLimit = 12000000m,
            LastLoanDate = "Jan 2026"
        },
        new CreditPassportMemberDto
        {
            Id = "cp5",
            Name = "Mukasa Peter",
            MemberId = "M-9022",
            Classification = "Individual",
            Tier = "SILVER",
            TrustScore = 71,
            OnTimeRatePct = 92,
            LoansCompleted = 1,
            TotalRepaid = 3500000m,
            CurrentLimit = 6000000m,
            LastLoanDate = "Sep 2025"
        },
        new CreditPassportMemberDto
        {
            Id = "cp6",
            Name = "Kiiza Wholesale Co.",
            MemberId = "SME-0755",
            Classification = "SME",
            Tier = "GOLD",
            TrustScore = 83,
            OnTimeRatePct = 94,
            LoansCompleted = 2,
            TotalRepaid = 44000000m,
            CurrentLimit = 60000000m,
            LastLoanDate = "Dec 2025"
        }
    };

    public async Task<IEnumerable<LoanApplicationDto>> GetAllLoanApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var resultList = new List<LoanApplicationDto>();

        try
        {
            var dbApps = await _context.LoanApplications
                .Include(la => la.Applicant)
                .Include(la => la.Sacco)
                .OrderByDescending(la => la.CreatedAt)
                .ToListAsync(cancellationToken);

            foreach (var app in dbApps)
            {
                var existingInMemory = Applications.FirstOrDefault(a => a.Id == app.Id || a.Reference.Equals(app.ApplicationNumber, StringComparison.OrdinalIgnoreCase));
                if (existingInMemory != null)
                {
                    var overlaid = OverlayPersistedWorkflow(existingInMemory, app);
                    await HydrateFromDatabaseAsync(overlaid, app.Id, cancellationToken);
                    resultList.Add(overlaid);
                }
                else
                {
                    var dto = ToDto(app);
                    await HydrateFromDatabaseAsync(dto, app.Id, cancellationToken);
                    resultList.Add(dto);
                }
            }
        }
        catch (Exception ex)
        {
            // Falling through leaves only the hard-coded demo files in the response, which is
            // indistinguishable from a healthy empty database unless the consequence is stated.
            Console.WriteLine($"[ERROR] Could not read loan applications from the database ({ex.GetType().Name}): {ex.Message}. " +
                              "Returning in-memory demo data only — persisted applications will be missing from every list and detail view.");
        }

        foreach (var memApp in Applications)
        {
            if (!resultList.Any(r => r.Id == memApp.Id || r.Reference.Equals(memApp.Reference, StringComparison.OrdinalIgnoreCase)))
            {
                resultList.Add(memApp);
            }
        }

        return resultList;
    }

    public async Task<LoanApplicationDto?> GetLoanApplicationByRefAsync(string reference, CancellationToken cancellationToken = default)
    {
        var apps = await GetAllLoanApplicationsAsync(cancellationToken);
        return apps.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<LoanApplicationDto> CreateLoanApplicationAsync(CreateLoanApplicationDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (dto.Principal <= 0)
        {
            throw new ArgumentException("Loan principal must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(dto.Purpose))
        {
            throw new ArgumentException("Loan purpose is required.");
        }

        if (dto.TenureMonths <= 0)
        {
            throw new ArgumentException("Tenure months must be greater than 0.");
        }

        var refNo = $"LA-2026-{Random.Shared.Next(1000, 9999)}X";
        var newId = Guid.NewGuid();

        try
        {
            var sacco = await _context.Saccos.FirstOrDefaultAsync(cancellationToken);
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
                _context.Saccos.Add(sacco);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var user = await _context.Users.FirstOrDefaultAsync(cancellationToken);
            if (user is null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = "applicant@talanton.demo",
                    PasswordHash = "Demo123!",
                    FullName = "Demo Applicant",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    SaccoId = sacco.Id
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var applicantName = string.IsNullOrWhiteSpace(dto.ApplicantName) ? "Amara Trading Ltd" : dto.ApplicantName.Trim();
            var applicant = await _context.Applicants.FirstOrDefaultAsync(a => a.DisplayName.ToLower() == applicantName.ToLower(), cancellationToken);
            if (applicant is null)
            {
                applicant = new Applicant
                {
                    Id = Guid.NewGuid(),
                    ApplicantType = string.IsNullOrWhiteSpace(dto.ApplicantType) ? "cooperative" : dto.ApplicantType,
                    DisplayName = applicantName,
                    IsActive = true,
                    SaccoId = sacco.Id,
                    ApplicantUserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Applicants.Add(applicant);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var entity = new LoanApplication
            {
                Id = newId,
                ApplicationNumber = refNo,
                ApplicantId = applicant.Id,
                SaccoId = sacco.Id,
                CreatedByUserId = user.Id,
                CurrentStatus = "SUBMITTED",
                CurrentStage = "verification",
                PrincipalAmount = dto.Principal,
                AnnualSimpleInterestRatePct = 12.0m,
                TermMonths = dto.TenureMonths,
                AdministrativeFeeAmount = 50000m,
                Purpose = dto.Purpose,
                SubmittedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,

                // The figures the applicant declared, kept on the row rather than substituted
                // with stand-ins on every read. Underwriting starts from what was actually
                // applied for; a file that reported someone else's savings and income was not a
                // file anyone could underwrite.
                MemberId = string.IsNullOrWhiteSpace(dto.MemberId) ? string.Empty : dto.MemberId.Trim(),
                SavingsBalance = dto.SavingsBalance,
                MonthlyIncome = dto.MonthlyIncome,
                MonthlyDebt = dto.MonthlyDebt,
                Multiplier = dto.Multiplier > 0 ? dto.Multiplier : 3.0m,
                DtiNetRatio = dto.MonthlyIncome > 0 ? Math.Round((dto.MonthlyDebt / dto.MonthlyIncome) * 100, 1) : 0,
                NetTakeHome = dto.MonthlyIncome - dto.MonthlyDebt,
                Verdict = "PENDING",
                StatusNote = $"Application {refNo} submitted. Verification in progress.",
            };

            _context.LoanApplications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Underwriter,
                NotificationEvents.ApplicationSubmitted,
                $"New application {refNo}",
                $"{applicantName} submitted {dto.Principal:N0} UGX over {dto.TenureMonths} months for {dto.Purpose}.",
                refNo,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            // The caller still receives a created DTO, so a failure here silently produces an
            // application that exists for this process only and disappears on restart.
            Console.WriteLine($"[ERROR] Could not save the new loan application to the database ({ex.GetType().Name}): {ex.Message}. " +
                              "The application was returned to the caller but has NOT been persisted.");
        }

        var createdDto = new LoanApplicationDto
        {
            Id = newId,
            Reference = refNo,
            ApplicantName = string.IsNullOrWhiteSpace(dto.ApplicantName) ? "Amara Trading Ltd" : dto.ApplicantName,
            MemberId = string.IsNullOrWhiteSpace(dto.MemberId) ? "APP-TEST-001" : dto.MemberId,
            ApplicantType = string.IsNullOrWhiteSpace(dto.ApplicantType) ? "cooperative" : dto.ApplicantType,
            Status = "submitted",
            Stage = "verification",
            Principal = dto.Principal,
            Purpose = dto.Purpose,
            TenureMonths = dto.TenureMonths,
            SavingsBalance = dto.SavingsBalance,
            MonthlyIncome = dto.MonthlyIncome,
            MonthlyDebt = dto.MonthlyDebt,
            Multiplier = dto.Multiplier > 0 ? dto.Multiplier : 3.0m,
            SubmittedOn = DateTime.UtcNow.ToString("MMM dd, yyyy"),
            StatusNote = $"Application {refNo} submitted. Verification in progress.",
            DtiNetRatio = dto.MonthlyIncome > 0 ? Math.Round((dto.MonthlyDebt / dto.MonthlyIncome) * 100, 1) : 0,
            NetTakeHome = dto.MonthlyIncome - dto.MonthlyDebt,
            Verdict = "IN_REVIEW"
        };

        Applications.Insert(0, createdDto);
        return createdDto;
    }

    public async Task<LoanApplicationDto?> UpdateUnderwritingAsync(string reference, UpdateUnderwritingOverrideDto dto, CancellationToken cancellationToken = default)
    {
        // Resolve through the merged view, not the hard-coded demo list. Applications created
        // through the API exist only in the database, so looking them up here returned null and
        // the caller got a 404 — which is why an underwriter's revised terms were never recorded
        // and the applicant was never offered the choice.
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        var originalPrincipal = app.Principal;
        var originalTenure = app.TenureMonths;
        var hasCounterOfferAdjustment = dto.RequestedPrincipal < app.Principal || dto.TenureMonths != app.TenureMonths;

        app.ApplicantType = dto.ApplicantType;
        app.Multiplier = dto.Multiplier;
        app.TenureMonths = dto.TenureMonths;
        app.Principal = dto.RequestedPrincipal;
        app.SavingsBalance = dto.SavingsBalance;
        app.MonthlyIncome = dto.BasicMonthlyPay;
        app.MonthlyDebt = dto.MonthlyDeductions;

        // Recalculate guardrail metrics
        var maxCap = app.SavingsBalance * app.Multiplier;
        var estMonthlyPayment = app.TenureMonths > 0 ? (app.Principal / app.TenureMonths) : 0;
        var residualPay = app.MonthlyIncome - app.MonthlyDebt - estMonthlyPayment;

        app.GuardrailDepositMultiplierPassed = app.Principal <= maxCap;
        app.GuardrailOneThirdPayPassed = residualPay >= (app.MonthlyIncome / 3.0m);
        app.DtiNetRatio = app.MonthlyIncome > 0 ? Math.Round(((app.MonthlyDebt + estMonthlyPayment) / app.MonthlyIncome) * 100, 1) : 0;
        app.NetTakeHome = residualPay;

        var totalPledged = app.Guarantors.Sum(g => g.PledgedShares);
        var uncollateralized = Math.Max(0, app.Principal - app.SavingsBalance);
        app.GuardrailGuarantorPassed = totalPledged >= uncollateralized;

        if (hasCounterOfferAdjustment)
        {
            app.CounterOfferPrincipal = dto.RequestedPrincipal;
            app.CounterOfferTenureMonths = dto.TenureMonths;
            app.CounterOfferReason = string.IsNullOrWhiteSpace(dto.AdjustmentReason)
                ? "Underwriter reduced the requested amount or adjusted the tenure after review."
                : dto.AdjustmentReason;
            app.CounterOfferStatus = "PENDING";
            app.ApplicantConsentReceived = false;
            app.ApplicantConsentAt = null;
            app.Verdict = "PENDING";
            app.Stage = "underwriting";
            app.Status = "counter_offer_pending";
            app.StatusNote = $"Revised offer sent to applicant: principal reduced from {originalPrincipal:N0} UGX to {dto.RequestedPrincipal:N0} UGX and tenure changed from {originalTenure} to {dto.TenureMonths} months. Applicant consent is required before the file can move to committee.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.CounterOfferMade, "LoanApplication", app.Reference,
                before: $"principal {originalPrincipal:N0} over {originalTenure} months",
                after: $"principal {dto.RequestedPrincipal:N0} over {dto.TenureMonths} months",
                cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Applicant,
                NotificationEvents.CounterOfferSent,
                $"Revised offer on {app.Reference}",
                $"The underwriting desk has revised your request to {dto.RequestedPrincipal:N0} UGX over " +
                $"{dto.TenureMonths} months. {app.CounterOfferReason} Your acceptance is required before it can " +
                "go to the committee.",
                app.Reference, app.MemberId, cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Underwriter,
                NotificationEvents.CounterOfferSent,
                $"{app.Reference} sent to the applicant for consent",
                $"{app.ApplicantName} has been asked to accept {dto.RequestedPrincipal:N0} UGX over " +
                $"{dto.TenureMonths} months. The file stays at the underwriting desk until they answer.",
                app.Reference, cancellationToken: cancellationToken);

            return app;
        }

        // All three guardrails count. The verdict used to ignore guarantor cover while the
        // underwriter's own screen included it, so a file could be shown as a guardrail breach and
        // still be recorded APPROVED — the two answers disagreed on the same file.
        app.Verdict = (app.GuardrailDepositMultiplierPassed
                       && app.GuardrailOneThirdPayPassed
                       && app.GuardrailGuarantorPassed) ? "APPROVED" : "DECLINED";

        app.StatusNote = app.Verdict == "APPROVED"
            ? $"File {reference} meets all underwriting guardrail checks."
            : $"File {reference} is declined. {DescribeGuardrailBreaches(app)}";

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.UnderwritingDecided, "LoanApplication", app.Reference,
            after: $"Verdict {app.Verdict}; principal {app.Principal:N0} over {app.TenureMonths} months",
            cancellationToken: cancellationToken);

        if (app.Verdict == "DECLINED")
        {
            await _notify.RaiseAsync(
                NotificationAudiences.Applicant,
                NotificationEvents.UnderwritingDeclined,
                $"{app.Reference} declined at underwriting",
                app.StatusNote,
                app.Reference, app.MemberId, cancellationToken);
        }

        return app;
    }

    /// <summary>
    /// Names the checks that actually failed, rather than listing every possible reason. An
    /// applicant told "multiplier breach or take-home deficit" cannot tell which applies to them.
    /// </summary>
    private static string DescribeGuardrailBreaches(LoanApplicationDto app)
    {
        var breaches = new List<string>();

        if (!app.GuardrailDepositMultiplierPassed)
        {
            breaches.Add($"the request exceeds {app.Multiplier:0.##}x the savings balance of {app.SavingsBalance:N0} UGX");
        }

        if (!app.GuardrailOneThirdPayPassed)
        {
            breaches.Add($"repayments would leave take-home pay of {app.NetTakeHome:N0} UGX, below the statutory one third");
        }

        if (!app.GuardrailGuarantorPassed)
        {
            var pledged = app.Guarantors.Sum(g => g.PledgedShares);
            var gap = Math.Max(0, app.Principal - app.SavingsBalance);
            breaches.Add($"guarantor cover of {pledged:N0} UGX does not reach the uncollateralised gap of {gap:N0} UGX");
        }

        return breaches.Count == 0
            ? "No guardrail breach was recorded."
            : char.ToUpperInvariant(breaches[0][0]) + breaches[0][1..] +
              (breaches.Count > 1 ? "; " + string.Join("; ", breaches.Skip(1)) : string.Empty) + ".";
    }

    public async Task<LoanApplicationDto?> RespondToCounterOfferAsync(string reference, CounterOfferDecisionDto dto, CancellationToken cancellationToken = default)
    {
        // Resolve through the merged view, not the hard-coded demo list. Applications created
        // through the API exist only in the database, so looking them up here returned null and
        // the caller got a 404 — which is why an underwriter's revised terms were never recorded
        // and the applicant was never offered the choice.
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        if (string.IsNullOrWhiteSpace(dto.Decision))
        {
            throw new ArgumentException("Counter-offer decision is required.");
        }

        var decision = dto.Decision.Trim();
        if (!decision.Equals("ACCEPT", StringComparison.OrdinalIgnoreCase) && !decision.Equals("DECLINE", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Counter-offer decision must be ACCEPT or DECLINE.");
        }

        if (app.CounterOfferStatus != "PENDING")
        {
            app.StatusNote = "There is no active counter-offer pending applicant consent.";
            return app;
        }

        if (decision.Equals("ACCEPT", StringComparison.OrdinalIgnoreCase))
        {
            app.CounterOfferStatus = "ACCEPTED";
            app.ApplicantConsentReceived = true;
            app.ApplicantConsentAt = DateTime.UtcNow;
            app.Principal = app.CounterOfferPrincipal ?? app.Principal;
            app.TenureMonths = app.CounterOfferTenureMonths ?? app.TenureMonths;
            app.Status = "in_review";
            app.Stage = "underwriting";
            app.Verdict = "APPROVED";
            app.MinimumAdditionalGuarantorsRequired = 0;
            app.StatusNote =
                $"Applicant accepted the revised offer on {app.ApplicantConsentAt:dd MMM yyyy 'at' HH:mm} UTC. " +
                $"Consent is recorded against {app.Principal:N0} UGX over {app.TenureMonths} months and the file " +
                "can proceed to committee review.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.CounterOfferAnswered, "LoanApplication", app.Reference,
                after: $"Applicant ACCEPTED: principal {app.Principal:N0} over {app.TenureMonths} months",
                cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Underwriter,
                NotificationEvents.CounterOfferAccepted,
                $"{app.Reference}: revised offer accepted",
                $"{app.ApplicantName} accepted {app.Principal:N0} UGX over {app.TenureMonths} months. " +
                "The file is cleared to route to the committee.",
                app.Reference, cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Applicant,
                NotificationEvents.CounterOfferAccepted,
                $"Your acceptance of {app.Reference} is recorded",
                app.StatusNote,
                app.Reference, app.MemberId, cancellationToken);

            return app;
        }

        app.CounterOfferStatus = "DECLINED";
        app.ApplicantConsentReceived = false;
        app.ApplicantConsentAt = null;
        // Declining ends the revised offer, but not necessarily the application: the applicant may
        // strengthen the file with additional guarantors and have it underwritten again. The file
        // still cannot reach committee on this verdict — resubmission returns it to underwriting
        // for a fresh decision, not past it.
        app.Status = "awaiting_guarantors";
        app.Stage = "underwriting";
        app.Verdict = "DECLINED";
        app.MinimumAdditionalGuarantorsRequired = MinimumAdditionalGuarantors;
        app.StatusNote =
            $"Applicant declined the revised offer. {MinimumAdditionalGuarantors} additional guarantors are " +
            "required before this file can be reconsidered; without them it terminates at the underwriting desk.";
        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.CounterOfferAnswered, "LoanApplication", app.Reference,
            after: "Applicant DECLINED the revised offer",
            cancellationToken: cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Applicant,
            NotificationEvents.GuarantorsRequired,
            $"{MinimumAdditionalGuarantors} more guarantors needed on {app.Reference}",
            $"You declined the revised offer. Add {MinimumAdditionalGuarantors} guarantors who are not already " +
            "on the file and resubmit, and the underwriting desk will look at it again.",
            app.Reference, app.MemberId, cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Underwriter,
            NotificationEvents.CounterOfferDeclined,
            $"{app.Reference}: revised offer declined",
            $"{app.ApplicantName} declined the revised terms. The file stays at the underwriting desk, marked as " +
            $"needing {MinimumAdditionalGuarantors} additional guarantors.",
            app.Reference, cancellationToken: cancellationToken);

        return app;
    }

    /// <summary>
    /// How many *new* guarantors an applicant must add to have a declined offer reconsidered.
    /// </summary>
    public const int MinimumAdditionalGuarantors = 2;

    /// <summary>
    /// Reopens an application the applicant declined, on the strength of additional guarantors.
    ///
    /// Only guarantors not already on the file count toward the requirement — re-listing existing
    /// ones adds no security, which is the whole point of asking for them.
    /// </summary>
    public async Task<LoanApplicationDto?> ResubmitWithGuarantorsAsync(
        string reference, ResubmitWithGuarantorsDto dto, CancellationToken cancellationToken = default)
    {
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        if (!string.Equals(app.CounterOfferStatus, "DECLINED", StringComparison.OrdinalIgnoreCase))
        {
            app.StatusNote = "This application has no declined offer to reconsider.";
            return app;
        }

        var existingMemberIds = app.Guarantors
            .Select(g => g.MemberId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var additions = (dto.Guarantors ?? new List<GuarantorDto>())
            .Where(g => !string.IsNullOrWhiteSpace(g.MemberId) && !existingMemberIds.Contains(g.MemberId))
            .GroupBy(g => g.MemberId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToList();

        if (additions.Count < MinimumAdditionalGuarantors)
        {
            app.StatusNote =
                $"{MinimumAdditionalGuarantors} additional guarantors are required to reconsider this file. " +
                $"{additions.Count} new guarantor(s) supplied; guarantors already on the file do not count.";
            app.MinimumAdditionalGuarantorsRequired = MinimumAdditionalGuarantors - additions.Count;
            return app;
        }

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        foreach (var guarantor in additions)
        {
            guarantor.Id = Guid.NewGuid().ToString("N");
            app.Guarantors.Add(guarantor);
            await PersistGuarantorAsync(entity, guarantor, cancellationToken);
        }

        var totalPledged = app.Guarantors.Sum(g => g.PledgedShares);
        var uncollateralized = Math.Max(0, app.Principal - app.SavingsBalance);
        app.GuardrailGuarantorPassed = totalPledged >= uncollateralized;

        // Back to underwriting for a fresh decision. The verdict is cleared so the previous
        // decline cannot carry through, and the counter-offer is closed out.
        app.CounterOfferStatus = "NONE";
        app.CounterOfferPrincipal = null;
        app.CounterOfferTenureMonths = null;
        app.CounterOfferReason = null;
        app.Verdict = "PENDING";
        app.Status = "in_review";
        app.Stage = "underwriting";
        app.MinimumAdditionalGuarantorsRequired = 0;
        app.StatusNote =
            $"Applicant added {additions.Count} additional guarantors after declining the revised offer. " +
            $"Total pledged is now {totalPledged:N0} against an uncollateralised gap of {uncollateralized:N0}. " +
            "Returned to underwriting for a fresh decision.";

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.ResubmittedWithGuarantors, "LoanApplication", app.Reference,
            after: $"Added {additions.Count} guarantors ({string.Join(", ", additions.Select(g => g.MemberId))}); " +
                   $"returned to underwriting",
            cancellationToken: cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Underwriter,
            NotificationEvents.ResubmittedWithGuarantors,
            $"{app.Reference} resubmitted with {additions.Count} more guarantors",
            $"{app.ApplicantName} added {string.Join(", ", additions.Select(g => $"{g.Name} ({g.MemberId})"))}. " +
            $"Cover is now {totalPledged:N0} UGX against a gap of {uncollateralized:N0} UGX. Awaiting a fresh decision.",
            app.Reference, cancellationToken: cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Applicant,
            NotificationEvents.ResubmittedWithGuarantors,
            $"{app.Reference} is back with the underwriting desk",
            app.StatusNote,
            app.Reference, app.MemberId, cancellationToken);

        return app;
    }

    public async Task<LoanApplicationDto?> AddGuarantorAsync(string reference, GuarantorDto guarantor, CancellationToken cancellationToken = default)
    {
        // Resolve through the merged view rather than the in-memory list alone: applications
        // created through the API exist only in the database, and used to be unreachable here.
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        var existing = app.Guarantors.FirstOrDefault(
            g => g.MemberId.Equals(guarantor.MemberId, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            // Re-pledging replaces the previous figure. Adding a second row for the same member
            // double-counted their shares toward coverage.
            existing.Name = guarantor.Name;
            existing.PledgedShares = guarantor.PledgedShares;
            existing.AvailableShares = guarantor.AvailableShares;
            guarantor = existing;
        }
        else
        {
            guarantor.Id = Guid.NewGuid().ToString("N");
            app.Guarantors.Add(guarantor);
        }

        var totalPledged = app.Guarantors.Sum(g => g.PledgedShares);
        var uncollateralized = Math.Max(0, app.Principal - app.SavingsBalance);
        app.GuardrailGuarantorPassed = totalPledged >= uncollateralized;

        await PersistGuarantorAsync(entity, guarantor, cancellationToken);
        return app;
    }

    /// <summary>
    /// Writes one guarantor pledge through to the database, keyed on application + member so a
    /// re-pledge updates in place. No-ops for the hard-coded demo files, which have no row.
    /// </summary>
    private async Task PersistGuarantorAsync(LoanApplication? entity, GuarantorDto guarantor, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        try
        {
            var row = await _context.ApplicationGuarantors.FirstOrDefaultAsync(
                g => g.LoanApplicationId == entity.Id && g.MemberId == guarantor.MemberId,
                cancellationToken);

            if (row is null)
            {
                row = new ApplicationGuarantor
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = entity.Id,
                    MemberId = guarantor.MemberId,
                };
                _context.ApplicationGuarantors.Add(row);
            }

            row.Name = guarantor.Name;
            row.PledgedShares = guarantor.PledgedShares;
            row.AvailableShares = guarantor.AvailableShares;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Could not save guarantor {guarantor.MemberId} for {entity.ApplicationNumber} " +
                              $"({ex.GetType().Name}): {ex.Message}. The pledge exists in memory only and will be lost on restart.");
        }
    }

    public async Task<LoanApplicationDto?> CastVoteAsync(string reference, CastCommitteeVoteDto voteDto, CancellationToken cancellationToken = default)
    {
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var existing = app.CommitteeVotes.FirstOrDefault(v => v.MemberRole.Equals(voteDto.MemberRole, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
        {
            existing.Vote = voteDto.Vote;
        }
        else
        {
            app.CommitteeVotes.Add(new CommitteeVoteDetailDto
            {
                MemberName = voteDto.MemberRole,
                MemberRole = voteDto.MemberRole,
                Vote = voteDto.Vote
            });
        }

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        await PersistVoteAsync(entity, voteDto, cancellationToken);
        await _audit.RecordAsync(AuditActions.VoteCast, "LoanApplication", app.Reference,
            actorName: voteDto.MemberRole,
            after: $"{voteDto.MemberRole} voted {voteDto.Vote}",
            cancellationToken: cancellationToken);

        var quorum = QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);
        await _notify.RaiseAsync(
            NotificationAudiences.Committee,
            NotificationEvents.VoteCast,
            $"{voteDto.MemberRole} voted {voteDto.Vote} on {app.Reference}",
            quorum.Reason,
            app.Reference, cancellationToken: cancellationToken);

        return app;
    }

    /// <summary>
    /// Writes a committee vote through to the database. Votes were previously held only in a
    /// static list, so a restart erased the board's decisions and left no record of who decided
    /// what — the audit trail the founder asked for has nothing to draw on without this.
    ///
    /// A seat maps to the seeded user of the same name; the vote hangs off the application's
    /// CommitteeReview, which is created on first vote.
    /// </summary>
    private async Task PersistVoteAsync(LoanApplication? entity, CastCommitteeVoteDto voteDto, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        try
        {
            var voter = await _context.Users.FirstOrDefaultAsync(
                u => u.FullName.ToLower() == voteDto.MemberRole.ToLower(), cancellationToken);

            if (voter is null)
            {
                Console.WriteLine($"[ERROR] No user account holds the seat '{voteDto.MemberRole}', so the vote on " +
                                  $"{entity.ApplicationNumber} was not recorded. Seed accounts may be missing.");
                return;
            }

            var review = await _context.CommitteeReviews
                .FirstOrDefaultAsync(r => r.LoanApplicationId == entity.Id, cancellationToken);

            if (review is null)
            {
                review = new CommitteeReview
                {
                    Id = Guid.NewGuid(),
                    LoanApplicationId = entity.Id,
                    InitiatedByUserId = voter.Id,
                    ReviewStatus = "Open",
                    QuorumRequired = QuorumEvaluationService.GetRequiredApprovals(entity.PrincipalAmount),
                };
                _context.CommitteeReviews.Add(review);
                await _context.SaveChangesAsync(cancellationToken);
            }

            var vote = await _context.CommitteeVotes.FirstOrDefaultAsync(
                v => v.CommitteeReviewId == review.Id && v.VoterUserId == voter.Id, cancellationToken);

            if (vote is null)
            {
                vote = new CommitteeVote
                {
                    Id = Guid.NewGuid(),
                    CommitteeReviewId = review.Id,
                    VoterUserId = voter.Id,
                };
                _context.CommitteeVotes.Add(vote);
            }

            // Re-voting replaces the member's previous position rather than adding a second vote.
            vote.Vote = voteDto.Vote;
            vote.VotedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Could not save the {voteDto.MemberRole} vote on {entity.ApplicationNumber} " +
                              $"({ex.GetType().Name}): {ex.Message}. It exists in memory only and will be lost on restart.");
        }
    }

    public async Task<LoanApplicationDto?> RouteStageAsync(string reference, string targetStage, CancellationToken cancellationToken = default)
    {
        // Resolve through the merged view, not the hard-coded demo list. Applications created
        // through the API exist only in the database, so looking them up here returned null and
        // the caller got a 404 — which is why an underwriter's revised terms were never recorded
        // and the applicant was never offered the choice.
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        if (targetStage.Equals("committee", StringComparison.OrdinalIgnoreCase))
        {
            // An applicant who declined a revised offer is waiting on guarantors, not on a
            // guardrail breach. Both states carry Verdict "DECLINED", so this is checked first —
            // otherwise the file was correctly blocked but told the wrong story about why.
            if (app.CounterOfferStatus.Equals("DECLINED", StringComparison.OrdinalIgnoreCase))
            {
                app.Status = "awaiting_guarantors";
                app.Stage = "underwriting";
                app.MinimumAdditionalGuarantorsRequired = MinimumAdditionalGuarantors;
                app.StatusNote =
                    $"File {reference} cannot reach committee: the applicant declined the revised offer and " +
                    $"{MinimumAdditionalGuarantors} additional guarantors are required before it is reconsidered.";
                await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
                return app;
            }

            // Block declined files from reaching committee — verdict is set by UpdateUnderwritingAsync().
            if (app.Verdict.Equals("DECLINED", StringComparison.OrdinalIgnoreCase))
            {
                app.Status = "declined";
                app.StatusNote = $"File {reference} failed underwriting guardrail checks and cannot proceed to committee. It terminates at the underwriting desk.";
                await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
                return app;
            }

            if (app.CounterOfferStatus == "PENDING")
            {
                app.StatusNote = $"File {reference} cannot proceed to committee until the applicant explicitly accepts or declines the revised offer.";
                return app;
            }

            app.Stage = targetStage;
            app.Status = "in_review";
            app.StatusNote = $"File {reference} routed to Committee Board for authorization.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.StageRouted, "LoanApplication", app.Reference,
                after: "Routed to committee for authorization",
                cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Committee,
                NotificationEvents.RoutedToCommittee,
                $"{app.Reference} is awaiting a board vote",
                $"{app.ApplicantName} — {app.Principal:N0} UGX over {app.TenureMonths} months. " +
                $"{(QuorumEvaluationService.IsBigLoan(app.Principal) ? "Big loan: three approvals including the Chairperson and Treasurer." : "Small loan: one approval.")}",
                app.Reference, cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Applicant,
                NotificationEvents.RoutedToCommittee,
                $"{app.Reference} is with the committee",
                "Underwriting is complete and your file is now with the board for authorisation.",
                app.Reference, app.MemberId, cancellationToken);

            return app;
        }

        if (targetStage.Equals("disbursement", StringComparison.OrdinalIgnoreCase))
        {
            // Enforce quorum requirement before allowing disbursement routing
            var quorumResult = QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);
            
            if (!quorumResult.IsQuorumPassed)
            {
                app.StatusNote = $"File {reference} cannot proceed to disbursement: {quorumResult.Reason}";
                return app;
            }

            // STEP: Lock guarantor shares when loan is disbursed. The lock is written through to
            // the database — held only in memory it vanished on restart, letting the same shares
            // be pledged again to a second loan.
            if (app.Guarantors != null && app.Guarantors.Count > 0)
            {
                app.Guarantors = GuarantorShareLockingService.LockGuarantorShares(app.Guarantors);
                await PersistShareLocksAsync(entity, app.Guarantors, cancellationToken);
            }

            app.Stage = "disbursed";
            app.Status = "disbursed";
            app.DisbursedAt = DateTime.UtcNow;
            app.DeferredForLiquidityAt = null;
            app.DeferredForLiquidityReason = null;
            app.StatusNote = $"File {reference} approved by committee and funds released. Loan is now active and in repayment.";

            if (entity is not null)
            {
                entity.DisbursedAt = app.DisbursedAt;
                entity.DeferredForLiquidityAt = null;
                entity.DeferredForLiquidityReason = null;
            }

            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.FundsReleased, "LoanApplication", app.Reference,
                after: $"Released {app.Principal:N0} over {app.TenureMonths} months; " +
                       $"quorum {quorumResult.ApprovalCount}/{quorumResult.RequiredApprovals}",
                cancellationToken: cancellationToken);

            await _notify.RaiseAsync(
                NotificationAudiences.Applicant,
                NotificationEvents.FundsReleased,
                $"Funds released on {app.Reference}",
                $"{app.Principal:N0} UGX has been released over {app.TenureMonths} months. Repayment starts next month.",
                app.Reference, app.MemberId, cancellationToken);

            if (app.Guarantors.Count > 0)
            {
                await _notify.RaiseAsync(
                    NotificationAudiences.Committee,
                    NotificationEvents.SharesLocked,
                    $"Guarantor shares locked on {app.Reference}",
                    $"{app.Guarantors.Sum(g => g.PledgedShares):N0} UGX of shares across " +
                    $"{app.Guarantors.Count} guarantor(s) are now committed and cannot back another loan " +
                    "until this one is repaid.",
                    app.Reference, cancellationToken: cancellationToken);
            }

            return app;
        }

        app.Stage = targetStage;
        if (targetStage == "underwriting")
        {
            app.StatusNote = $"File {reference} in Underwriting review stage.";
        }

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        return app;
    }

    /// <summary>
    /// Money received against a disbursed loan, and — once it is settled in full — the release of
    /// the guarantors' shares.
    ///
    /// The share-unlock service has existed since the shares were first locked, with nothing ever
    /// calling it: a guarantor who backed a loan that was repaid years ago still had those shares
    /// counted as committed, so they could not stand behind anyone else. This is the workflow that
    /// calls it.
    /// </summary>
    public async Task<LoanApplicationDto?> RecordRepaymentAsync(
        string reference, RecordRepaymentDto dto, CancellationToken cancellationToken = default)
    {
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        if (dto.Amount <= 0)
        {
            throw new ArgumentException("Repayment amount must be greater than 0.");
        }

        if (!app.Stage.Equals("disbursed", StringComparison.OrdinalIgnoreCase))
        {
            app.StatusNote = $"File {reference} has not been disbursed, so there is nothing to repay against it.";
            return app;
        }

        if (app.RepaidAt is not null)
        {
            app.StatusNote = $"Loan {reference} was already settled in full on {app.RepaidAt:dd MMM yyyy}.";
            return app;
        }

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        // Never record more than is owed: an overpayment settles the loan, it does not create a
        // negative balance that would then be released twice.
        var outstandingBefore = Math.Max(0, app.Principal - app.AmountRepaid);
        var applied = Math.Min(dto.Amount, outstandingBefore);
        app.AmountRepaid += applied;
        var outstanding = Math.Max(0, app.Principal - app.AmountRepaid);
        var isSettled = outstanding <= 0;

        if (entity is not null)
        {
            entity.AmountRepaid = app.AmountRepaid;
        }

        if (isSettled)
        {
            app.RepaidAt = DateTime.UtcNow;
            app.Status = "completed";
            app.StatusNote =
                $"Loan {reference} is repaid in full ({app.AmountRepaid:N0} UGX). " +
                $"The shares pledged by {app.Guarantors.Count} guarantor(s) have been released.";

            if (entity is not null)
            {
                entity.RepaidAt = app.RepaidAt;
            }

            if (app.Guarantors.Count > 0)
            {
                app.Guarantors = GuarantorShareLockingService.UnlockGuarantorShares(app.Guarantors);
                await PersistShareReleasesAsync(entity, app.Guarantors, cancellationToken);
            }
        }
        else
        {
            app.StatusNote =
                $"{applied:N0} UGX received against {reference}. " +
                $"{app.AmountRepaid:N0} UGX repaid of {app.Principal:N0} UGX; {outstanding:N0} UGX outstanding. " +
                "Guarantor shares stay locked until the balance is cleared.";
        }

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);

        await _audit.RecordAsync(
            isSettled ? AuditActions.LoanSettled : AuditActions.RepaymentRecorded,
            "LoanApplication", app.Reference,
            actorName: dto.RecordedByRole,
            before: $"repaid {app.AmountRepaid - applied:N0} of {app.Principal:N0}",
            after: isSettled
                ? $"Settled in full; guarantor shares released ({app.Guarantors.Sum(g => g.PledgedShares):N0})"
                : $"repaid {app.AmountRepaid:N0} of {app.Principal:N0}; {outstanding:N0} outstanding",
            cancellationToken: cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Applicant,
            isSettled ? NotificationEvents.LoanSettled : NotificationEvents.RepaymentRecorded,
            isSettled ? $"{reference} is fully repaid" : $"Repayment received on {reference}",
            app.StatusNote,
            app.Reference, app.MemberId, cancellationToken);

        if (isSettled && app.Guarantors.Count > 0)
        {
            await _notify.RaiseAsync(
                NotificationAudiences.Committee,
                NotificationEvents.SharesReleased,
                $"Guarantor shares released on {reference}",
                $"{app.Guarantors.Sum(g => g.PledgedShares):N0} UGX across {app.Guarantors.Count} guarantor(s) " +
                "is free to back another loan.",
                app.Reference, cancellationToken: cancellationToken);
        }

        return app;
    }

    /// <summary>
    /// Marks a file as waiting for cash rather than refused. It keeps its place in the release
    /// queue and continues to count against the liquidity ratio, so the position does not appear
    /// to improve simply because files are stuck.
    /// </summary>
    public async Task<LoanApplicationDto?> DeferForLiquidityAsync(
        string reference, string reason, CancellationToken cancellationToken = default)
    {
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        var alreadyDeferred = app.DeferredForLiquidityAt is not null;

        app.Status = LiquidityService.DeferredStatus;
        app.Stage = "committee";
        app.DeferredForLiquidityAt = app.DeferredForLiquidityAt ?? DateTime.UtcNow;
        app.DeferredForLiquidityReason = reason;
        app.StatusNote = $"Deferred: awaiting liquidity. {reason}";

        if (entity is not null)
        {
            entity.DeferredForLiquidityAt = app.DeferredForLiquidityAt;
            entity.DeferredForLiquidityReason = reason;
        }

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);

        if (!alreadyDeferred)
        {
            await _audit.RecordAsync(AuditActions.DeferredForLiquidity, "LoanApplication", app.Reference,
                after: reason, cancellationToken: cancellationToken);

            await _notify.RaiseForAsync(
                new[] { NotificationAudiences.Committee, NotificationAudiences.Applicant },
                NotificationEvents.DeferredForLiquidity,
                $"{app.Reference} deferred: awaiting liquidity",
                reason, app.Reference, cancellationToken);
        }

        return app;
    }

    /// <summary>
    /// Records that two officers jointly released a file the liquidity gate had locked. The
    /// signatures, the reason and the cash position at the time are all written to the file and
    /// to the audit trail — an override that leaves no trace is indistinguishable from a bug.
    /// </summary>
    public async Task<LoanApplicationDto?> RecordEmergencyOverrideAsync(
        string reference,
        OverrideAuthorizationResult authorization,
        string cashPositionSummary,
        CancellationToken cancellationToken = default)
    {
        var app = await GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null) return null;

        var entity = await _context.LoanApplications
            .FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        app.EmergencyOverrideFirstSeat = authorization.FirstSeat;
        app.EmergencyOverrideSecondSeat = authorization.SecondSeat;
        app.EmergencyOverrideReason = authorization.Reason;
        app.EmergencyOverrideAt = DateTime.UtcNow;

        if (entity is not null)
        {
            entity.EmergencyOverrideFirstSeat = authorization.FirstSeat;
            entity.EmergencyOverrideSecondSeat = authorization.SecondSeat;
            entity.EmergencyOverrideReason = authorization.Reason;
            entity.EmergencyOverrideAt = app.EmergencyOverrideAt;
        }

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);

        await _audit.RecordAsync(AuditActions.EmergencyOverrideUsed, "LoanApplication", app.Reference,
            actorName: authorization.FirstSeat,
            before: cashPositionSummary,
            after: $"Dual-key release by {authorization.FirstSeat} and {authorization.SecondSeat}: {authorization.Reason}",
            cancellationToken: cancellationToken);

        await _notify.RaiseAsync(
            NotificationAudiences.Committee,
            NotificationEvents.EmergencyOverrideUsed,
            $"Emergency release used on {app.Reference}",
            $"{authorization.FirstSeat} and {authorization.SecondSeat} jointly released this file against the " +
            $"liquidity lock. Reason: {authorization.Reason}. Cash position at the time: {cashPositionSummary}",
            app.Reference, cancellationToken: cancellationToken);

        return app;
    }

    /// <summary>
    /// Clears the locks on a settled loan: the shares go back to the guarantors' free balance and
    /// the release is stamped, so a later audit can tell "never locked" from "locked and released".
    /// </summary>
    private async Task PersistShareReleasesAsync(
        LoanApplication? entity, List<GuarantorDto> guarantors, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        try
        {
            var rows = await _context.ApplicationGuarantors
                .Where(g => g.LoanApplicationId == entity.Id)
                .ToListAsync(cancellationToken);

            var releasedAt = DateTime.UtcNow;
            foreach (var guarantor in guarantors)
            {
                var row = rows.FirstOrDefault(r => r.MemberId == guarantor.MemberId);
                if (row is null) continue;

                row.AvailableShares = guarantor.AvailableShares;
                row.LockedShares = 0;
                row.SharesReleasedAt = releasedAt;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Could not release guarantor share locks for {entity.ApplicationNumber} " +
                              $"({ex.GetType().Name}): {ex.Message}. Those shares will still read as committed.");
        }
    }

    /// <summary>
    /// Records that pledged shares are now committed against a disbursed loan, so they cannot be
    /// counted toward another application's coverage.
    /// </summary>
    private async Task PersistShareLocksAsync(LoanApplication? entity, List<GuarantorDto> guarantors, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        try
        {
            var rows = await _context.ApplicationGuarantors
                .Where(g => g.LoanApplicationId == entity.Id)
                .ToListAsync(cancellationToken);

            var lockedAt = DateTime.UtcNow;
            foreach (var guarantor in guarantors)
            {
                var row = rows.FirstOrDefault(r => r.MemberId == guarantor.MemberId);
                if (row is null) continue;

                row.LockedShares = guarantor.PledgedShares;
                row.AvailableShares = guarantor.AvailableShares;
                row.SharesLockedAt = lockedAt;
                row.SharesReleasedAt = null;
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Could not record guarantor share locks for {entity.ApplicationNumber} " +
                              $"({ex.GetType().Name}): {ex.Message}. The same shares could be pledged to another loan.");
        }
    }

    /// <summary>
    /// Loads the persisted guarantors and committee votes for an application and copies them onto
    /// the DTO, so a restart no longer wipes the board's decisions or the pledges behind a file.
    /// </summary>
    private async Task HydrateFromDatabaseAsync(LoanApplicationDto dto, Guid loanApplicationId, CancellationToken cancellationToken)
    {
        var guarantors = await _context.ApplicationGuarantors
            .Where(g => g.LoanApplicationId == loanApplicationId)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync(cancellationToken);

        if (guarantors.Count > 0)
        {
            dto.Guarantors = guarantors.Select(g => new GuarantorDto
            {
                Id = g.Id.ToString("N"),
                Name = g.Name,
                MemberId = g.MemberId,
                PledgedShares = g.PledgedShares,
                AvailableShares = g.AvailableShares,
                LockedShares = g.LockedShares,
                SharesLockedAt = g.SharesLockedAt,
                SharesReleasedAt = g.SharesReleasedAt,
            }).ToList();
        }

        var votes = await _context.CommitteeVotes
            .Where(v => _context.CommitteeReviews
                .Any(r => r.Id == v.CommitteeReviewId && r.LoanApplicationId == loanApplicationId))
            .Join(_context.Users, v => v.VoterUserId, u => u.Id, (v, u) => new { v.Vote, u.FullName, v.VotedAt })
            .OrderBy(x => x.VotedAt)
            .ToListAsync(cancellationToken);

        if (votes.Count > 0)
        {
            dto.CommitteeVotes = votes.Select(v => new CommitteeVoteDetailDto
            {
                MemberName = v.FullName,
                MemberRole = v.FullName, // the seat is the account's name; see DemoUsersSeeder
                Vote = v.Vote,
            }).ToList();
        }
    }

    /// <summary>
    /// Writes the whole workflow state through to the row.
    ///
    /// This used to save only the stage, the amounts and the counter-offer, so the verdict, the
    /// status note and the guardrail results were recomputed as defaults on the next read. That
    /// is why an applicant who declined a revised offer came back marked simply "IN_REVIEW" with
    /// "Verification in progress" — the file genuinely was waiting on more guarantors, but
    /// nothing said so, and a declined verdict could not survive a round trip to block committee
    /// routing either.
    /// </summary>
    private async Task PersistWorkflowFieldsAsync(LoanApplication? entity, LoanApplicationDto app, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        entity.CurrentStatus = app.Status;
        entity.CurrentStage = app.Stage;
        entity.PrincipalAmount = app.Principal;
        entity.TermMonths = app.TenureMonths;

        if (!string.IsNullOrWhiteSpace(app.MemberId))
        {
            entity.MemberId = app.MemberId;
        }

        entity.Verdict = string.IsNullOrWhiteSpace(app.Verdict) ? "PENDING" : app.Verdict;
        entity.StatusNote = app.StatusNote ?? string.Empty;
        entity.SavingsBalance = app.SavingsBalance;
        entity.MonthlyIncome = app.MonthlyIncome;
        entity.MonthlyDebt = app.MonthlyDebt;
        entity.Multiplier = app.Multiplier;
        entity.DtiNetRatio = app.DtiNetRatio;
        entity.NetTakeHome = app.NetTakeHome;
        entity.GuardrailDepositMultiplierPassed = app.GuardrailDepositMultiplierPassed;
        entity.GuardrailOneThirdPayPassed = app.GuardrailOneThirdPayPassed;
        entity.GuardrailGuarantorPassed = app.GuardrailGuarantorPassed;

        entity.CounterOfferPrincipalAmount = app.CounterOfferPrincipal;
        entity.CounterOfferTermMonths = app.CounterOfferTenureMonths;
        entity.CounterOfferReason = app.CounterOfferReason;
        entity.CounterOfferStatus = string.IsNullOrWhiteSpace(app.CounterOfferStatus) ? "NONE" : app.CounterOfferStatus;
        entity.ApplicantConsentAt = app.ApplicantConsentAt;
        entity.ApplicantConsentReceived = app.ApplicantConsentReceived;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The persisted row as the DTO every screen reads. One place, so a field added to the row
    /// cannot be shown on a fresh file and silently dropped on a demo one.
    /// </summary>
    private static LoanApplicationDto ToDto(LoanApplication entity)
    {
        var dto = new LoanApplicationDto
        {
            Id = entity.Id,
            Reference = entity.ApplicationNumber,
            ApplicantName = entity.Applicant?.DisplayName ?? "Amara Trading Ltd",
            MemberId = entity.MemberId,
            ApplicantType = entity.Applicant?.ApplicantType ?? "individual",
            Purpose = entity.Purpose,
            SubmittedOn = (entity.SubmittedAt ?? entity.CreatedAt).ToString("MMM dd, yyyy"),
        };

        return OverlayPersistedWorkflow(dto, entity);
    }

    private static LoanApplicationDto OverlayPersistedWorkflow(LoanApplicationDto dto, LoanApplication entity)
    {
        var isSubmitted = entity.CurrentStatus.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase);

        dto.Status = isSubmitted ? "submitted" : entity.CurrentStatus.ToLowerInvariant();
        dto.Stage = string.IsNullOrWhiteSpace(entity.CurrentStage) ? dto.Stage : entity.CurrentStage;
        dto.Principal = entity.PrincipalAmount;
        dto.TenureMonths = entity.TermMonths;

        if (!string.IsNullOrWhiteSpace(entity.MemberId))
        {
            dto.MemberId = entity.MemberId;
        }

        dto.Verdict = string.IsNullOrWhiteSpace(entity.Verdict) ? "PENDING" : entity.Verdict;
        dto.StatusNote = string.IsNullOrWhiteSpace(entity.StatusNote)
            ? $"Application {entity.ApplicationNumber} submitted. Verification in progress."
            : entity.StatusNote;
        dto.SavingsBalance = entity.SavingsBalance;
        dto.MonthlyIncome = entity.MonthlyIncome;
        dto.MonthlyDebt = entity.MonthlyDebt;
        dto.Multiplier = entity.Multiplier > 0 ? entity.Multiplier : 3.0m;
        dto.DtiNetRatio = entity.DtiNetRatio;
        dto.NetTakeHome = entity.NetTakeHome;
        dto.GuardrailDepositMultiplierPassed = entity.GuardrailDepositMultiplierPassed;
        dto.GuardrailOneThirdPayPassed = entity.GuardrailOneThirdPayPassed;
        dto.GuardrailGuarantorPassed = entity.GuardrailGuarantorPassed;

        dto.CounterOfferPrincipal = entity.CounterOfferPrincipalAmount;
        dto.CounterOfferTenureMonths = entity.CounterOfferTermMonths;
        dto.CounterOfferReason = entity.CounterOfferReason;
        dto.CounterOfferStatus = string.IsNullOrWhiteSpace(entity.CounterOfferStatus) ? "NONE" : entity.CounterOfferStatus;
        dto.ApplicantConsentAt = entity.ApplicantConsentAt;
        dto.ApplicantConsentReceived = entity.ApplicantConsentReceived;

        dto.MinimumAdditionalGuarantorsRequired =
            dto.CounterOfferStatus.Equals("DECLINED", StringComparison.OrdinalIgnoreCase)
                ? MinimumAdditionalGuarantors
                : 0;

        dto.DeferredForLiquidityAt = entity.DeferredForLiquidityAt;
        dto.DeferredForLiquidityReason = entity.DeferredForLiquidityReason;
        dto.EmergencyOverrideFirstSeat = entity.EmergencyOverrideFirstSeat;
        dto.EmergencyOverrideSecondSeat = entity.EmergencyOverrideSecondSeat;
        dto.EmergencyOverrideReason = entity.EmergencyOverrideReason;
        dto.EmergencyOverrideAt = entity.EmergencyOverrideAt;
        dto.AmountRepaid = entity.AmountRepaid;
        dto.RepaidAt = entity.RepaidAt;
        dto.DisbursedAt = entity.DisbursedAt;

        return dto;
    }

    public Task<IEnumerable<CreditPassportMemberDto>> GetCreditPassportMembersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<CreditPassportMemberDto>>(PassportMembers);
    }
}
