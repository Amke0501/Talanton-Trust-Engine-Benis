using Talanton.Api.Models;
using Talanton.Api.Services;

namespace Talanton.Api.Tests;

/// <summary>
/// Unpaid invoices hold back reports, disbursement and new applications.
/// </summary>
public class InvoiceHoldServiceTests
{
    private static readonly DateTime Now = new(2026, 9, 8, 0, 0, 0, DateTimeKind.Utc);

    private static Invoice Inv(
        string number, decimal amount, decimal paid = 0m, string status = "Outstanding", int dueInDays = -7) =>
        new()
        {
            Id = Guid.NewGuid(),
            MemberId = "M-8842",
            InvoiceNumber = number,
            Amount = amount,
            AmountPaid = paid,
            Status = status,
            IssuedAt = Now.AddDays(-30),
            DueAt = Now.AddDays(dueInDays),
        };

    [Fact]
    public void A_member_with_no_invoices_is_not_held()
    {
        var r = InvoiceHoldService.Evaluate(new List<Invoice>(), Now);

        Assert.False(r.IsHeld);
        Assert.Equal(0m, r.OutstandingBalance);
    }

    [Fact]
    public void A_single_unpaid_invoice_holds_the_member()
    {
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-001", 250_000m) }, Now);

        Assert.True(r.IsHeld);
        Assert.Equal(250_000m, r.OutstandingBalance);
        Assert.Contains("INV-001", r.Reason);
    }

    [Fact]
    public void A_fully_paid_invoice_does_not_hold()
    {
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-002", 250_000m, paid: 250_000m) }, Now);
        Assert.False(r.IsHeld);
    }

    [Fact]
    public void Part_payment_does_not_lift_the_hold()
    {
        // A balance is a balance; paying some of it should not restore access.
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-003", 250_000m, paid: 249_999m) }, Now);

        Assert.True(r.IsHeld);
        Assert.Equal(1m, r.OutstandingBalance);
    }

    [Fact]
    public void Overpayment_does_not_produce_a_negative_balance()
    {
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-004", 100_000m, paid: 150_000m) }, Now);

        Assert.False(r.IsHeld);
        Assert.Equal(0m, r.OutstandingBalance);
    }

    [Theory]
    [InlineData("Cancelled")]
    [InlineData("WrittenOff")]
    [InlineData("Paid")]
    public void Invoices_that_are_no_longer_owed_do_not_hold(string status)
    {
        // Cancelling or writing off a charge is a settled decision and must not keep a member out.
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-005", 500_000m, status: status) }, Now);
        Assert.False(r.IsHeld);
    }

    [Fact]
    public void Overdue_invoices_are_reported_separately_from_merely_unpaid_ones()
    {
        var r = InvoiceHoldService.Evaluate(new[]
        {
            Inv("INV-006", 100_000m, dueInDays: -10),  // overdue
            Inv("INV-007", 200_000m, dueInDays: 10),   // unpaid, still within terms
        }, Now);

        Assert.True(r.IsHeld);
        Assert.Equal(2, r.OutstandingCount);
        Assert.Equal(300_000m, r.OutstandingBalance);
        Assert.Equal(1, r.OverdueCount);
        Assert.Equal(100_000m, r.OverdueBalance);
    }

    [Fact]
    public void The_oldest_due_date_is_reported_for_overdue_invoices()
    {
        var r = InvoiceHoldService.Evaluate(new[]
        {
            Inv("INV-008", 100_000m, dueInDays: -3),
            Inv("INV-009", 100_000m, dueInDays: -60),
        }, Now);

        Assert.Equal(Now.AddDays(-60), r.OldestDueAt);
    }

    [Fact]
    public void An_invoice_with_no_due_date_still_holds_but_is_not_overdue()
    {
        var invoice = Inv("INV-010", 50_000m);
        invoice.DueAt = null;

        var r = InvoiceHoldService.Evaluate(new[] { invoice }, Now);

        Assert.True(r.IsHeld);
        Assert.Equal(0, r.OverdueCount);
        Assert.Contains("none yet overdue", r.Reason);
    }

    [Fact]
    public void The_reason_names_every_invoice_holding_the_member()
    {
        var r = InvoiceHoldService.Evaluate(new[] { Inv("INV-B", 1m), Inv("INV-A", 1m) }, Now);

        Assert.Contains("INV-A", r.Reason);
        Assert.Contains("INV-B", r.Reason);
        Assert.Equal(new[] { "INV-A", "INV-B" }, r.InvoiceNumbers);
    }
}
