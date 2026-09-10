namespace Talanton.Api.Models;

/// <summary>
/// One in-app alert for one audience.
///
/// The founder asked to be told when a file moves; QA found no notification of any kind. Email
/// and SMS need a provider and credentials the deployment does not have, so the channel that can
/// be delivered end to end today is in-app: a row here, read by the bell in each portal's header.
/// Every alert also carries the address it would be sent to, so wiring an email or SMS gateway
/// later is a matter of draining this table rather than re-instrumenting the workflow.
/// </summary>
public class Notification
{
    public Guid Id { get; set; }

    /// <summary>
    /// Which portal should see it: applicant | underwriter | committee. Kept as the portal role
    /// rather than a user id because the demo signs several people into the same portal.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Optional narrowing within the audience — a member number, or a committee seat.</summary>
    public string? AudienceKey { get; set; }

    /// <summary>The loan file this is about, so the portal can open it.</summary>
    public string? Reference { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    /// <summary>A short machine-readable event name; see <see cref="Services.NotificationEvents"/>.</summary>
    public string EventType { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
