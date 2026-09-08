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

    public LoanApplicationService(ApplicationDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
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
                    var isSubmitted = app.CurrentStatus.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase) || app.CurrentStatus.Equals("submitted", StringComparison.OrdinalIgnoreCase);
                    var dto = new LoanApplicationDto
                    {
                        Id = app.Id,
                        Reference = app.ApplicationNumber,
                        ApplicantName = app.Applicant?.DisplayName ?? "Amara Trading Ltd",
                        MemberId = "APP-TEST-001",
                        ApplicantType = app.Applicant?.ApplicantType ?? "cooperative",
                        Status = isSubmitted ? "submitted" : app.CurrentStatus.ToLowerInvariant(),
                        Stage = string.IsNullOrWhiteSpace(app.CurrentStage) ? (isSubmitted ? "verification" : "underwriting") : app.CurrentStage,
                        Principal = app.PrincipalAmount,
                        Purpose = app.Purpose,
                        TenureMonths = app.TermMonths,
                        SavingsBalance = 2000000m,
                        MonthlyIncome = 1500000m,
                        MonthlyDebt = 300000m,
                        Multiplier = 3.0m,
                        SubmittedOn = app.SubmittedAt?.ToString("MMM dd, yyyy") ?? app.CreatedAt.ToString("MMM dd, yyyy"),
                        StatusNote = $"Application {app.ApplicationNumber} submitted. Verification in progress.",
                        DtiNetRatio = 20.0m,
                        NetTakeHome = 1200000m,
                        Verdict = "IN_REVIEW",
                        CounterOfferPrincipal = app.CounterOfferPrincipalAmount,
                        CounterOfferTenureMonths = app.CounterOfferTermMonths,
                        CounterOfferReason = app.CounterOfferReason,
                        CounterOfferStatus = string.IsNullOrWhiteSpace(app.CounterOfferStatus) ? "NONE" : app.CounterOfferStatus,
                        ApplicantConsentAt = app.ApplicantConsentAt,
                        ApplicantConsentReceived = app.ApplicantConsentReceived
                    };

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
                CreatedAt = DateTime.UtcNow
            };

            _context.LoanApplications.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
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
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
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
            app.StatusNote = $"Revised offer sent to applicant: principal reduced from {originalPrincipal:C} to {dto.RequestedPrincipal:C} and tenure changed from {originalTenure} to {dto.TenureMonths} months. Applicant consent is required before the file can move to committee.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.CounterOfferMade, "LoanApplication", app.Reference,
                before: $"principal {originalPrincipal:N0} over {originalTenure} months",
                after: $"principal {dto.RequestedPrincipal:N0} over {dto.TenureMonths} months",
                cancellationToken: cancellationToken);
            return app;
        }

        app.Verdict = (app.GuardrailDepositMultiplierPassed && app.GuardrailOneThirdPayPassed) ? "APPROVED" : "DECLINED";
        app.StatusNote = app.Verdict == "APPROVED" 
            ? $"File {reference} meets all underwriting guardrail checks." 
            : $"File {reference} is declined. Individual multiplier breach or Payslip take-home deficit.";

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.UnderwritingDecided, "LoanApplication", app.Reference,
            after: $"Verdict {app.Verdict}; principal {app.Principal:N0} over {app.TenureMonths} months",
            cancellationToken: cancellationToken);
        return app;
    }

    public async Task<LoanApplicationDto?> RespondToCounterOfferAsync(string reference, CounterOfferDecisionDto dto, CancellationToken cancellationToken = default)
    {
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
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
            app.StatusNote = "Applicant accepted the revised offer. Consent has been recorded and the file can proceed to committee review.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.CounterOfferAnswered, "LoanApplication", app.Reference,
                after: $"Applicant ACCEPTED: principal {app.Principal:N0} over {app.TenureMonths} months",
                cancellationToken: cancellationToken);
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
        app.StatusNote =
            $"Applicant declined the revised offer. The file can be reconsidered if at least " +
            $"{MinimumAdditionalGuarantors} additional guarantors are added; otherwise it terminates here.";
        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.CounterOfferAnswered, "LoanApplication", app.Reference,
            after: "Applicant DECLINED the revised offer",
            cancellationToken: cancellationToken);
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
        app.StatusNote =
            $"Applicant added {additions.Count} additional guarantors after declining the revised offer. " +
            $"Total pledged is now {totalPledged:N0} against an uncollateralised gap of {uncollateralized:N0}. " +
            "Returned to underwriting for a fresh decision.";

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        await _audit.RecordAsync(AuditActions.ResubmittedWithGuarantors, "LoanApplication", app.Reference,
            after: $"Added {additions.Count} guarantors ({string.Join(", ", additions.Select(g => g.MemberId))}); " +
                   $"returned to underwriting",
            cancellationToken: cancellationToken);

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
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
        if (app == null) return null;

        var entity = await _context.LoanApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        if (targetStage.Equals("committee", StringComparison.OrdinalIgnoreCase))
        {
            // Block declined files from reaching committee — verdict is set by UpdateUnderwritingAsync().
            //
            // FOLLOW-UP #1 (verdict formula mismatch): The verdict computation in
            // UpdateUnderwritingAsync (line ~535) only checks GuardrailDepositMultiplierPassed &&
            // GuardrailOneThirdPayPassed, whereas the frontend (underwriter-dashboard-view.tsx)
            // also includes GuardrailGuarantorPassed in its overallPassed calculation. This
            // mismatch should be reconciled in a separate change.
            //
            // FOLLOW-UP #2 (StatusNote overlap): When an applicant declines a counter-offer,
            // both Verdict and CounterOfferStatus are set to "DECLINED". Because this verdict
            // check fires first, the StatusNote will read "failed underwriting guardrail checks"
            // rather than the more specific counter-offer message below. The block is correct
            // either way; only the message is less precise for that path.
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

            if (app.CounterOfferStatus == "DECLINED")
            {
                app.Status = "declined";
                app.Stage = "underwriting";
                app.StatusNote = $"File {reference} was declined by the applicant after the counter-offer and cannot bypass consent to reach committee.";
                await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
                return app;
            }

            app.Stage = targetStage;
            app.Status = "in_review";
            app.StatusNote = $"File {reference} routed to Committee Board for authorization.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.StageRouted, "LoanApplication", app.Reference,
                after: "Routed to committee for authorization",
                cancellationToken: cancellationToken);
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
            app.StatusNote = $"File {reference} approved by committee and funds released. Loan is now active and in repayment.";
            await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
            await _audit.RecordAsync(AuditActions.FundsReleased, "LoanApplication", app.Reference,
                after: $"Released {app.Principal:N0} over {app.TenureMonths} months; " +
                       $"quorum {quorumResult.ApprovalCount}/{quorumResult.RequiredApprovals}",
                cancellationToken: cancellationToken);
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

    private async Task PersistWorkflowFieldsAsync(LoanApplication? entity, LoanApplicationDto app, CancellationToken cancellationToken)
    {
        if (entity is null) return;

        entity.CurrentStatus = app.Status;
        entity.CurrentStage = app.Stage;
        entity.PrincipalAmount = app.Principal;
        entity.TermMonths = app.TenureMonths;
        entity.CounterOfferPrincipalAmount = app.CounterOfferPrincipal;
        entity.CounterOfferTermMonths = app.CounterOfferTenureMonths;
        entity.CounterOfferReason = app.CounterOfferReason;
        entity.CounterOfferStatus = string.IsNullOrWhiteSpace(app.CounterOfferStatus) ? "NONE" : app.CounterOfferStatus;
        entity.ApplicantConsentAt = app.ApplicantConsentAt;
        entity.ApplicantConsentReceived = app.ApplicantConsentReceived;

        await _context.SaveChangesAsync(cancellationToken);
    }

    private static LoanApplicationDto OverlayPersistedWorkflow(LoanApplicationDto dto, LoanApplication entity)
    {
        dto.Status = entity.CurrentStatus.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase) ? "submitted" : entity.CurrentStatus.ToLowerInvariant();
        dto.Stage = string.IsNullOrWhiteSpace(entity.CurrentStage) ? dto.Stage : entity.CurrentStage;
        dto.Principal = entity.PrincipalAmount;
        dto.TenureMonths = entity.TermMonths;
        dto.CounterOfferPrincipal = entity.CounterOfferPrincipalAmount;
        dto.CounterOfferTenureMonths = entity.CounterOfferTermMonths;
        dto.CounterOfferReason = entity.CounterOfferReason;
        dto.CounterOfferStatus = string.IsNullOrWhiteSpace(entity.CounterOfferStatus) ? "NONE" : entity.CounterOfferStatus;
        dto.ApplicantConsentAt = entity.ApplicantConsentAt;
        dto.ApplicantConsentReceived = entity.ApplicantConsentReceived;
        return dto;
    }

    public Task<IEnumerable<CreditPassportMemberDto>> GetCreditPassportMembersAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<CreditPassportMemberDto>>(PassportMembers);
    }
}
