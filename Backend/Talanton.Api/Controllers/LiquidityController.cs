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
        // The gate's own view, so the indicator on screen and the answer the board gets when they
        // press Disburse cannot disagree. Read through the gate rather than the raw query: if the
        // ledger is unreadable the screen must say so, not show a healthy-looking zero.
        var status = await _liquidity.GetStatusForGateAsync(cancellationToken);

        if (!status.IsAvailable)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                isAvailable = false,
                isLocked = true,
                minimumSafeRatio = LiquidityService.MinimumSafeRatio,
                message = "The SACCO's cash position could not be read. Disbursement is blocked until it can be verified.",
            });
        }

        return Ok(new
        {
            totalLiquidCash = status.TotalLiquidCash,
            totalPendingLoans = status.TotalPendingLoans,
            currentLiquidityRatio = status.CurrentLiquidityRatio,
            isLocked = status.IsLocked,
            deficit = status.Deficit,
            maxSafeDisbursementCap = status.MaxSafeDisbursementCap,
            minimumSafeRatio = LiquidityService.MinimumSafeRatio,
            isAvailable = true,

            // The release order, oldest commitment first. Without this the board could see that
            // cash was short but not which file the cash actually reaches.
            queue = status.Queue.Select(q => new
            {
                reference = q.Reference,
                principal = q.Principal,
                queuePosition = q.QueuePosition,
                queuedAt = q.QueuedAt,
                cumulativeDemand = q.CumulativeDemand,
                isWithinSafeCap = q.IsWithinSafeCap,
                status = q.Status,
            }),

            emergencyKeyHolders = EmergencyOverrideService.KeyHolderSeats,
        });
    }
}
