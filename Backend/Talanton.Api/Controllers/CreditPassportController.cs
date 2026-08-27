using Microsoft.AspNetCore.Mvc;
using Talanton.Api.DTOs;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditPassportController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;

    public CreditPassportController(ILoanApplicationService loanService)
    {
        _loanService = loanService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CreditPassportMemberDto>>> GetPassportMembers(CancellationToken cancellationToken)
    {
        var members = await _loanService.GetCreditPassportMembersAsync(cancellationToken);
        return Ok(members);
    }
}
