namespace Talanton.Api.Models;

public class DocumentVerification
{
    public Guid Id { get; set; }

    public Guid ApplicationDocumentId { get; set; }

    public ApplicationDocument ApplicationDocument { get; set; } = null!;

    public string VerificationStatus { get; set; } = "Pending";

    public Guid? VerifiedByUserId { get; set; }

    public User? VerifiedByUser { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public string? Notes { get; set; }

    public string? RejectionReason { get; set; }
}