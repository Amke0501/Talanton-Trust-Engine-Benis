using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiquidityController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public LiquidityController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetLiquidityStatus(CancellationToken cancellationToken)
    {
        var liquidAccounts = await _context.LedgerAccounts
            .Where(a => a.AccountType == "Cash_Vault" || a.AccountType == "Bank_Current" || a.AccountType == "Mobile_Money_Float")
            .ToListAsync(cancellationToken);

        decimal totalLiquidCash = liquidAccounts.Sum(a => a.CurrentBalance);

        // Sum requested principal amount of all loan applications that are in the committee's active review bucket.
        // Active reviews are CurrentStage = "committee" and CurrentStatus = "in_review"
        var pendingLoans = await _context.LoanApplications
            .Where(la => la.CurrentStage == "committee" && la.CurrentStatus == "in_review")
            .ToListAsync(cancellationToken);

        decimal totalPendingLoans = pendingLoans.Sum(la => la.PrincipalAmount);

        decimal currentRatio = totalPendingLoans > 0 
            ? Math.Round(totalLiquidCash / totalPendingLoans, 2) 
            : 99.99m; // Default high value representing infinity / no pending loans

        bool isLocked = currentRatio < 2.0m;
        decimal deficit = isLocked ? Math.Max(0, (2.0m * totalPendingLoans) - totalLiquidCash) : 0m;

        return Ok(new
        {
            totalLiquidCash,
            totalPendingLoans,
            currentLiquidityRatio = currentRatio,
            isLocked,
            deficit,
            maxSafeDisbursementCap = Math.Round(totalLiquidCash / 2.0m, 2)
        });
    }
}
