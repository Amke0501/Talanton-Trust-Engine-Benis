using Talanton.Api.Models;

namespace Talanton.Api.Services;

/// <summary>
/// What a member's unpaid invoices prevent them from doing.
///
/// The founder's rule: a client with an unpaid invoice should not receive reports, have funds
/// released, or open new applications until it is settled.
///
/// The hold triggers on any outstanding balance, matching that wording. Whether an invoice within
/// its payment terms should also hold is a policy question rather than a technical one — the
/// overdue invoices are reported separately so the rule can be narrowed to those by changing the
/// single expression in <see cref="Evaluate"/>, without touching the callers.
///
/// Part payment does not lift a hold. A balance is a balance.
/// </summary>
public class InvoiceHoldService
{
    public static InvoiceHoldResult Evaluate(IEnumerable<Invoice> invoices, DateTime? asOf = null)
    {
        var now = asOf ?? DateTime.UtcNow;

        // Only Outstanding invoices count. Cancelled and written-off charges are settled business
        // decisions and must not keep a member locked out.
        var outstanding = invoices
            .Where(i => string.Equals(i.Status, "Outstanding", StringComparison.OrdinalIgnoreCase))
            .Where(i => i.Balance > 0m)
            .ToList();

        var overdue = outstanding.Where(i => i.DueAt.HasValue && i.DueAt.Value < now).ToList();

        var isHeld = outstanding.Count > 0;

        return new InvoiceHoldResult
        {
            IsHeld = isHeld,
            OutstandingCount = outstanding.Count,
            OutstandingBalance = outstanding.Sum(i => i.Balance),
            OverdueCount = overdue.Count,
            OverdueBalance = overdue.Sum(i => i.Balance),
            OldestDueAt = overdue.Count > 0 ? overdue.Min(i => i.DueAt) : null,
            InvoiceNumbers = outstanding.Select(i => i.InvoiceNumber).OrderBy(n => n).ToList(),
            Reason = isHeld
                ? $"{outstanding.Count} unpaid invoice(s) totalling {outstanding.Sum(i => i.Balance):N0}" +
                  (overdue.Count > 0 ? $", of which {overdue.Count} overdue" : ", none yet overdue") +
                  $". Reports, disbursement and new applications are on hold until settled. " +
                  $"Invoice(s): {string.Join(", ", outstanding.Select(i => i.InvoiceNumber).OrderBy(n => n))}."
                : "No unpaid invoices. Nothing is on hold.",
        };
    }
}

public class InvoiceHoldResult
{
    /// <summary>Whether reports, disbursement and new applications are blocked for this member.</summary>
    public bool IsHeld { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int OutstandingCount { get; set; }
    public decimal OutstandingBalance { get; set; }

    /// <summary>Of the outstanding invoices, those already past their due date.</summary>
    public int OverdueCount { get; set; }
    public decimal OverdueBalance { get; set; }

    public DateTime? OldestDueAt { get; set; }

    public List<string> InvoiceNumbers { get; set; } = new();
}
