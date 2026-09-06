using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;

namespace Talanton.Api.Services;

/// <summary>
/// Works out whether the SACCO is holding enough cash to keep lending.
///
/// The ratio is liquid cash against the principal already committed to files awaiting release.
/// Below <see cref="MinimumSafeRatio"/> the SACCO is over-extended and further disbursement is
/// blocked — the founder's rule that the numbers alone could not enforce while this lived in a
/// controller with no caller.
/// </summary>
public class LiquidityService
{
    /// <summary>Cash must be at least twice the committed pipeline.</summary>
    public const decimal MinimumSafeRatio = 2.0m;

    /// <summary>Reported when there is no pipeline at all, so the ratio is unbounded.</summary>
    private const decimal UnboundedRatio = 99.99m;

    private static readonly string[] LiquidAccountTypes =
        { "Cash_Vault", "Bank_Current", "Mobile_Money_Float" };

    private readonly ApplicationDbContext _context;

    public LiquidityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LiquidityStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var liquidCash = await _context.LedgerAccounts
            .Where(a => LiquidAccountTypes.Contains(a.AccountType))
            .SumAsync(a => a.CurrentBalance, cancellationToken);

        var pending = await _context.LoanApplications
            .Where(la => la.CurrentStage == "committee" && la.CurrentStatus == "in_review")
            .SumAsync(la => la.PrincipalAmount, cancellationToken);

        var ratio = pending > 0
            ? Math.Round(liquidCash / pending, 2)
            : UnboundedRatio;

        var isLocked = ratio < MinimumSafeRatio;

        return new LiquidityStatus
        {
            TotalLiquidCash = liquidCash,
            TotalPendingLoans = pending,
            CurrentLiquidityRatio = ratio,
            IsLocked = isLocked,
            Deficit = isLocked ? Math.Max(0, (MinimumSafeRatio * pending) - liquidCash) : 0m,
            MaxSafeDisbursementCap = Math.Round(liquidCash / MinimumSafeRatio, 2),
            IsAvailable = true,
        };
    }

    /// <summary>
    /// Status for the disbursement gate. If the ledger cannot be read we do not know that cash is
    /// short, so the release is allowed and the failure is reported loudly — a silent block would
    /// halt all lending on an infrastructure fault. Flip <see cref="LiquidityStatus.IsLocked"/> to
    /// true in the catch below to make the gate fail closed instead.
    /// </summary>
    public async Task<LiquidityStatus> GetStatusForGateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetStatusAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Liquidity could not be evaluated ({ex.GetType().Name}): {ex.Message}. " +
                              "Disbursement proceeded WITHOUT a cash-position check.");
            return new LiquidityStatus { IsAvailable = false, IsLocked = false };
        }
    }
}

public class LiquidityStatus
{
    public decimal TotalLiquidCash { get; set; }
    public decimal TotalPendingLoans { get; set; }
    public decimal CurrentLiquidityRatio { get; set; }
    public bool IsLocked { get; set; }
    public decimal Deficit { get; set; }
    public decimal MaxSafeDisbursementCap { get; set; }

    /// <summary>False when the ledger could not be read, so the figures above mean nothing.</summary>
    public bool IsAvailable { get; set; }
}
