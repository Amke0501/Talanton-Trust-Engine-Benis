'use client'

import { useEffect, useState } from 'react'
import { fetchLiquidityStatus, type LiquidityStatus } from '@/lib/api-service'
import { formatUGX } from '@/lib/talenton-data'

/**
 * Cash position for the committee: how much the SACCO holds against what it has already
 * committed to files awaiting release. The ratio was computed on the server for weeks with
 * nothing displaying it, so nobody could see the SACCO approaching an over-extended position.
 */
export function LiquidityIndicator() {
  const [status, setStatus] = useState<LiquidityStatus | null>(null)
  const [state, setState] = useState<'loading' | 'ready' | 'unavailable'>('loading')

  useEffect(() => {
    let active = true
    fetchLiquidityStatus()
      .then((result) => {
        if (!active) return
        if (result) { setStatus(result); setState('ready') } else { setState('unavailable') }
      })
      .catch(() => { if (active) setState('unavailable') })
    return () => { active = false }
  }, [])

  if (state === 'loading') {
    return (
      <div className="rounded-2xl border border-gray-200 bg-white p-4">
        <p className="text-xs text-gray-500">Checking cash position…</p>
      </div>
    )
  }

  if (state === 'unavailable' || !status) {
    return (
      <div className="rounded-2xl border border-amber-300 bg-amber-50 p-4">
        <p className="text-[0.65rem] font-bold uppercase tracking-widest text-amber-700">
          Cash position unavailable
        </p>
        <p className="mt-1 text-xs text-amber-800">
          The ledger could not be read, so disbursements are proceeding without a cash-safety check.
        </p>
      </div>
    )
  }

  const safe = !status.isLocked
  const headroom = Math.min(100, Math.round((status.currentLiquidityRatio / (status.minimumSafeRatio * 2)) * 100))

  return (
    <div className={`rounded-2xl border p-4 ${safe ? 'border-emerald-200 bg-emerald-50' : 'border-rose-300 bg-rose-50'}`}>
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className={`text-[0.65rem] font-bold uppercase tracking-widest ${safe ? 'text-emerald-700' : 'text-rose-700'}`}>
            {safe ? 'Cash position healthy' : 'Lending on hold — cash too low'}
          </p>
          <p className="mt-1 text-2xl font-bold tabular-nums text-[#103a27]">
            {status.currentLiquidityRatio.toFixed(2)}
            <span className="ml-1 text-xs font-semibold text-gray-500">
              / {status.minimumSafeRatio.toFixed(2)} minimum
            </span>
          </p>
        </div>
        <span
          className={`rounded-full px-3 py-1 text-[0.65rem] font-bold ${
            safe ? 'bg-emerald-600 text-white' : 'bg-rose-600 text-white'
          }`}
        >
          {safe ? 'SAFE' : 'BLOCKED'}
        </span>
      </div>

      <div className="mt-3 h-2 w-full overflow-hidden rounded-full bg-white">
        <div
          className={`h-full rounded-full ${safe ? 'bg-emerald-500' : 'bg-rose-500'}`}
          style={{ width: `${Math.max(4, headroom)}%` }}
        />
      </div>

      <dl className="mt-3 grid grid-cols-2 gap-x-4 gap-y-1.5 text-xs">
        <dt className="text-gray-600">Cash on hand</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.totalLiquidCash)}</dd>
        <dt className="text-gray-600">Committed, awaiting release</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.totalPendingLoans)}</dd>
        <dt className="text-gray-600">Safe to release</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.maxSafeDisbursementCap)}</dd>
        {!safe && (
          <>
            <dt className="font-semibold text-rose-700">Shortfall</dt>
            <dd className="text-right font-bold tabular-nums text-rose-700">{formatUGX(status.deficit)}</dd>
          </>
        )}
      </dl>
    </div>
  )
}
