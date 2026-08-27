namespace Talanton.Api.Models;

public class Applicant
{
    public Guid Id { get; set; }

    public string ApplicantType { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public Guid SaccoId { get; set; }

    public Sacco Sacco { get; set; } = null!;

    public Guid? ApplicantUserId { get; set; }

    public User? ApplicantUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicantIndividualProfile? IndividualProfile { get; set; }

    public ApplicantCooperativeProfile? CooperativeProfile { get; set; }
}