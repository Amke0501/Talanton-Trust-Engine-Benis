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

    /// <summary>
    /// Statuses that still represent money the SACCO has committed but not yet paid out. A file
    /// deferred for liquidity is very much still committed, so it keeps counting against the
    /// ratio — dropping it would make the position look healthier the more files were stuck.
    /// </summary>
    public static readonly string[] CommittedStatuses =
        { "in_review", "approved", DeferredStatus };

    /// <summary>The status a file takes when the cash position will not reach it yet.</summary>
    public const string DeferredStatus = "deferred_awaiting_liquidity";

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

        // Oldest first: the release queue is first in, first out, so a file that has been waiting
        // since Monday is not overtaken by one signed off this morning simply because the cash
        // ran out in between. SubmittedAt is the applicant's place in the queue; CreatedAt is the
        // fallback for files that predate it.
        var committed = await _context.LoanApplications
            .Where(la => la.CurrentStage == "committee" && CommittedStatuses.Contains(la.CurrentStatus))
            .OrderBy(la => la.SubmittedAt ?? la.CreatedAt)
            .ThenBy(la => la.CreatedAt)
            .Select(la => new
            {
                la.ApplicationNumber,
                la.PrincipalAmount,
                Queued = la.SubmittedAt ?? la.CreatedAt,
                la.CurrentStatus,
            })
            .ToListAsync(cancellationToken);

        var pending = committed.Sum(c => c.PrincipalAmount);

        var ratio = pending > 0
            ? Math.Round(liquidCash / pending, 2)
            : UnboundedRatio;

        var isLocked = ratio < MinimumSafeRatio;
        var cap = Math.Round(liquidCash / MinimumSafeRatio, 2);

        // Walk the queue in order and mark where the safe cap runs out. Everything from that
        // point on is deferred — not refused, just behind a file that has been waiting longer.
        var queue = new List<LiquidityQueueEntry>();
        decimal runningTotal = 0m;
        var position = 0;
        foreach (var item in committed)
        {
            position++;
            runningTotal += item.PrincipalAmount;
            queue.Add(new LiquidityQueueEntry
            {
                Reference = item.ApplicationNumber,
                Principal = item.PrincipalAmount,
                QueuePosition = position,
                QueuedAt = item.Queued,
                CumulativeDemand = runningTotal,
                IsWithinSafeCap = runningTotal <= cap,
                Status = item.CurrentStatus,
            });
        }

        return new LiquidityStatus
        {
            TotalLiquidCash = liquidCash,
            TotalPendingLoans = pending,
            CurrentLiquidityRatio = ratio,
            IsLocked = isLocked,
            Deficit = isLocked ? Math.Max(0, (MinimumSafeRatio * pending) - liquidCash) : 0m,
            MaxSafeDisbursementCap = cap,
            IsAvailable = true,
            Queue = queue,
        };
    }

    /// <summary>
    /// Where one file sits in the release queue: whether the cash on hand reaches it, and if not,
    /// which older files are ahead of it. Consulted by the disbursement gate so that releases go
    /// out in the order they were committed rather than in whatever order the board clicks.
    /// </summary>
    public async Task<QueuePositionResult> GetQueuePositionAsync(
        string reference, CancellationToken cancellationToken = default)
    {
        var status = await GetStatusAsync(cancellationToken);
        return EvaluateQueuePosition(status, reference);
    }

    /// <summary>The queue rule on its own, so it can be exercised without a database.</summary>
    public static QueuePositionResult EvaluateQueuePosition(LiquidityStatus status, string reference)
    {
        var entry = status.Queue.FirstOrDefault(
            q => q.Reference.Equals(reference, StringComparison.OrdinalIgnoreCase));

        if (entry is null)
        {
            // Not in the committee queue — a file being released from elsewhere has nothing
            // ahead of it to wait for.
            return new QueuePositionResult { IsInQueue = false, IsReleasable = true, Status = status };
        }

        var ahead = status.Queue
            .Where(q => q.QueuePosition < entry.QueuePosition && !q.IsWithinSafeCap)
            .Select(q => q.Reference)
            .ToList();

        return new QueuePositionResult
        {
            IsInQueue = true,
            Entry = entry,
            IsReleasable = entry.IsWithinSafeCap,
            BlockedByAhead = ahead,
            Status = status,
        };
    }

    /// <summary>
    /// Status for the disbursement gate.
    ///
    /// If the ledger cannot be read we do not know that the cash is there, and releasing money
    /// against an unverified cash position is the exact failure this gate exists to prevent — so
    /// it fails closed. QA flagged the previous fail-open behaviour as a serious operational
    /// risk: an unreachable database silently disabled the check while every screen still
    /// reported the release as authorised.
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
                              "Disbursement was REFUSED because the cash position could not be verified.");
            return new LiquidityStatus { IsAvailable = false, IsLocked = true };
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

    /// <summary>The committed files in release order, oldest first.</summary>
    public List<LiquidityQueueEntry> Queue { get; set; } = new();
}

/// <summary>One committed file's place in the first-in, first-out release queue.</summary>
public class LiquidityQueueEntry
{
    public string Reference { get; set; } = string.Empty;
    public decimal Principal { get; set; }
    public int QueuePosition { get; set; }
    public DateTime QueuedAt { get; set; }

    /// <summary>Total principal of this file and everything ahead of it.</summary>
    public decimal CumulativeDemand { get; set; }

    /// <summary>Whether the safe release cap still covers the queue up to and including this file.</summary>
    public bool IsWithinSafeCap { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class QueuePositionResult
{
    public bool IsInQueue { get; set; }
    public LiquidityQueueEntry? Entry { get; set; }
    public bool IsReleasable { get; set; }
    public List<string> BlockedByAhead { get; set; } = new();
    public LiquidityStatus Status { get; set; } = new();
}
