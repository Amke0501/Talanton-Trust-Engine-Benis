using Microsoft.AspNetCore.Mvc;
using Talanton.Api.DTOs;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicantController : ControllerBase
{
    private readonly IApplicantService _applicantService;

    public ApplicantController(IApplicantService applicantService)
    {
        _applicantService = applicantService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApplicantDto>>> GetApplicants(CancellationToken cancellationToken)
    {
        var applicants = await _applicantService.GetAllApplicantsAsync(cancellationToken);
        return Ok(applicants);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicantDto>> GetApplicant(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid applicant id.");
        }

        var applicant = await _applicantService.GetApplicantByIdAsync(id, cancellationToken);
        return applicant is null ? NotFound() : Ok(applicant);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicantDto>> CreateApplicant([FromBody] CreateApplicantDto dto, CancellationToken cancellationToken)
    {
        if (dto is null)
        {
            return BadRequest("Applicant payload is required.");
        }

        try
        {
            var createdApplicant = await _applicantService.CreateApplicantAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetApplicant), new { id = createdApplicant.Id }, createdApplicant);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApplicantDto>> UpdateApplicant(Guid id, [FromBody] UpdateApplicantDto dto, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid applicant id.");
        }

        if (dto is null)
        {
            return BadRequest("Applicant payload is required.");
        }

        try
        {
            var updatedApplicant = await _applicantService.UpdateApplicantAsync(id, dto, cancellationToken);
            return updatedApplicant is null ? NotFound() : Ok(updatedApplicant);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteApplicant(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Invalid applicant id.");
        }

        var deleted = await _applicantService.DeleteApplicantAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
