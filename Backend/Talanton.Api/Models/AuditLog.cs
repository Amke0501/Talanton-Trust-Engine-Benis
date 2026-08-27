namespace Talanton.Api.Models;

public class AuditLog
{
    public Guid Id { get; set; }

    public Guid? ActorUserId { get; set; }

    public User? ActorUser { get; set; }

    public string ActionType { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? BeforeData { get; set; }

    public string? AfterData { get; set; }

    public string? CorrelationId { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}