using Microsoft.AspNetCore.Mvc;
using Talanton.Api.Services;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LiquidityController : ControllerBase
{
    private readonly LiquidityService _liquidity;

    public LiquidityController(LiquidityService liquidity)
    {
        _liquidity = liquidity;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetLiquidityStatus(CancellationToken cancellationToken)
    {
        var status = await _liquidity.GetStatusAsync(cancellationToken);

        return Ok(new
        {
            totalLiquidCash = status.TotalLiquidCash,
            totalPendingLoans = status.TotalPendingLoans,
            currentLiquidityRatio = status.CurrentLiquidityRatio,
            isLocked = status.IsLocked,
            deficit = status.Deficit,
            maxSafeDisbursementCap = status.MaxSafeDisbursementCap,
            minimumSafeRatio = LiquidityService.MinimumSafeRatio
        });
    }
}
