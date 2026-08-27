namespace Talanton.Api.Models;

public class ApplicantIndividualProfile
{
    public Guid ApplicantId { get; set; }

    public Applicant Applicant { get; set; } = null!;

    public DateTime? DateOfBirth { get; set; }

    public string? NationalIdNumber { get; set; }

    public string? EmploymentType { get; set; }

    public decimal? MonthlyIncome { get; set; }

    public string? Address { get; set; }
}