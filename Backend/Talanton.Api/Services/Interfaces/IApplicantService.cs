using Talanton.Api.DTOs;

namespace Talanton.Api.Services.Interfaces;

public interface IApplicantService
{
    Task<List<ApplicantDto>> GetAllApplicantsAsync(CancellationToken cancellationToken = default);

    Task<ApplicantDto?> GetApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApplicantDto> CreateApplicantAsync(CreateApplicantDto dto, CancellationToken cancellationToken = default);

    Task<ApplicantDto?> UpdateApplicantAsync(Guid id, UpdateApplicantDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteApplicantAsync(Guid id, CancellationToken cancellationToken = default);
}
