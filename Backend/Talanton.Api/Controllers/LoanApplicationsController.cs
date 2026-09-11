using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Talanton.Api.DTOs;
using Talanton.Api.Services.Interfaces;

namespace Talanton.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoanApplicationsController : ControllerBase
{
    private readonly ILoanApplicationService _loanService;
    private readonly Services.LiquidityService _liquidity;
    private readonly Services.AuditService _audit;
    private readonly Data.ApplicationDbContext _context;
    private readonly Services.CurrentUserService _currentUser;

    public LoanApplicationsController(
        ILoanApplicationService loanService,
        Services.LiquidityService liquidity,
        Services.AuditService audit,
        Data.ApplicationDbContext context,
        Services.CurrentUserService currentUser)
    {
        _loanService = loanService;
        _liquidity = liquidity;
        _audit = audit;
        _context = context;
        _currentUser = currentUser;
    }

    /// <summary>
    /// The committee seat the caller actually holds, or a refusal.
    ///
    /// Every decision that moves money used to read the seat out of the request body, which meant
    /// the caller chose their own authority: a request claiming "Treasurer" was treated as the
    /// Treasurer. The seat now comes from the SACCO's records, keyed to a login Supabase has
    /// verified, and the body is ignored.
    /// </summary>
    private async Task<(string? Seat, ActionResult? Refusal)> RequireCommitteeSeatAsync(
        CancellationToken cancellationToken)
    {
        var user = await _currentUser.ResolveAsync(cancellationToken);

        if (user is null)
        {
            return (null, StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = "This account is not registered with the SACCO.",
            }));
        }

        if (!user.IsCommittee || string.IsNullOrWhiteSpace(user.CommitteeSeat))
        {
            return (null, StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = $"{user.FullName} does not hold a committee seat, so cannot vote or release funds.",
            }));
        }

        return (user.CommitteeSeat, null);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LoanApplicationDto>>> GetApplications(CancellationToken cancellationToken)
    {
        var apps = await _loanService.GetAllLoanApplicationsAsync(cancellationToken);
        return Ok(apps);
    }

    [HttpGet("{reference}")]
    public async Task<ActionResult<LoanApplicationDto>> GetApplicationByRef(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        return app == null ? NotFound() : Ok(app);
    }

    [HttpPost]
    public async Task<ActionResult<LoanApplicationDto>> CreateApplication([FromBody] CreateLoanApplicationDto dto, CancellationToken cancellationToken)
    {
        // A member with unpaid invoices cannot open a new application until they are settled.
        var invoiceHold = await GetInvoiceHoldAsync(dto.MemberId, cancellationToken);
        if (invoiceHold.IsHeld)
        {
            return StatusCode(StatusCodes.Status409Conflict, new
            {
                message = $"A new application cannot be opened: {invoiceHold.Reason}",
                invoiceHold.OutstandingBalance,
                invoiceHold.InvoiceNumbers,
            });
        }

        var created = await _loanService.CreateLoanApplicationAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetApplicationByRef), new { reference = created.Reference }, created);
    }

    [HttpPut("{reference}/underwrite")]
    public async Task<ActionResult<LoanApplicationDto>> UpdateUnderwriting(string reference, [FromBody] UpdateUnderwritingOverrideDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.UpdateUnderwritingAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/counter-offer")]
    public async Task<ActionResult<LoanApplicationDto>> RespondToCounterOffer(string reference, [FromBody] CounterOfferDecisionDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.RespondToCounterOfferAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    /// <summary>
    /// An applicant's second attempt after declining a revised offer, on the strength of at least
    /// two additional guarantors. Returns the file to underwriting, never straight to committee.
    /// </summary>
    [HttpPost("{reference}/resubmit-with-guarantors")]
    public async Task<ActionResult<LoanApplicationDto>> ResubmitWithGuarantors(
        string reference, [FromBody] ResubmitWithGuarantorsDto dto, CancellationToken cancellationToken)
    {
        var updated = await _loanService.ResubmitWithGuarantorsAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/guarantor")]
    public async Task<ActionResult<LoanApplicationDto>> AddGuarantor(string reference, [FromBody] GuarantorDto guarantor, CancellationToken cancellationToken)
    {
        var updated = await _loanService.AddGuarantorAsync(reference, guarantor, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/vote")]
    public async Task<ActionResult<LoanApplicationDto>> CastVote(string reference, [FromBody] CastCommitteeVoteDto voteDto, CancellationToken cancellationToken)
    {
        var (seat, refusal) = await RequireCommitteeSeatAsync(cancellationToken);
        if (refusal is not null) return refusal;

        // A member casts their own vote and only their own. The seat in the body is discarded.
        voteDto.MemberRole = seat!;

        var updated = await _loanService.CastVoteAsync(reference, voteDto, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/route")]
    public async Task<ActionResult<LoanApplicationDto>> RouteStage(string reference, [FromQuery] string stage, CancellationToken cancellationToken)
    {
        var updated = await _loanService.RouteStageAsync(reference, stage, cancellationToken);
        return updated == null ? NotFound() : Ok(updated);
    }

    [HttpPost("{reference}/quorum-check")]
    public async Task<ActionResult<QuorumCheckResponseDto>> CheckQuorum(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        var result = Services.QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);

        return Ok(new QuorumCheckResponseDto
        {
            IsQuorumPassed = result.IsQuorumPassed,
            Reason = result.Reason,
            IsBigLoan = result.IsBigLoan,
            RequiredApprovals = result.RequiredApprovals,
            ApprovalCount = result.ApprovalCount,
            HasChairpersonVeto = result.HasChairpersonVeto,
            HasRequiredMembers = result.HasRequiredMembers
        });
    }

    [HttpPost("{reference}/disburse")]
    public async Task<ActionResult<DisbursementAuthorizationResponseDto>> DisburseFunds(
        string reference,
        [FromBody] DisbursementAuthorizationDto authDto,
        CancellationToken cancellationToken)
    {
        var (seat, refusal) = await RequireCommitteeSeatAsync(cancellationToken);
        if (refusal is not null) return refusal;

        // The releasing officer is whoever signed in, never whoever the request claims to be.
        authDto.RequestorRole = seat!;

        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        // Step 1: Verify quorum has passed
        var quorumResult = Services.QuorumEvaluationService.EvaluateQuorum(app.CommitteeVotes, app.Principal);
        if (!quorumResult.IsQuorumPassed)
        {
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — quorum not met. {quorumResult.Reason}",
                cancellationToken: cancellationToken);

            return BadRequest(new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = $"Cannot disburse: Quorum requirement not met. {quorumResult.Reason}"
            });
        }

        // Step 2: Check disbursement authorization
        var authResult = Services.DisbursementAuthorizationService.EvaluateDisbursementAuthority(
            app.Principal,
            authDto.RequestorRole,
            !string.IsNullOrEmpty(authDto.ChairpersonSignature),
            !string.IsNullOrEmpty(authDto.SecretarySignature)
        );

        if (!authResult.IsAuthorized)
        {
            // Forbid() challenges the default authentication scheme; none is registered, so it
            // cannot produce a clean 403 here. Return the status directly with the reason, so the
            // caller can show the committee why the release was refused.
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — {authResult.Reason}",
                cancellationToken: cancellationToken);

            return StatusCode(StatusCodes.Status403Forbidden, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = authResult.Reason
            });
        }

        // Step 2a: a member with unpaid invoices does not get funds released.
        var invoiceHold = await GetInvoiceHoldAsync(app.MemberId, cancellationToken);
        if (invoiceHold.IsHeld)
        {
            await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                actorName: authDto.RequestorRole,
                after: $"Refused — {invoiceHold.Reason}",
                cancellationToken: cancellationToken);

            return StatusCode(StatusCodes.Status409Conflict, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = $"Disbursement is on hold: {invoiceHold.Reason}"
            });
        }

        // Step 2b: the SACCO must still be holding enough cash, and this file must be the one the
        // cash reaches. Authority to release is not the same as having the money, and having the
        // money is not the same as it being this applicant's turn.
        var gate = await EvaluateCashGateAsync(reference, cancellationToken);
        if (!gate.CanRelease)
        {
            var authorization = Services.EmergencyOverrideService.Evaluate(
                authDto.EmergencyFirstSeat, authDto.EmergencySecondSeat, authDto.EmergencyReason);

            // One of the two keys must belong to the officer making the request. Without this a
            // single person could name two colleagues and release funds on their own — the exact
            // thing two keys exist to prevent.
            //
            // This is not yet a full dual-key: the second officer's approval is asserted by the
            // first rather than given in their own session. Recording it against both names in the
            // audit trail is the current control; a true second approval needs its own sign-in.
            if (authorization.IsAuthorized
                && !seat!.Equals(authorization.FirstSeat, StringComparison.OrdinalIgnoreCase)
                && !seat!.Equals(authorization.SecondSeat, StringComparison.OrdinalIgnoreCase))
            {
                authorization = new Services.OverrideAuthorizationResult
                {
                    IsAuthorized = false,
                    Explanation = $"You are signed in as the {seat}, which is neither of the two " +
                                  "signatures given. One of them must be your own.",
                };
            }

            var overrideRequested =
                !string.IsNullOrWhiteSpace(authDto.EmergencyFirstSeat) ||
                !string.IsNullOrWhiteSpace(authDto.EmergencySecondSeat) ||
                !string.IsNullOrWhiteSpace(authDto.EmergencyReason);

            if (!authorization.IsAuthorized)
            {
                // Hold the file rather than dropping it: it keeps its place in the queue and is
                // shown as "Deferred: awaiting liquidity" until cash recovers.
                await _loanService.DeferForLiquidityAsync(reference, gate.Reason, cancellationToken);

                await _audit.RecordAsync(Services.AuditActions.ReleaseRefused, "LoanApplication", reference,
                    actorName: authDto.RequestorRole,
                    after: $"Refused — {gate.Reason}" +
                           (overrideRequested ? $" Emergency release also refused: {authorization.Explanation}" : string.Empty),
                    cancellationToken: cancellationToken);

                return StatusCode(StatusCodes.Status409Conflict, new DisbursementAuthorizationResponseDto
                {
                    IsAuthorized = false,
                    IsDeferredForLiquidity = true,
                    Reason = gate.Reason +
                             (overrideRequested
                                 ? $" The emergency release was also refused: {authorization.Explanation}"
                                 : $" Two of {string.Join(", ", Services.EmergencyOverrideService.KeyHolderSeats)} may " +
                                   "jointly authorise an emergency release, with a written reason."),
                    LiquidityStatus = DescribeLiquidity(gate.Status),
                });
            }

            await _loanService.RecordEmergencyOverrideAsync(
                reference, authorization, gate.Reason, cancellationToken);
        }

        // Step 3: Execute disbursement (route to disbursed stage)
        var updated = await _loanService.RouteStageAsync(reference, "disbursement", cancellationToken);

        if (updated == null || updated.Stage != "disbursed")
        {
            return StatusCode(500, new DisbursementAuthorizationResponseDto
            {
                IsAuthorized = false,
                Reason = "Disbursement routing failed."
            });
        }

        // Step 4: Return success response
        return Ok(new DisbursementAuthorizationResponseDto
        {
            IsAuthorized = true,
            Reason = $"Loan {reference} successfully disbursed.",
            UpdatedApplication = updated,
            DisbursementAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Money received against a disbursed loan. Settling it in full releases the guarantors'
    /// pledged shares back to them.
    /// </summary>
    [HttpPost("{reference}/repayment")]
    public async Task<ActionResult<LoanApplicationDto>> RecordRepayment(
        string reference, [FromBody] RecordRepaymentDto dto, CancellationToken cancellationToken)
    {
        var (seat, refusal) = await RequireCommitteeSeatAsync(cancellationToken);
        if (refusal is not null) return refusal;

        if (dto.Amount <= 0)
        {
            return BadRequest(new { message = "A repayment amount greater than zero is required." });
        }

        dto.RecordedByRole = seat!;

        var updated = await _loanService.RecordRepaymentAsync(reference, dto, cancellationToken);
        return updated == null ? NotFound(new { message = $"Loan application {reference} not found." }) : Ok(updated);
    }

    /// <summary>
    /// The cash gate: is the SACCO holding enough, and has the queue reached this file?
    ///
    /// Two distinct refusals share one answer here, because to a caller they mean the same thing
    /// — the money is not available for this file right now — but the reason given to the board
    /// differs, and the board needs the difference to know whether to wait or to escalate.
    /// </summary>
    private async Task<CashGateResult> EvaluateCashGateAsync(string reference, CancellationToken cancellationToken)
    {
        var status = await _liquidity.GetStatusForGateAsync(cancellationToken);

        if (!status.IsAvailable)
        {
            return new CashGateResult
            {
                CanRelease = false,
                Status = status,
                Reason = "The SACCO's cash position could not be read, so the release was refused. " +
                         "Funds are never released against an unverified cash position.",
            };
        }

        if (status.IsLocked)
        {
            return new CashGateResult
            {
                CanRelease = false,
                Status = status,
                Reason = $"SYSTEM LOCK: insufficient liquidity buffer. The ratio is " +
                         $"{status.CurrentLiquidityRatio:0.00} against a {Services.LiquidityService.MinimumSafeRatio:0.00} " +
                         $"minimum — cash on hand {status.TotalLiquidCash:N0} against {status.TotalPendingLoans:N0} " +
                         $"committed, a shortfall of {status.Deficit:N0}.",
            };
        }

        var position = Services.LiquidityService.EvaluateQueuePosition(status, reference);
        if (!position.IsReleasable)
        {
            var entry = position.Entry!;
            return new CashGateResult
            {
                CanRelease = false,
                Status = status,
                Reason = $"Deferred: awaiting liquidity. This file is number {entry.QueuePosition} in the release " +
                         $"queue; releasing it would take cumulative disbursement to {entry.CumulativeDemand:N0}, " +
                         $"above the safe cap of {status.MaxSafeDisbursementCap:N0}. Files committed earlier are " +
                         "released first.",
            };
        }

        return new CashGateResult { CanRelease = true, Status = status, Reason = string.Empty };
    }

    private static object DescribeLiquidity(Services.LiquidityStatus status) => new
    {
        status.TotalLiquidCash,
        status.TotalPendingLoans,
        status.CurrentLiquidityRatio,
        status.IsLocked,
        status.Deficit,
        status.MaxSafeDisbursementCap,
        status.IsAvailable,
        MinimumSafeRatio = Services.LiquidityService.MinimumSafeRatio,
    };

    private sealed class CashGateResult
    {
        public bool CanRelease { get; init; }
        public string Reason { get; init; } = string.Empty;
        public Services.LiquidityStatus Status { get; init; } = new();
    }

    /// <summary>
    /// Whether this member is blocked by unpaid invoices. Consulted before releasing funds and
    /// before accepting a new application, per the founder's rule.
    /// </summary>
    private async Task<Services.InvoiceHoldResult> GetInvoiceHoldAsync(string memberId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(memberId))
        {
            return new Services.InvoiceHoldResult { IsHeld = false, Reason = "No member identifier on the file." };
        }

        var invoices = await _context.Invoices
            .Where(i => i.MemberId == memberId)
            .ToListAsync(cancellationToken);

        return Services.InvoiceHoldService.Evaluate(invoices);
    }

    /// <summary>
    /// Whether this member's own savings support the amount requested — the per-member check,
    /// distinct from the SACCO-wide liquidity position.
    /// </summary>
    [HttpGet("{reference}/member-savings-check")]
    public async Task<ActionResult<object>> CheckMemberSavings(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        // The SACCO's own record of what this member has saved, rather than the figure entered on
        // the application. Falls back to the declared figure when no membership record exists, and
        // says so, so the check is never silently measured against the applicant's own claim.
        var membership = await _context.SaccoMemberships
            .FirstOrDefaultAsync(m => m.MembershipNumber == app.MemberId, cancellationToken);

        var recorded = membership?.SavingsBalance ?? app.SavingsBalance;

        var result = Services.MemberSavingsService.Evaluate(
            app.Principal, recorded, app.Multiplier, app.SavingsBalance);

        return Ok(new
        {
            result.IsWithinCap,
            result.Reason,
            result.RecordedSavings,
            result.Multiplier,
            result.MaximumBorrowable,
            result.RequestedPrincipal,
            result.ExcessAmount,
            result.DeclaredSavings,
            result.DeclaredSavingsMismatch,
            result.DeclaredSavingsDifference,
            SavingsSource = membership is null ? "application (no membership record found)" : "SACCO membership record",
        });
    }

    [HttpGet("{reference}/guarantor-coverage")]
    public async Task<ActionResult<object>> CheckGuarantorCoverage(string reference, CancellationToken cancellationToken)
    {
        var app = await _loanService.GetLoanApplicationByRefAsync(reference, cancellationToken);
        if (app == null)
            return NotFound(new { message = $"Loan application {reference} not found." });

        var coverage = Services.GuarantorShareLockingService.EvaluateGuarantorCoverage(
            app.Guarantors,
            app.Principal,
            app.SavingsBalance
        );

        return Ok(new
        {
            coverage.IsCovered,
            coverage.LoanGap,
            coverage.TotalPledgedShares,
            coverage.TotalAvailableShares,
            coverage.Deficit,
            coverage.Reason,
            Guarantors = app.Guarantors.Select(g => new
            {
                g.Id,
                g.Name,
                g.MemberId,
                g.PledgedShares,
                g.AvailableShares,
                IsCapacitySufficient = Services.GuarantorShareLockingService.ValidateGuarantorCapacity(g, g.PledgedShares).IsValid
            })
        });
    }
}
