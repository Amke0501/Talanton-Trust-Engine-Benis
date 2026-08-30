using Microsoft.AspNetCore.Mvc;
using Talanton.Api.DTOs;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;

    public LoanApplicationsController(ILoanApplicationService loanService)
    {
        _loanService = loanService;
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

        if (!authResult.IsAuthorized)
        {
            return Forbid(); // 403 Forbidden
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
