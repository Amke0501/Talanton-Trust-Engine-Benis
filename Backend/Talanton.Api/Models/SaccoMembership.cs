namespace Talanton.Api.Models;

public class SaccoMembership
{
    public Guid Id { get; set; }

    public Guid ApplicantId { get; set; }

    public Applicant Applicant { get; set; } = null!;

    public Guid SaccoId { get; set; }

    public Sacco Sacco { get; set; } = null!;

    public string MembershipNumber { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }

    public string Status { get; set; } = "Active";

    public bool IsPrimaryMember { get; set; }
}