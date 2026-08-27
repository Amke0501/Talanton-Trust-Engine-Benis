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
}
