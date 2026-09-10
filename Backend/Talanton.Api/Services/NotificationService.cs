using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Models;

namespace Talanton.Api.Services;

/// <summary>
/// Raises the in-app alerts each portal shows in its header.
///
/// Like <see cref="AuditService"/>, notifying never blocks the action it describes: a file that
/// was correctly routed must not be rolled back because an alert could not be written.
/// </summary>
public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RaiseAsync(
        string audience,
        string eventType,
        string title,
        string body,
        string? reference = null,
        string? audienceKey = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                Audience = audience,
                AudienceKey = string.IsNullOrWhiteSpace(audienceKey) ? null : audienceKey,
                Reference = reference,
                Title = title,
                Body = body,
                EventType = eventType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Notification '{eventType}' for {audience} was not written " +
                              $"({ex.GetType().Name}): {ex.Message}. The action itself still took effect.");
        }
    }

    /// <summary>Raises the same alert for several portals at once.</summary>
    public async Task RaiseForAsync(
        IEnumerable<string> audiences,
        string eventType,
        string title,
        string body,
        string? reference = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var audience in audiences)
        {
            await RaiseAsync(audience, eventType, title, body, reference, cancellationToken: cancellationToken);
        }
    }

    public async Task<List<Notification>> GetForAudienceAsync(
        string audience, string? audienceKey, int take, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications.Where(n => n.Audience == audience);

        if (!string.IsNullOrWhiteSpace(audienceKey))
        {
            // An alert with no key is for everyone in the portal; one with a key is for that
            // member or seat alone.
            query = query.Where(n => n.AudienceKey == null || n.AudienceKey == audienceKey);
        }

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> MarkReadAsync(
        string audience, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var wanted = ids.ToList();
        if (wanted.Count == 0) return 0;

        var rows = await _context.Notifications
            .Where(n => n.Audience == audience && wanted.Contains(n.Id))
            .ToListAsync(cancellationToken);

        foreach (var row in rows)
        {
            row.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return rows.Count;
    }
}

/// <summary>The event names used on notifications, kept together so they stay searchable.</summary>
public static class NotificationEvents
{
    public const string ApplicationSubmitted = "APPLICATION_SUBMITTED";
    public const string CounterOfferSent = "COUNTER_OFFER_SENT";
    public const string CounterOfferAccepted = "COUNTER_OFFER_ACCEPTED";
    public const string CounterOfferDeclined = "COUNTER_OFFER_DECLINED";
    public const string GuarantorsRequired = "GUARANTORS_REQUIRED";
    public const string ResubmittedWithGuarantors = "RESUBMITTED_WITH_GUARANTORS";
    public const string UnderwritingDeclined = "UNDERWRITING_DECLINED";
    public const string RoutedToCommittee = "ROUTED_TO_COMMITTEE";
    public const string VoteCast = "COMMITTEE_VOTE_CAST";
    public const string FundsReleased = "FUNDS_RELEASED";
    public const string ReleaseRefused = "RELEASE_REFUSED";
    public const string DeferredForLiquidity = "DEFERRED_AWAITING_LIQUIDITY";
    public const string EmergencyOverrideUsed = "EMERGENCY_OVERRIDE_USED";
    public const string SharesLocked = "GUARANTOR_SHARES_LOCKED";
    public const string SharesReleased = "GUARANTOR_SHARES_RELEASED";
    public const string RepaymentRecorded = "REPAYMENT_RECORDED";
    public const string LoanSettled = "LOAN_SETTLED";
}

/// <summary>The portals an alert can be addressed to.</summary>
public static class NotificationAudiences
{
    public const string Applicant = "applicant";
    public const string Underwriter = "underwriter";
    public const string Committee = "committee";
}
