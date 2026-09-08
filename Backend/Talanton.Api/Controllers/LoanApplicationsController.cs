using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.DTOs;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;
    private readonly Services.LiquidityService _liquidity;
    private readonly Services.AuditService _audit;
    private readonly Data.ApplicationDbContext _context;

    public LoanApplicationsController(
        ILoanApplicationService loanService,
        Services.LiquidityService liquidity,
        Services.AuditService audit,
        Data.ApplicationDbContext context)
    {
        _loanService = loanService;
        _liquidity = liquidity;
        _audit = audit;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetApplications(CancellationToken cancellationToken)
    {
        var apps = await _loanService.GetAllLoanApplicationsAsync(cancellationToken);
        return Ok(apps);
    }

    [HttpGet("{reference}")]
    public async Task<ActionResult<LoanApplicationDto>> GetApplicationByRef(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        return app == null ? NotFound() : Ok(app);
    }

    [HttpPost]
    public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] CreateLoanApplicationDto dto, CancellationToken cancellationToken)
    {
        var created = await _loanService.CreateLoanApplicationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetApplicationByRef), new { reference = created.Reference }, created);
    }

    [HttpPut("{reference}/underwrite")]
    public async Task<ActionResult<LoanApplicationDto>> UpdateUnderwriting(string reference, [FromBody] UpdateUnderwritingOverrideDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.UpdateUnderwritingAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/counter-offer")]
    public async Task<ActionResult<LoanApplicationDto>> RespondToCounterOffer(string reference, [FromBody] CounterOfferDecisionDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.RespondToCounterOfferAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// An applicant's second attempt after declining a revised offer, on the strength of at least
    /// two additional guarantors. Returns the file to underwriting, never straight to committee.
    /// </summary>
    [HttpPost("{reference}/resubmit-with-guarantors")]
    public async Task<ActionResult<LoanApplicationDto>> ResubmitWithGuarantors(
        string reference, [FromBody] ResubmitWithGuarantorsDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.ResubmitWithGuarantorsAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/guarantor")]
    public async Task<ActionResult<LoanApplicationDto>> AddGuarantor(string reference, [FromBody] GuarantorDto guarantor, CancellationToken cancellationToken)
    {
        var updated = await _loanService.AddGuarantorAsync(reference, guarantor, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/vote")]
    public async Task<ActionResult<LoanApplicationDto>> CastVote(string reference, [FromBody] CastCommitteeVoteDto voteDto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.CastVoteAsync(reference, voteDto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/route")]
    public async Task<ActionResult<LoanApplicationDto>> RouteStage(string reference, [FromQuery] string stage, CancellationToken cancellationToken)
    {
        var updated = await _loanService.RouteStageAsync(reference, stage, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/quorum-check")]
    public async Task<ActionResult<QuorumCheckResponseDto>> CheckQuorum(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        var result = Services.QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);

        return Ok(new QuorumCheckResponseDto
        {
            IsQuorumPassed = result.IsQuorumPassed,
            Reason = result.Reason,
            IsBigLoan = result.IsBigLoan,
            RequiredApprovals = result.RequiredApprovals,
            ApprovalCount = result.ApprovalCount,
            HasChairpersonVeto = result.HasChairpersonVeto,
            HasRequiredMembers = result.HasRequiredMembers
        });
    }

    [HttpPost("{reference}/disburse")]
    public async Task<ActionResult<DisbursementAuthorizationResponseDto>> DisburseFunds(
        string reference,
        [FromBody] DisbursementAuthorizationDto authDto,
        CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        // Step 1: Verify quorum has passed
        var quorumResult = Services.QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);
        if (!quorumResult.IsQuorumPassed)
        {
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — quorum not met. {quorumResult.Reason}",
                cancellationToken: cancellationToken);

            return BadRequest(new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = $"Cannot disburse: Quorum requirement not met. {quorumResult.Reason}"
            });
        }

        // Step 2: Check disbursement authorization
        var authResult = Services.DisbursementAuthorizationService.EvaluateDisbursementAuthority(
            app.Principal,
            authDto.RequestorRole,
            !string.IsNullOrEmpty(authDto.ChairpersonSignature),
            !string.IsNullOrEmpty(authDto.SecretarySignature)
        );

        // Step 2b: the SACCO must still be holding enough cash. Authority to release is not the
        // same as having the money; releasing into an over-extended position is what this stops.
        var liquidity = await _liquidity.GetStatusForGateAsync(cancellationToken);
        if (liquidity.IsLocked)
        {
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — liquidity ratio {liquidity.CurrentLiquidityRatio:0.00} below minimum",
                cancellationToken: cancellationToken);

            return StatusCode(StatusCodes.Status409Conflict, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = $"Disbursement is on hold: liquidity ratio is {liquidity.CurrentLiquidityRatio:0.00}, " +
                         $"below the {Services.LiquidityService.MinimumSafeRatio:0.00} minimum. " +
                         $"Cash on hand {liquidity.TotalLiquidCash:N0} against {liquidity.TotalPendingLoans:N0} committed; " +
                         $"shortfall {liquidity.Deficit:N0}."
            });
        }

        if (!authResult.IsAuthorized)
        {
            // Forbid() challenges the default authentication scheme; none is registered, so it
            // cannot produce a clean 403 here. Return the status directly with the reason, so the
            // caller can show the committee why the release was refused.
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — {authResult.Reason}",
                cancellationToken: cancellationToken);

            return StatusCode(StatusCodes.Status403Forbidden, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = authResult.Reason
            });
        }

        // Step 3: Execute disbursement (route to disbursed stage)
        var updated = await _loanService.RouteStageAsync(reference, "disbursement", cancellationToken);

        if (updated == null || updated.Stage != "disbursed")
        {
            return StatusCode(500, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = "Disbursement routing failed."
            });
        }

        // Step 4: Return success response
        return Ok(new DisbursementAuthorizationResponseDto
        {
            IsAuthorized = true,
            Reason = $"Loan {reference} successfully disbursed.",
            UpdatedApplication = updated,
            DisbursementAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Whether this member's own savings support the amount requested — the per-member check,
    /// distinct from the SACCO-wide liquidity position.
    /// </summary>
    [HttpGet("{reference}/member-savings-check")]
    public async Task<ActionResult<object>> CheckMemberSavings(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        // The SACCO's own record of what this member has saved, rather than the figure entered on
        // the application. Falls back to the declared figure when no membership record exists, and
        // says so, so the check is never silently measured against the applicant's own claim.
        var membership = await _context.SaccoMemberships
            .FirstOrDefaultAsync(m => m.MembershipNumber == app.MemberId, cancellationToken);

        var recorded = membership?.SavingsBalance ?? app.SavingsBalance;

        var result = Services.MemberSavingsService.Evaluate(
            app.Principal, recorded, app.Multiplier, app.SavingsBalance);

        return Ok(new
        {
            result.IsWithinCap,
            result.Reason,
            result.RecordedSavings,
            result.Multiplier,
            result.MaximumBorrowable,
            result.RequestedPrincipal,
            result.ExcessAmount,
            result.DeclaredSavings,
            result.DeclaredSavingsMismatch,
            result.DeclaredSavingsDifference,
            SavingsSource = membership is null ? "application (no membership record found)" : "SACCO membership record",
        });
    }

    [HttpGet("{reference}/guarantor-coverage")]
    public async Task<ActionResult<object>> CheckGuarantorCoverage(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        var coverage = Services.GuarantorShareLockingService.EvaluateGuarantorCoverage(
            app.Guarantors,
            app.Principal,
            app.SavingsBalance
        );

        return Ok(new
        {
            coverage.IsCovered,
            coverage.LoanGap,
            coverage.TotalPledgedShares,
            coverage.TotalAvailableShares,
            coverage.Deficit,
            coverage.Reason,
            Guarantors = app.Guarantors.Select(g => new
            {
                g.Id,
                g.Name,
                g.MemberId,
                g.PledgedShares,
                g.AvailableShares,
                IsCapacitySufficient = Services.GuarantorShareLockingService.ValidateGuarantorCapacity(g, g.PledgedShares).IsValid
            })
        });
    }
}
