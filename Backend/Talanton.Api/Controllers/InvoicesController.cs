using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Services;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InvoicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>A member's invoices, most recently issued first.</summary>
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetForMember(string memberId, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .Where(i => i.MemberId == memberId)
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);

        return Ok(invoices.Select(i => new
        {
            i.Id,
            i.InvoiceNumber,
            i.Description,
            i.Amount,
            i.AmountPaid,
            i.Balance,
            i.IssuedAt,
            i.DueAt,
            i.SettledAt,
            i.Status,
        }));
    }

    /// <summary>
    /// Whether this member is on hold. Reports, disbursement and new applications all consult this.
    /// </summary>
    [HttpGet("holds/{memberId}")]
    public async Task<IActionResult> GetHold(string memberId, CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .Where(i => i.MemberId == memberId)
            .ToListAsync(cancellationToken);

        var hold = InvoiceHoldService.Evaluate(invoices);

        return Ok(new
        {
            memberId,
            hold.IsHeld,
            hold.Reason,
            hold.OutstandingCount,
            hold.OutstandingBalance,
            hold.OverdueCount,
            hold.OverdueBalance,
            hold.OldestDueAt,
            hold.InvoiceNumbers,
        });
    }
}
