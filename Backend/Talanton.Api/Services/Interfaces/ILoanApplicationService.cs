using Talanton.Api.DTOs;

namespace Talanton.Api.Services.Interfaces;

public interface ILoanApplicationService
{
    Task<IEnumerable<LoanApplicationDto>> GetAllLoanApplicationsAsync(CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> GetLoanApplicationByRefAsync(string reference, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto> CreateLoanApplicationAsync(CreateLoanApplicationDto dto, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> UpdateUnderwritingAsync(string reference, UpdateUnderwritingOverrideDto dto, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> RespondToCounterOfferAsync(string reference, CounterOfferDecisionDto dto, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> AddGuarantorAsync(string reference, GuarantorDto guarantor, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> CastVoteAsync(string reference, CastCommitteeVoteDto voteDto, CancellationToken cancellationToken = default);
    Task<LoanApplicationDto?> RouteStageAsync(string reference, string targetStage, CancellationToken cancellationToken = default);
    Task<IEnumerable<CreditPassportMemberDto>> GetCreditPassportMembersAsync(CancellationToken cancellationToken = default);
}
