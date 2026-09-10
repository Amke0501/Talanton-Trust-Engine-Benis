using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// First in, first out. The gate has to answer two different questions — "is there enough cash at
/// all?" and "has the queue reached this file?" — and the second is what stops a file signed off
/// this morning from overtaking one that has been waiting since Monday.
/// </summary>
public class LiquidityQueueTests
{
    /// <summary>A queue of files, oldest first, against a given safe release cap.</summary>
    private static LiquidityStatus QueueOf(decimal safeCap, params (string Reference, decimal Principal)[] files)
    {
        var status = new LiquidityStatus { MaxSafeDisbursementCap = safeCap, IsAvailable = true };

        decimal running = 0;
        var position = 0;
        foreach (var (reference, principal) in files)
        {
            position++;
            running += principal;
            status.Queue.Add(new LiquidityQueueEntry
            {
                Reference = reference,
                Principal = principal,
                QueuePosition = position,
                QueuedAt = DateTime.UtcNow.AddDays(-files.Length + position),
                CumulativeDemand = running,
                IsWithinSafeCap = running <= safeCap,
                Status = "in_review",
            });
        }

        return status;
    }

    [Fact]
    public void AFileTheCapReachesIsReleasable()
    {
        var status = QueueOf(100_000_000m, ("LA-1", 40_000_000m), ("LA-2", 30_000_000m));

        var result = LiquidityService.EvaluateQueuePosition(status, "LA-2");

        Assert.True(result.IsInQueue);
        Assert.True(result.IsReleasable);
        Assert.Empty(result.BlockedByAhead);
    }

    [Fact]
    public void AFileBeyondTheCapIsDeferred()
    {
        var status = QueueOf(50_000_000m, ("LA-1", 40_000_000m), ("LA-2", 30_000_000m));

        var result = LiquidityService.EvaluateQueuePosition(status, "LA-2");

        Assert.True(result.IsInQueue);
        Assert.False(result.IsReleasable);
        Assert.Equal(70_000_000m, result.Entry!.CumulativeDemand);
    }

    [Fact]
    public void AnOlderFileIsStillReleasableWhenALaterOneIsNot()
    {
        var status = QueueOf(50_000_000m, ("LA-1", 40_000_000m), ("LA-2", 30_000_000m));

        Assert.True(LiquidityService.EvaluateQueuePosition(status, "LA-1").IsReleasable);
        Assert.False(LiquidityService.EvaluateQueuePosition(status, "LA-2").IsReleasable);
    }

    [Fact]
    public void ADeferredFileNamesTheOlderFilesAheadOfIt()
    {
        // The cap covers the first file only; the second and third are both behind it.
        var status = QueueOf(45_000_000m,
            ("LA-1", 40_000_000m), ("LA-2", 30_000_000m), ("LA-3", 10_000_000m));

        var third = LiquidityService.EvaluateQueuePosition(status, "LA-3");

        Assert.False(third.IsReleasable);
        Assert.Contains("LA-2", third.BlockedByAhead);
        // LA-1 is within the cap, so it is not blocking anything.
        Assert.DoesNotContain("LA-1", third.BlockedByAhead);
    }

    [Fact]
    public void AFileOutsideTheCommitteeQueueHasNothingToWaitFor()
    {
        var status = QueueOf(10_000_000m, ("LA-1", 40_000_000m));

        var result = LiquidityService.EvaluateQueuePosition(status, "LA-NOT-QUEUED");

        Assert.False(result.IsInQueue);
        Assert.True(result.IsReleasable);
    }

    [Fact]
    public void TheLookupIgnoresReferenceCasing()
    {
        var status = QueueOf(100_000_000m, ("LA-2026-0001", 10_000_000m));

        var result = LiquidityService.EvaluateQueuePosition(status, "la-2026-0001");

        Assert.True(result.IsInQueue);
        Assert.Equal(1, result.Entry!.QueuePosition);
    }

    [Fact]
    public void AFileExactlyOnTheCapIsStillReleasable()
    {
        var status = QueueOf(40_000_000m, ("LA-1", 40_000_000m));

        Assert.True(LiquidityService.EvaluateQueuePosition(status, "LA-1").IsReleasable);
    }

    [Fact]
    public void DeferredFilesStillCountAsCommitted()
    {
        // A file parked for liquidity has not been paid out, so dropping it from the committed
        // total would make the SACCO's position look better the more files were stuck.
        Assert.Contains(LiquidityService.DeferredStatus, LiquidityService.CommittedStatuses);
        Assert.Contains("in_review", LiquidityService.CommittedStatuses);
        Assert.Contains("approved", LiquidityService.CommittedStatuses);
    }
}
