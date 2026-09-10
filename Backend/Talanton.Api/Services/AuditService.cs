using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Models;

namespace Talanton.Api.Services;

/// <summary>
/// Records who did what, to which file, and when.
///
/// The AuditLogs table has existed since the first migration and nothing ever wrote a row, so
/// there was no record of who approved, rejected or released a loan — the founder's standing
/// requirement before this handles real money.
///
/// Auditing never blocks the action it describes: a failure to record is reported loudly but the
/// decision still stands, because refusing a valid approval over a logging fault would be worse.
/// </summary>
public class AuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecordAsync(
        string actionType,
        string entityType,
        string entityId,
        string? actorName = null,
        string? before = null,
        string? after = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guid? actorId = null;
            if (!string.IsNullOrWhiteSpace(actorName))
            {
                // Committee seats are held by accounts named for the seat; see DemoUsersSeeder.
                var actor = await _context.Users.FirstOrDefaultAsync(
                    u => u.FullName.ToLower() == actorName.ToLower(), cancellationToken);
                actorId = actor?.Id;
            }

            _context.AuditLogs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                ActorUserId = actorId,
                ActionType = actionType,
                EntityType = entityType,
                EntityId = entityId,
                BeforeData = before,
                AfterData = after,
                OccurredAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Audit record '{actionType}' for {entityType} {entityId} was not written " +
                              $"({ex.GetType().Name}): {ex.Message}. The action itself still took effect.");
        }
    }
}

/// <summary>The action names written to the audit trail, kept in one place so they stay searchable.</summary>
public static class AuditActions
{
    public const string VoteCast = "COMMITTEE_VOTE_CAST";
    public const string UnderwritingDecided = "UNDERWRITING_DECIDED";
    public const string CounterOfferMade = "COUNTER_OFFER_MADE";
    public const string CounterOfferAnswered = "COUNTER_OFFER_ANSWERED";
    public const string StageRouted = "STAGE_ROUTED";
    public const string ResubmittedWithGuarantors = "RESUBMITTED_WITH_GUARANTORS";
    public const string FundsReleased = "FUNDS_RELEASED";
    public const string ReleaseRefused = "RELEASE_REFUSED";
    public const string DeferredForLiquidity = "DEFERRED_AWAITING_LIQUIDITY";
    public const string EmergencyOverrideUsed = "EMERGENCY_RELEASE_OVERRIDE";
    public const string RepaymentRecorded = "REPAYMENT_RECORDED";
    public const string LoanSettled = "LOAN_SETTLED";
}
