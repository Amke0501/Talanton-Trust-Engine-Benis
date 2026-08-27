namespace Talanton.Api.Models;

public class ApplicantCooperativeProfile
{
    public Guid ApplicantId { get; set; }

    public Applicant Applicant { get; set; } = null!;

    public Guid CooperativeId { get; set; }

    public Cooperative Cooperative { get; set; } = null!;

    public string? AuthorizedSignatoryName { get; set; }

    public string? AuthorizedSignatoryPhone { get; set; }

    public int MembershipCount { get; set; }
}