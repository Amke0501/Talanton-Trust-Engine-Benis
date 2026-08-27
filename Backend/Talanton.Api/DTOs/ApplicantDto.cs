namespace Talanton.Api.DTOs;

public class ApplicantDto
{
    public Guid Id { get; set; }

    public string ApplicantType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Guid SaccoId { get; set; }

    public Guid? ApplicantUserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
