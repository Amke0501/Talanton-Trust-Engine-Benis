using Talanton.Api.Models;

namespace Talanton.Api.Repositories.Interfaces;

public interface IApplicantRepository
{
    Task<List<Applicant>> GetAllApplicantsAsync(CancellationToken cancellationToken = default);

    Task<Applicant?> GetApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Applicant> CreateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default);

    Task<Applicant?> UpdateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default);

    Task<bool> DeleteApplicantAsync(Guid id, CancellationToken cancellationToken = default);
}
