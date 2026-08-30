'use client'

import { useState } from 'react'
import {
  ArrowLeft,
  CheckCircle2,
  Lock,
  Minus,
  ShieldCheck,
  ThumbsDown,
  ThumbsUp,
  UserCheck,
  Wallet,
  XCircle,
  DollarSign,
  Calendar,
  Send
} from 'lucide-react'
import {
  formatUGX,
  SEED_PORTFOLIO_LOANS,
  type Application,
  type BoardMemberVote,
  type PortfolioLoan,
} from '@/lib/talenton-data'
import { Card, CardBody } from '@/components/talenton/primitives'
import { disburseLoan } from '@/lib/api-service'

export function CommitteeDashboardView({
  application,
  onCastVote,
  onBack,
}: {
  application: Application
  onCastVote: (memberRole: string, vote: 'APPROVE' | 'REJECT' | 'ABSTAIN') => void
  onBack?: () => void
}) {
  // Board Member Voting State
  const [boardVotes, setBoardVotes] = useState<BoardMemberVote[]>(
    application.committeeVotes || [
      { id: 'v1', name: 'Chairman', role: 'Chairperson', vote: 'APPROVE' },
      { id: 'v2', name: 'Sec. General', role: 'Risk Head', vote: 'APPROVE' },
      { id: 'v3', name: 'Mrs. Nabukenya', role: 'Credit Officer', vote: 'APPROVE' },
      { id: 'v4', name: 'Dr. Ochieng', role: 'Treasurer', vote: 'ABSTAIN' },
      { id: 'v5', name: 'Eng. Museveni', role: 'Board Member', vote: 'ABSTAIN' },
    ]
  )

  // Portfolio State
  const [portfolioLoans, setPortfolioLoans] = useState<PortfolioLoan[]>(SEED_PORTFOLIO_LOANS)
  const [portfolioFilter, setPortfolioFilter] = useState<'ALL' | 'PENDING' | 'APPROVED' | 'ACTIVE' | 'COMPLETED' | 'REJECTED'>('ALL')
  const [isDisbursing, setIsDisbursing] = useState(false)
  const [disbursedSuccess, setDisbursedSuccess] = useState(application.stage === 'disbursed')

  // ========== NEW: QUORUM LOGIC BASED ON LOAN SIZE ==========
  const BIG_LOAN_THRESHOLD = 5_000_000; // 5M UGX
  const isBigLoan = application.principal >= BIG_LOAN_THRESHOLD;
  const requiredApprovals = isBigLoan ? 3 : 1;

  // Count votes
  const approveCount = boardVotes.filter((v) => v.vote === 'APPROVE').length;
  const rejectCount = boardVotes.filter((v) => v.vote === 'REJECT').length;
  const abstainCount = boardVotes.filter((v) => v.vote === 'ABSTAIN' || !v.vote).length;

  // Check for Chairperson veto
  const chairpersonVeto = boardVotes.some(
    (v) => v.role === 'Chairperson' && v.vote === 'REJECT'
  );

  // For big loans, check if both Chairman and Treasurer approved
  const chairmanApproved = boardVotes.some(
    (v) => v.role === 'Chairperson' && v.vote === 'APPROVE'
  );
  const treasurerApproved = boardVotes.some(
    (v) => v.role === 'Treasurer' && v.vote === 'APPROVE'
  );
  const hasRequiredMembers = chairmanApproved && treasurerApproved;

  // Determine quorum status
  let isQuorumPassed = false;
  let quorumReason = '';

  if (chairpersonVeto) {
    isQuorumPassed = false;
    quorumReason = 'Chairperson Veto: Absolute rejection applied.';
  } else if (isBigLoan) {
    isQuorumPassed = approveCount >= requiredApprovals && hasRequiredMembers;
    if (approveCount < requiredApprovals) {
      quorumReason = `Big Loan: ${approveCount}/${requiredApprovals} approvals needed. Both Chairman and Treasurer must approve.`;
    } else if (!hasRequiredMembers) {
      quorumReason = 'Big Loan: Chairman and Treasurer approval required.';
    } else {
      quorumReason = `Big Loan Approved: ${approveCount}/${requiredApprovals} approvals with required members.`;
    }
  } else {
    isQuorumPassed = approveCount >= requiredApprovals;
    quorumReason = isQuorumPassed
      ? `Small Loan Approved: ${approveCount} approval received.`
      : `Small Loan: Requires 1 approval (currently ${approveCount}).`;
  }
  // ========== END: QUORUM LOGIC ==========

  function handleVoteClick(role: string, vote: 'APPROVE' | 'REJECT' | 'ABSTAIN') {
    const updated = boardVotes.map((b) => (b.role === role ? { ...b, vote } : b))
    setBoardVotes(updated)
    onCastVote(role, vote)
  }

  async function handleDisburseFunds() {
    setIsDisbursing(true)
    await disburseLoan(application.reference)
    setIsDisbursing(false)
    setDisbursedSuccess(true)
    
    // Update local portfolio
    setPortfolioLoans((prev) => [
      {
        reference: application.reference,
        borrowerName: `${application.fullName} (${application.memberId})`,
        borrowerMeta: `Authorized by Board Quorum (${approveCount}/5)`,
        type: application.applicantType === 'individual' ? 'BOSA' : 'SME',
        principal: application.principal,
        status: 'REPAYING',
        repaymentProgress: '0/12 paid',
        dueDate: 'Due: Next month',
        isLocked: true,
      },
      ...prev.filter(l => l.reference !== application.reference)
    ])
  }

  const filteredLoans = portfolioLoans.filter((l) => {
    if (portfolioFilter === 'ALL') return true
    if (portfolioFilter === 'PENDING') return l.status === 'PENDING'
    if (portfolioFilter === 'APPROVED') return l.status === 'APPROVED'
    if (portfolioFilter === 'ACTIVE') return l.status === 'REPAYING'
    if (portfolioFilter === 'COMPLETED') return l.status === 'COMPLETED'
    if (portfolioFilter === 'REJECTED') return l.status === 'REJECTED'
    return true
  })

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      
      {/* Top Bar with Back Button & Breadcrumbs */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-gray-200">
        <div className="flex items-center gap-3">
          {onBack && (
            <button
              onClick={onBack}
              className="p-2 rounded-full bg-white border border-gray-200 hover:bg-gray-100 text-[#103a27] transition-colors cursor-pointer"
              title="Back to applications list"
            >
              <ArrowLeft className="size-4" />
            </button>
          )}
          <div>
            <div className="flex items-center gap-2">
              <h2 className="text-xl font-serif font-bold text-[#103a27]">{application.fullName}</h2>
              <span className="font-mono text-xs bg-[#0d2a1c] text-[#a4cc44] px-2.5 py-0.5 rounded-full font-bold">
                {application.reference}
              </span>
            </div>
            <p className="text-xs text-gray-500 mt-0.5">
              Member ID: <strong className="text-gray-700">{application.memberId}</strong> &bull; Purpose: <strong className="text-gray-700">{application.purpose}</strong>
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          {disbursedSuccess ? (
            <span className="px-3.5 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-900 border border-emerald-300 flex items-center gap-1.5">
              <CheckCircle2 className="size-4 text-emerald-600" />
              Funds Disbursed & Active
            </span>
          ) : (
            <span className={`px-3 py-1 rounded-full text-xs font-bold ${
              isQuorumPassed 
                ? 'bg-emerald-100 text-emerald-800' 
                : chairpersonVeto
                ? 'bg-red-100 text-red-800'
                : 'bg-amber-100 text-amber-800'
            }`}>
              {isQuorumPassed 
                ? 'Quorum Passed ✓' 
                : chairpersonVeto
                ? 'Vetoed by Chairperson ✗'
                : isBigLoan
                ? `Big Loan: ${approveCount}/${requiredApprovals} + Chairman & Treasurer`
                : `Small Loan: ${approveCount}/${requiredApprovals} Approval Required`
              }
            </span>
          )}
        </div>
      </div>

      {/* SECTION 1: COMMITTEE AUTHORIZATION BOARD */}
      <div className="grid gap-6 lg:grid-cols-12">
        
        {/* Verified Underwriting Stats Freeze Card */}
        <div className="lg:col-span-4">
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-4">
              <div className="flex items-center gap-2 pb-3 border-b border-gray-100">
                <ShieldCheck className="size-5 text-[#103a27]" />
                <h3 className="font-serif text-sm font-bold text-[#103a27]">
                  Verified Underwriting Stats
                </h3>
              </div>

              <div className="space-y-3 text-xs">
                <div className="flex justify-between py-1.5 border-b border-gray-50">
                  <span className="text-gray-500 font-medium">Applicant Loan</span>
                  <span className="font-mono font-bold text-[#103a27] text-sm">{formatUGX(application.principal)}</span>
                </div>
                <div className="flex justify-between py-1.5 border-b border-gray-50">
                  <span className="text-gray-500 font-medium">DTI Percentage</span>
                  <span className="font-bold text-[#103a27]">{application.dtiNetRatio?.toFixed(1) || '82.0'}%</span>
                </div>
                <div className="flex justify-between py-1.5 border-b border-gray-50">
                  <span className="text-gray-500 font-medium">Savings Multiplier</span>
                  <span className="font-bold text-[#103a27]">{application.multiplier?.toFixed(2) || '3.75'}x</span>
                </div>
                <div className="flex justify-between items-center py-1.5 border-b border-gray-50">
                  <span className="text-gray-500 font-medium">Audit Check Verdict</span>
                  <span className={`rounded-full px-2.5 py-0.5 text-[0.65rem] font-bold ${
                    application.verdict === 'APPROVED'
                      ? 'bg-emerald-100 text-emerald-800'
                      : 'bg-rose-100 text-rose-800'
                  }`}>
                    {application.verdict || 'DECLINED'}
                  </span>
                </div>
              </div>

              <div className="rounded-xl bg-amber-50 p-3.5 border border-amber-200">
                <p className="text-[0.65rem] text-amber-900 leading-relaxed font-medium">
                  <strong>Note for Board:</strong> These stats are frozen snapshots from the underwriting phase. Overrides are restricted to appraisal officers.
                </p>
              </div>
            </CardBody>
          </Card>
        </div>

        {/* Committee Board Sign-off Card (Who Approved What) */}
        <div className="lg:col-span-8">
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-5">
              <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                <div>
                  <h3 className="font-serif text-sm font-bold text-[#103a27]">
                    Committee Board Sign-off
                  </h3>
                  <p className="text-xs text-gray-500 mt-0.5">
                    Review underwriter recommendation, cast individual votes, and sign off.
                  </p>
                </div>
                <span className="rounded-full bg-[#103a27]/10 px-3 py-1 text-xs font-bold text-[#103a27]">
                  Board Quorum: 4 / 5
                </span>
              </div>

              {/* Voting Cards Grid */}
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
                {boardVotes.map((mem) => {
                  const isApprove = mem.vote === 'APPROVE'
                  const isReject = mem.vote === 'REJECT'
                  const isAbstain = mem.vote === 'ABSTAIN' || !mem.vote
                  return (
                    <div
                      key={mem.role}
                      className="flex flex-col justify-between rounded-2xl border border-gray-100 bg-[#f4f5f4] p-3 text-center space-y-3"
                    >
                      <div>
                        <p className="text-xs font-bold text-[#103a27]">{mem.name}</p>
                        <p className="text-[0.65rem] text-gray-500 font-medium">{mem.role}</p>
                      </div>

                      {/* Vote Buttons */}
                      <div className="flex items-center justify-center gap-1">
                        <button
                          type="button"
                          onClick={() => handleVoteClick(mem.role, 'APPROVE')}
                          title="Approve loan"
                          className={`size-7 rounded-full flex items-center justify-center transition-all cursor-pointer ${
                            isApprove ? 'bg-emerald-600 text-white shadow-md' : 'bg-white text-gray-400 hover:bg-emerald-50 hover:text-emerald-700'
                          }`}
                        >
                          <ThumbsUp className="size-3.5" />
                        </button>
                        <button
                          type="button"
                          onClick={() => handleVoteClick(mem.role, 'REJECT')}
                          title="Reject loan"
                          className={`size-7 rounded-full flex items-center justify-center transition-all cursor-pointer ${
                            isReject ? 'bg-rose-600 text-white shadow-md' : 'bg-white text-gray-400 hover:bg-rose-50 hover:text-rose-700'
                          }`}
                        >
                          <ThumbsDown className="size-3.5" />
                        </button>
                        <button
                          type="button"
                          onClick={() => handleVoteClick(mem.role, 'ABSTAIN')}
                          title="Abstain"
                          className={`size-7 rounded-full flex items-center justify-center transition-all cursor-pointer ${
                            isAbstain ? 'bg-slate-700 text-white shadow-md' : 'bg-white text-gray-400 hover:bg-slate-100'
                          }`}
                        >
                          <Minus className="size-3.5" />
                        </button>
                      </div>

                      <span
                        className={`text-[0.65rem] font-bold uppercase tracking-wider ${
                          isApprove ? 'text-emerald-700' : isReject ? 'text-rose-700' : 'text-slate-500'
                        }`}
                      >
                        {mem.vote || 'ABSTAINED'}
                      </span>
                    </div>
                  )
                })}
              </div>

              {/* Committee Quorum Outcome Tracker & Disburse Button */}
              <div className="flex flex-wrap items-center justify-between gap-4 rounded-2xl bg-[#0d2a1c] p-4 text-white">
                <div>
                  <p className="text-xs font-bold text-white">Committee Quorum Outcome Tracker</p>
                  <p className="text-[0.7rem] text-white/70 mt-0.5">
                    {isBigLoan 
                      ? `Big Loan (≥5M): Requires ${requiredApprovals}/5 approvals + Chairman & Treasurer approval`
                      : `Small Loan (<5M): Requires ${requiredApprovals}/5 approval`
                    } • Currently {approveCount} Approvals, {rejectCount} Rejections, {abstainCount} Abstentions
                  </p>
                  {chairpersonVeto && (
                    <p className="text-[0.7rem] text-red-300 mt-1 font-semibold">
                      ⚠️ CHAIRPERSON VETO: Absolute rejection applied.
                    </p>
                  )}
                  {isBigLoan && !hasRequiredMembers && (
                    <p className="text-[0.7rem] text-yellow-300 mt-1 font-semibold">
                      ⚠️ MISSING REQUIRED APPROVALS: Chairman {chairmanApproved ? '✓' : '✗'} & Treasurer {treasurerApproved ? '✓' : '✗'} must approve big loans.
                    </p>
                  )}
                </div>

                <div className="flex items-center gap-3">
                  <span
                    className={`rounded-xl px-4 py-2 text-xs font-bold shadow-md ${
                      isQuorumPassed ? 'bg-[#a4cc44] text-[#0d2a1c]' : 'bg-rose-600 text-white'
                    }`}
                  >
                    {isQuorumPassed ? 'QUORUM PASSED ✓' : chairpersonVeto ? 'VETO APPLIED ✗' : 'QUORUM BLOCKED'}
                  </span>

                  {isQuorumPassed && !disbursedSuccess && (
                    <button
                      type="button"
                      onClick={handleDisburseFunds}
                      disabled={isDisbursing}
                      className="px-5 py-2 rounded-xl bg-emerald-500 hover:bg-emerald-400 text-[#0d2a1c] font-bold text-xs shadow-lg transition-all flex items-center gap-1.5 cursor-pointer"
                    >
                      <DollarSign className="size-4" />
                      {isDisbursing ? 'Releasing...' : 'Disburse Funds Now'}
                    </button>
                  )}
                  {(chairpersonVeto || !isQuorumPassed) && !disbursedSuccess && (
                    <button
                      disabled
                      className="px-5 py-2 rounded-xl bg-gray-400 text-white font-bold text-xs shadow-lg opacity-50 cursor-not-allowed flex items-center gap-1.5"
                    >
                      <DollarSign className="size-4" />
                      Cannot Disburse
                    </button>
                  )}
                </div>
              </div>
            </CardBody>
          </Card>
        </div>
      </div>

      {/* SECTION 2: LOAN PORTFOLIO & DISBURSEMENT TRACKER */}
      <Card className="border-none shadow-sm rounded-2xl bg-white">
        <CardBody className="p-6 space-y-5">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between pb-3 border-b border-gray-100">
            <div>
              <h3 className="font-serif text-lg font-bold text-[#103a27]">
                Disbursed Loan Portfolio & Repayment Tracker
              </h3>
              <p className="text-xs text-gray-500 mt-0.5">
                Track released funds, payment timestamps, and active member repayment performance.
              </p>
            </div>

            <div className="flex items-center gap-3">
              <div className="rounded-xl bg-[#0d2a1c] px-3.5 py-2 text-white text-xs">
                <span className="text-white/60 block text-[0.6rem]">ACTIVE PORTFOLIO</span>
                <span className="font-mono font-bold text-[#a4cc44]">UGX 89,000,000</span>
              </div>
            </div>
          </div>

          {/* Table */}
          <div className="overflow-x-auto rounded-xl border border-gray-100">
            <table className="w-full text-left text-xs">
              <thead className="bg-[#f4f5f4] font-bold uppercase tracking-wider text-gray-500 border-b border-gray-200">
                <tr>
                  <th className="p-3">FILE</th>
                  <th className="p-3">BORROWER</th>
                  <th className="p-3">TYPE</th>
                  <th className="p-3">PRINCIPAL</th>
                  <th className="p-3">REPAYMENT</th>
                  <th className="p-3">STATUS</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 font-medium">
                {filteredLoans.map((loan) => (
                  <tr key={loan.reference} className="hover:bg-gray-50 transition-colors">
                    <td className="p-3 font-mono font-bold text-[#103a27]">{loan.reference}</td>
                    <td className="p-3">
                      <p className="font-bold text-[#103a27]">{loan.borrowerName}</p>
                      <p className="text-[0.65rem] text-gray-500">{loan.borrowerMeta}</p>
                    </td>
                    <td className="p-3">
                      <span className="rounded bg-gray-100 px-2 py-0.5 text-[0.65rem] font-bold text-[#103a27]">
                        {loan.type}
                      </span>
                    </td>
                    <td className="p-3 font-mono font-bold text-[#103a27]">{formatUGX(loan.principal)}</td>
                    <td className="p-3">
                      {loan.repaymentProgress ? (
                        <div>
                          <p className="text-xs font-semibold text-gray-800">{loan.repaymentProgress}</p>
                          {loan.dueDate && <p className="text-[0.65rem] text-gray-500">{loan.dueDate}</p>}
                        </div>
                      ) : (
                        <span className="text-gray-400">---</span>
                      )}
                    </td>
                    <td className="p-3">
                      <span
                        className={`rounded-full px-2.5 py-1 text-[0.65rem] font-bold ${
                          loan.status === 'REPAYING' || loan.status === 'COMPLETED'
                            ? 'bg-emerald-100 text-emerald-800'
                            : 'bg-amber-100 text-amber-800'
                        }`}
                      >
                        {loan.status === 'REPAYING' ? 'DISBURSED (ACTIVE)' : loan.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardBody>
      </Card>
    </div>
  )
}
