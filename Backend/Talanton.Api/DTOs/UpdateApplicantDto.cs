namespace Talanton.Api.DTOs;

public class UpdateApplicantDto
{
    public string? ApplicantType { get; set; }

    public string? DisplayName { get; set; }

    public bool? IsActive { get; set; }

    public Guid? SaccoId { get; set; }

    public Guid? ApplicantUserId { get; set; }
}
