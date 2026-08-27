namespace Talanton.Api.Models;

public class ApplicationDocument
{
    public Guid Id { get; set; }

    public Guid LoanApplicationId { get; set; }

    public LoanApplication LoanApplication { get; set; } = null!;

    public string DocumentName { get; set; } = string.Empty;

    public string StorageUri { get; set; } = string.Empty;

    public string FileHash { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public Guid UploadedByUserId { get; set; }

    public User UploadedByUser { get; set; } = null!;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}