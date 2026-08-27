using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.DTOs;
using Talanton.Api.Models;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Services;

public class LoanApplicationService : ILoanApplicationService
{
    private readonly ApplicationDbContext _context;

    public LoanApplicationService(ApplicationDbContext context)
    {
        _context = context;
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
            StatusNote = "File LA-2026-0941A is declined. BOSA multiplier breach; Payslip take-home deficit.",
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
            Stage = "committee",
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
            Classification = "BOSA",
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
            Classification = "BOSA",
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
            Classification = "BOSA",
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
            Classification = "BOSA",
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
                    resultList.Add(OverlayPersistedWorkflow(existingInMemory, app));
                }
                else
                {
                    var isSubmitted = app.CurrentStatus.Equals("SUBMITTED", StringComparison.OrdinalIgnoreCase) || app.CurrentStatus.Equals("submitted", StringComparison.OrdinalIgnoreCase);
                    resultList.Add(new LoanApplicationDto
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
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Exception retrieving applications from EF Core DB: {ex.Message}");
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
            Console.WriteLine($"[WARNING] Exception saving loan application to EF Core DB: {ex.Message}");
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
            return app;
        }

        app.Verdict = (app.GuardrailDepositMultiplierPassed && app.GuardrailOneThirdPayPassed) ? "APPROVED" : "DECLINED";
        app.StatusNote = app.Verdict == "APPROVED" 
            ? $"File {reference} meets all underwriting guardrail checks." 
            : $"File {reference} is declined. BOSA multiplier breach or Payslip take-home deficit.";

        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
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
            return app;
        }

        app.CounterOfferStatus = "DECLINED";
        app.ApplicantConsentReceived = false;
        app.ApplicantConsentAt = null;
        app.Status = "declined";
        app.Stage = "underwriting";
        app.Verdict = "DECLINED";
        app.StatusNote = "Applicant declined the revised offer. The file cannot proceed without a new underwriting decision.";
        await PersistWorkflowFieldsAsync(entity, app, cancellationToken);
        return app;
    }

    public Task<LoanApplicationDto?> AddGuarantorAsync(string reference, GuarantorDto guarantor, CancellationToken cancellationToken = default)
    {
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
        if (app == null) return Task.FromResult<LoanApplicationDto?>(null);

        guarantor.Id = Guid.NewGuid().ToString("N");
        app.Guarantors.Add(guarantor);

        // Recalculate guarantor cover
        var totalPledged = app.Guarantors.Sum(g => g.PledgedShares);
        var uncollateralized = Math.Max(0, app.Principal - app.SavingsBalance);
        app.GuardrailGuarantorPassed = totalPledged >= uncollateralized;

        return Task.FromResult<LoanApplicationDto?>(app);
    }

    public Task<LoanApplicationDto?> CastVoteAsync(string reference, CastCommitteeVoteDto voteDto, CancellationToken cancellationToken = default)
    {
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
        if (app == null) return Task.FromResult<LoanApplicationDto?>(null);

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

        return Task.FromResult<LoanApplicationDto?>(app);
    }

    public async Task<LoanApplicationDto?> RouteStageAsync(string reference, string targetStage, CancellationToken cancellationToken = default)
    {
        var app = Applications.FirstOrDefault(a => a.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));
        if (app == null) return null;

        var entity = await _context.LoanApplications.FirstOrDefaultAsync(a => a.ApplicationNumber == app.Reference, cancellationToken);

        if (targetStage.Equals("committee", StringComparison.OrdinalIgnoreCase))
        {
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
