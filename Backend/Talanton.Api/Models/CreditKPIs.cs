namespace Talanton.Api.Models;

public class CreditKPI
{
    public Guid Id { get; set; }

    public Guid CreditAssessmentId { get; set; }

    public CreditAssessment CreditAssessment { get; set; } = null!;

    public string KpiCode { get; set; } = string.Empty;

    public string KpiName { get; set; } = string.Empty;

    public decimal KpiValue { get; set; }

    public string? Unit { get; set; }

    public string? CalculationMethodVersion { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
}