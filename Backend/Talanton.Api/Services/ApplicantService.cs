using Talanton.Api.DTOs;
using Talanton.Api.Models;
using Talanton.Api.Repositories.Interfaces;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Services;

public class ApplicantService : IApplicantService
{
    private readonly IApplicantRepository _applicantRepository;

    public ApplicantService(IApplicantRepository applicantRepository)
    {
        _applicantRepository = applicantRepository;
    }

    public async Task<List<ApplicantDto>> GetAllApplicantsAsync(CancellationToken cancellationToken = default)
    {
        var applicants = await _applicantRepository.GetAllApplicantsAsync(cancellationToken);
        return applicants.Select(MapToDto).ToList();
    }

    public async Task<ApplicantDto?> GetApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        var applicant = await _applicantRepository.GetApplicantByIdAsync(id, cancellationToken);
        return applicant is null ? null : MapToDto(applicant);
    }

    public async Task<ApplicantDto> CreateApplicantAsync(CreateApplicantDto dto, CancellationToken cancellationToken = default)
    {
        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.ApplicantType))
        {
            throw new ArgumentException("Applicant type is required.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.DisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(dto));
        }

        if (dto.SaccoId == Guid.Empty)
        {
            throw new ArgumentException("SaccoId is required.", nameof(dto));
        }

        var applicant = new Applicant
        {
            Id = Guid.NewGuid(),
            ApplicantType = dto.ApplicantType.Trim(),
            DisplayName = dto.DisplayName.Trim(),
            IsActive = dto.IsActive,
            SaccoId = dto.SaccoId,
            ApplicantUserId = dto.ApplicantUserId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _applicantRepository.CreateApplicantAsync(applicant, cancellationToken);
        return MapToDto(created);
    }

    public async Task<ApplicantDto?> UpdateApplicantAsync(Guid id, UpdateApplicantDto dto, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        if (dto is null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        var existingApplicant = await _applicantRepository.GetApplicantByIdAsync(id, cancellationToken);
        if (existingApplicant is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(dto.ApplicantType))
        {
            existingApplicant.ApplicantType = dto.ApplicantType.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.DisplayName))
        {
            existingApplicant.DisplayName = dto.DisplayName.Trim();
        }

        if (dto.IsActive.HasValue)
        {
            existingApplicant.IsActive = dto.IsActive.Value;
        }

        if (dto.SaccoId.HasValue && dto.SaccoId.Value != Guid.Empty)
        {
            existingApplicant.SaccoId = dto.SaccoId.Value;
        }

        if (dto.ApplicantUserId.HasValue)
        {
            existingApplicant.ApplicantUserId = dto.ApplicantUserId.Value == Guid.Empty ? null : dto.ApplicantUserId.Value;
        }

        var updated = await _applicantRepository.UpdateApplicantAsync(existingApplicant, cancellationToken);
        return updated is null ? null : MapToDto(updated);
    }

    public async Task<bool> DeleteApplicantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        return await _applicantRepository.DeleteApplicantAsync(id, cancellationToken);
    }

    private static ApplicantDto MapToDto(Applicant applicant)
    {
        return new ApplicantDto
        {
            Id = applicant.Id,
            ApplicantType = applicant.ApplicantType,
            DisplayName = applicant.DisplayName,
            IsActive = applicant.IsActive,
            SaccoId = applicant.SaccoId,
            ApplicantUserId = applicant.ApplicantUserId,
            CreatedAt = applicant.CreatedAt
        };
    }
}
