'use client'

import { useEffect, useState } from 'react'
import { 
  FileText, 
  ArrowRight, 
  Search, 
  Filter, 
  ShieldCheck, 
  CheckCircle2, 
  XCircle, 
  Minus, 
  ThumbsUp, 
  Users,
  AlertTriangle
} from 'lucide-react'
import type { Application } from '@/lib/talenton-data'
import { formatUGX, evaluateQuorum, terminatesAtUnderwriting } from '@/lib/talenton-data'
import {
  disburseBatch,
  fetchLiquidityStatus,
  type BatchDisbursementItem,
  type LiquidityStatus,
} from '@/lib/api-service'
import { LiquidityIndicator } from '@/components/talenton/liquidity-indicator'

export function CommitteeLoansList({
  applications,
  onSelectApplication,
  onRefresh,
}: {
  applications: Application[]
  onSelectApplication: (app: Application) => void
  /** Called after a batch release so the queue reflects what just went out. */
  onRefresh?: () => void
}) {
  const [filter, setFilter] = useState<'ALL' | 'PENDING' | 'APPROVED' | 'DISBURSED'>('ALL')
  const [searchQuery, setSearchQuery] = useState('')

  // The cash position belongs at the top of this screen, not inside an opened file: a board
  // member should see whether the SACCO can lend at all before choosing what to read.
  const [liquidity, setLiquidity] = useState<LiquidityStatus | null>(null)
  const [releasing, setReleasing] = useState(false)
  const [batch, setBatch] = useState<{ reason: string; items: BatchDisbursementItem[] } | null>(null)

  useEffect(() => {
    let active = true
    fetchLiquidityStatus().then((s) => active && setLiquidity(s ?? null))
    return () => {
      active = false
    }
  }, [])

  const cashLocked = liquidity?.isLocked ?? false
  const cashUnknown = liquidity == null || liquidity.isAvailable === false

  async function releaseBatch() {
    setReleasing(true)
    setBatch(null)
    const result = await disburseBatch()
    setReleasing(false)
    setBatch({ reason: result.reason, items: result.items })
    // Re-read rather than trusting the local figure: the batch just changed the position.
    fetchLiquidityStatus().then((s) => setLiquidity(s ?? null))
    if (result.released > 0) onRefresh?.()
  }

  // Everything the committee may see. Counting is done from this list, never from the raw
  // `applications` array: the tab counts used to be computed before the declined-file filter, so
  // "All Files" advertised files the list below deliberately hid, and the count and the list
  // disagreed about the size of the queue.
  const committeeVisible = applications.filter((app) => !terminatesAtUnderwriting(app))

  const counts = {
    ALL: committeeVisible.length,
    PENDING: committeeVisible.filter((a) => a.stage === 'committee').length,
    APPROVED: committeeVisible.filter((a) => a.status === 'approved' || a.stage === 'disbursed').length,
    DISBURSED: committeeVisible.filter((a) => a.stage === 'disbursed').length,
  }

  const filteredApps = committeeVisible.filter((app) => {
    // Filter status
    if (filter === 'PENDING') {
      if (app.stage !== 'committee' && !(app.stage === 'underwriting' && app.verdict === 'APPROVED')) return false
    }
    if (filter === 'APPROVED') {
      if (app.status !== 'approved' && app.stage !== 'disbursed') return false
    }
    if (filter === 'DISBURSED') {
      if (app.stage !== 'disbursed') return false
    }

    // Search query
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase()
      return (
        app.fullName?.toLowerCase().includes(q) ||
        app.reference?.toLowerCase().includes(q) ||
        app.purpose?.toLowerCase().includes(q)
      )
    }

    return true
  })

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-4 border-b border-gray-200">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Committee Review Queue</h2>
          <p className="text-sm text-gray-500 mt-1">
            Review applicant files, monitor board sign-offs, and cast quorum votes.
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400" />
            <input 
              type="text" 
              placeholder="Search reference or name..." 
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9 pr-4 py-2 text-sm rounded-full border border-gray-200 bg-white focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] transition-all w-full md:w-64"
            />
          </div>
        </div>
      </div>

      {/* Cash position and the master release. The specification puts this at the top of the loan
          processing screen, interlocked with the batch action — the gate is only useful if it is
          visible before a decision, not after opening a file. */}
      <div className="grid gap-4 lg:grid-cols-5">
        <div className="lg:col-span-3">
          <LiquidityIndicator />
        </div>

        <div
          className={`flex flex-col justify-between gap-3 rounded-2xl border p-4 lg:col-span-2 ${
            cashLocked || cashUnknown ? 'border-rose-300 bg-rose-50' : 'border-emerald-200 bg-emerald-50'
          }`}
        >
          <div>
            <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-600">
              Batch release
            </p>
            <p className="mt-1 text-xs leading-relaxed text-gray-700">
              {cashUnknown
                ? 'The cash position could not be read, so nothing can be released.'
                : cashLocked
                ? `Blocked by the liquidity gate. ${formatUGX(liquidity!.deficit)} short of the 2:1 buffer.`
                : `Releases every approved file the cash reaches, oldest first, up to ${formatUGX(
                    liquidity!.maxSafeDisbursementCap
                  )}.`}
            </p>
          </div>

          <button
            type="button"
            onClick={releaseBatch}
            disabled={releasing || cashLocked || cashUnknown}
            title={
              cashLocked || cashUnknown
                ? 'Disabled while the liquidity gate is locked'
                : 'Release every approved file the cash position reaches'
            }
            className={`flex items-center justify-center gap-2 rounded-xl px-4 py-2.5 text-xs font-bold transition-colors ${
              cashLocked || cashUnknown
                ? 'cursor-not-allowed bg-gray-300 text-gray-600'
                : 'bg-[#103a27] text-white hover:bg-[#1a5235]'
            }`}
          >
            <ShieldCheck className="size-3.5" />
            {releasing ? 'Releasing…' : 'Approve & Disburse Batch'}
          </button>
        </div>
      </div>

      {batch && (
        <div className="rounded-2xl border border-gray-200 bg-white p-4">
          <p className="text-xs font-bold text-[#103a27]">{batch.reason}</p>
          {batch.items.length > 0 && (
            <ul className="mt-2.5 space-y-1.5">
              {batch.items.map((item) => (
                <li key={item.reference} className="flex flex-wrap items-baseline gap-x-2 text-[0.7rem]">
                  <span className="font-mono font-semibold text-[#103a27]">{item.reference}</span>
                  <span
                    className={`rounded px-1.5 py-0.5 text-[0.6rem] font-bold uppercase ${
                      item.released
                        ? 'bg-emerald-100 text-emerald-800'
                        : item.deferred
                        ? 'bg-amber-100 text-amber-900'
                        : 'bg-rose-100 text-rose-800'
                    }`}
                  >
                    {item.released ? 'Released' : item.deferred ? 'Deferred' : 'Held'}
                  </span>
                  <span className="text-gray-600">{item.reason}</span>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}

      {/* Filter Tabs */}
      <div className="flex flex-wrap gap-2">
        {[
          { id: 'ALL', label: `All Files (${counts.ALL})` },
          { id: 'PENDING', label: `Awaiting Quorum (${counts.PENDING})` },
          { id: 'APPROVED', label: `Approved (${counts.APPROVED})` },
          { id: 'DISBURSED', label: `Disbursed (${counts.DISBURSED})` },
        ].map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setFilter(tab.id as any)}
            className={`rounded-full px-4 py-1.5 text-xs font-semibold transition-all ${
              filter === tab.id
                ? 'bg-[#103a27] text-white shadow-sm'
                : 'bg-white text-gray-600 hover:bg-gray-100 border border-gray-200'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* List */}
      {filteredApps.length === 0 ? (
        <div className="bg-white rounded-2xl p-12 text-center border-dashed border-2 border-gray-200 mt-4">
          <FileText className="size-10 mx-auto text-gray-300 mb-4" />
          <p className="text-base font-semibold text-gray-600">No applications match your filter.</p>
          <p className="text-sm text-gray-400 mt-2">Try clearing your search query or selecting a different category.</p>
        </div>
      ) : (
        <div className="grid gap-4 mt-2">
          {filteredApps.map((app) => {
            // No invented votes. This list used to fall back to five hardcoded ballots when a file
            // had none, so an untouched application was displayed as though the board had already
            // voted on it — three approvals and all.
            const votes = app.committeeVotes || []
            // The size-based rule, shared with the committee dashboard and the server: one
            // approval below 5M; three including the Chairperson and Treasurer at or above it.
            // This row was still counting a four-out-of-five quorum that no longer exists
            // anywhere else, so the badge disagreed with the file it sat on.
            const quorum = evaluateQuorum(votes, app.principal)
            const approveCount = quorum.approvalCount
            const isApproved = quorum.isQuorumPassed || app.status === 'approved' || app.stage === 'disbursed'

            return (
              <div 
                key={app.id} 
                className="border-none shadow-sm rounded-2xl bg-white overflow-hidden hover:shadow-md transition-shadow group p-5 md:p-6"
              >
                <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
                  
                  {/* Left: Applicant Identity */}
                  <div className="flex items-start gap-4 min-w-[280px]">
                    <div className="size-12 rounded-xl bg-[#0d2a1c] text-[#a4cc44] flex items-center justify-center font-bold text-sm shrink-0 mt-0.5">
                      {app.applicantType === 'individual' ? 'Individual' : 'SME'}
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-bold text-[#103a27] text-base">{app.fullName}</h4>
                        <span className="text-xs font-mono bg-gray-100 px-2 py-0.5 rounded text-gray-600 font-semibold">
                          {app.reference}
                        </span>
                      </div>
                      <p className="text-xs text-gray-500 mt-1">
                        Purpose: <span className="text-gray-700 font-medium">{app.purpose || 'Working Capital'}</span>
                      </p>

                      {/* The committee's fixed 2:1 member limit, measured against the savings the
                          SACCO records — not the figure declared on the application. It sits on the
                          row because it bears on the decision being made there, rather than being
                          something a board member has to go and look for. */}
                      {app.isWithinMemberLimit === false && (
                        <p className="mt-1.5 inline-flex items-start gap-1.5 rounded-lg border border-rose-200 bg-rose-50 px-2 py-1 text-[0.65rem] font-semibold leading-snug text-rose-900">
                          <AlertTriangle className="mt-px size-3 shrink-0" />
                          <span>
                            Exceeds 2:1 Member Limit. Requires{' '}
                            {formatUGX(app.memberLimitShortfall ?? 0)} in Guarantor Deposits.
                          </span>
                        </p>
                      )}
                      <div className="flex items-center gap-3 mt-2 text-xs text-gray-500">
                        <span>DTI: <strong className="text-gray-800">{app.dtiNetRatio?.toFixed(1) || '28.5'}%</strong></span>
                        <span>Multiplier: <strong className="text-gray-800">{app.multiplier || 3}x</strong></span>
                      </div>
                    </div>
                  </div>

                  {/* Middle: Who Has Approved Breakdown */}
                  <div className="flex-1 border-t lg:border-t-0 lg:border-l lg:border-r border-gray-100 pt-4 lg:pt-0 lg:px-6">
                    <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400 mb-2.5">
                      Board Sign-off Votes ({approveCount}/{quorum.requiredApprovals} Approved)
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {votes.map((v) => {
                        const isApp = v.vote === 'APPROVE'
                        const isRej = v.vote === 'REJECT'
                        return (
                          <div 
                            key={v.id || v.role}
                            className={`flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-medium border ${
                              isApp 
                                ? 'bg-emerald-50 border-emerald-200 text-emerald-800' 
                                : isRej 
                                  ? 'bg-rose-50 border-rose-200 text-rose-800' 
                                  : 'bg-gray-50 border-gray-200 text-gray-600'
                            }`}
                            title={`${v.name || v.role}: ${v.vote || 'PENDING'}`}
                          >
                            {isApp ? (
                              <CheckCircle2 className="size-3 text-emerald-600" />
                            ) : isRej ? (
                              <XCircle className="size-3 text-rose-600" />
                            ) : (
                              <Minus className="size-3 text-gray-400" />
                            )}
                            <span className="text-[0.7rem] font-semibold">{v.role}</span>
                          </div>
                        )
                      })}
                    </div>
                  </div>

                  {/* Right: Principal & Action */}
                  <div className="flex flex-row lg:flex-col items-center lg:items-end justify-between gap-4 shrink-0">
                    <div className="text-left lg:text-right">
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Principal</p>
                      <p className="text-base font-bold text-[#103a27] font-mono">{formatUGX(app.principal || 0)}</p>
                      <span className={`inline-block mt-1 text-[0.65rem] font-bold uppercase px-2 py-0.5 rounded-full ${
                        isApproved ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'
                      }`}>
                        {isApproved ? 'Quorum Passed' : 'Pending Quorum'}
                      </span>
                    </div>

                    <button
                      onClick={() => onSelectApplication(app)}
                      className="flex items-center justify-center gap-2 px-5 py-2 rounded-xl bg-[#103a27] text-white text-xs font-bold hover:bg-[#124a31] transition-colors group-hover:bg-[#a4cc44] group-hover:text-[#0d2a1c]"
                    >
                      View Approval Details
                      <ArrowRight className="size-3.5" />
                    </button>
                  </div>
                  
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
