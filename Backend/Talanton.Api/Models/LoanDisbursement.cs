namespace Talanton.Api.Models;

public class LoanDisbursement
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid ApplicationDecisionId { get; set; }

    public ApplicationDecision ApplicationDecision { get; set; } = null!;

    public Guid? ProcessedByUserId { get; set; }

    public User? ProcessedByUser { get; set; }

    public string DisbursementStatus { get; set; } = "Pending";

    public decimal PrincipalDisbursedAmount { get; set; }

    public decimal AdministrativeFeeDeductedAmount { get; set; }

    public decimal NetAmountToBorrower { get; set; }

    public string? PaymentChannel { get; set; }

    public string? TransactionReference { get; set; }

    public DateTime? DisbursedAt { get; set; }
}