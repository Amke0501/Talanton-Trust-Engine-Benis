namespace Talanton.Api.DTOs;

public class CreateApplicantDto
{
    public string ApplicantType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Guid SaccoId { get; set; }

    public Guid? ApplicantUserId { get; set; }
}
