namespace Talanton.Api.Models;

public class ApplicationRequiredDocument
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public Guid RequiredDocumentTypeId { get; set; }

    public RequiredDocumentType RequiredDocumentType { get; set; } = null!;

    public bool IsRequired { get; set; } = true;

    public bool IsFulfilled { get; set; } = false;

    public DateTime? DueDate { get; set; }
}