using Microsoft.AspNetCore.Mvc;
using Talanton.Api.Services;

namespace Talanton.Api.Controllers;

/// <summary>
/// The in-app alerts each portal shows in its header.
///
/// QA found no notification feature of any kind — no email, no SMS, no in-app alert — so a
/// revised offer sat unnoticed until somebody happened to reopen the file. Email and SMS need a
/// provider this deployment does not have; in-app alerts can be delivered end to end today, and
/// every alert carries enough to be fanned out to those channels later without re-instrumenting
/// the workflow.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private const int MaxPageSize = 50;

    private static readonly string[] KnownAudiences =
    {
        NotificationAudiences.Applicant,
        NotificationAudiences.Underwriter,
        NotificationAudiences.Committee,
    };

    private readonly NotificationService _notifications;

    public NotificationsController(NotificationService notifications)
    {
        _notifications = notifications;
    }

    /// <summary>
    /// Alerts for one portal. <paramref name="key"/> narrows to one member or seat — an alert
    /// with no key of its own is for everyone in the portal.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetNotifications(
        [FromQuery] string audience,
        [FromQuery] string? key,
        [FromQuery] int take,
        CancellationToken cancellationToken)
    {
        var normalized = Normalize(audience);
        if (normalized is null)
        {
            return BadRequest(new
            {
                message = $"Unknown audience '{audience}'. Expected one of: {string.Join(", ", KnownAudiences)}.",
            });
        }

        var pageSize = take is <= 0 or > MaxPageSize ? 20 : take;
        var rows = await _notifications.GetForAudienceAsync(normalized, key, pageSize, cancellationToken);

        return Ok(new
        {
            audience = normalized,
            unreadCount = rows.Count(r => !r.IsRead),
            notifications = rows.Select(r => new
            {
                id = r.Id,
                title = r.Title,
                body = r.Body,
                eventType = r.EventType,
                reference = r.Reference,
                isRead = r.IsRead,
                createdAt = r.CreatedAt,
            }),
        });
    }

    /// <summary>Marks the given alerts as read for that portal.</summary>
    [HttpPost("mark-read")]
    public async Task<ActionResult<object>> MarkRead(
        [FromBody] MarkNotificationsReadRequest request, CancellationToken cancellationToken)
    {
        var normalized = Normalize(request.Audience);
        if (normalized is null)
        {
            return BadRequest(new
            {
                message = $"Unknown audience '{request.Audience}'. Expected one of: {string.Join(", ", KnownAudiences)}.",
            });
        }

        var updated = await _notifications.MarkReadAsync(normalized, request.Ids ?? new List<Guid>(), cancellationToken);
        return Ok(new { marked = updated });
    }

    private static string? Normalize(string? audience)
    {
        if (string.IsNullOrWhiteSpace(audience)) return null;
        var lowered = audience.Trim().ToLowerInvariant();
        return KnownAudiences.Contains(lowered) ? lowered : null;
    }
}

public class MarkNotificationsReadRequest
{
    public string Audience { get; set; } = string.Empty;
    public List<Guid> Ids { get; set; } = new();
}
