namespace Talanton.Api.Models;

/// <summary>
/// A charge raised against a member — service fees, penalties, recoveries.
///
/// The founder's rule is that a client with an unpaid invoice should not receive reports, have
/// funds released, or open new applications until it is settled. Nothing recorded invoices at all,
/// so the rule had nothing to act on.
/// </summary>
public class Invoice
{
    public Guid Id { get; set; }

    /// <summary>The SACCO membership number the invoice is raised against.</summary>
    public string MemberId { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Settled so far. Part payment reduces the balance but does not lift a hold.</summary>
    public decimal AmountPaid { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueAt { get; set; }

    public DateTime? SettledAt { get; set; }

    /// <summary>Outstanding | Paid | Cancelled | WrittenOff. Only Outstanding creates a hold.</summary>
    public string Status { get; set; } = "Outstanding";

    /// <summary>What is still owed on this invoice.</summary>
    public decimal Balance => Math.Max(0m, Amount - AmountPaid);
}
