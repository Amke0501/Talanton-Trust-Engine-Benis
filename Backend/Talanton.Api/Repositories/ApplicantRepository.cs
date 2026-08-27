using Microsoft.EntityFrameworkCore;
using Talanton.Api.Data;
using Talanton.Api.Models;
using Talanton.Api.Repositories.Interfaces;

namespace Talanton.Api.Repositories;

public class ApplicantRepository : IApplicantRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Applicant>> GetAllApplicantsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Applicants
            .AsNoTracking()
            .OrderBy(a => a.DisplayName)
            .ToListAsync(cancellationToken);
    }

    public async Task<Applicant?> GetApplicantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Applicants
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Applicant> CreateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default)
    {
        _context.Applicants.Add(applicant);
        await _context.SaveChangesAsync(cancellationToken);
        return applicant;
    }

    public async Task<Applicant?> UpdateApplicantAsync(Applicant applicant, CancellationToken cancellationToken = default)
    {
        _context.Applicants.Update(applicant);
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        return affectedRows > 0 ? applicant : null;
    }

    public async Task<bool> DeleteApplicantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var applicant = await _context.Applicants.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (applicant is null)
        {
            return false;
        }

        _context.Applicants.Remove(applicant);
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows > 0;
    }
}
