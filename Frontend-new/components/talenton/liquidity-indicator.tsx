'use client'

import { useEffect, useState } from 'react'
import { fetchLiquidityStatus, type LiquidityStatus } from '@/lib/api-service'
import { formatUGX } from '@/lib/talenton-data'

/**
 * Cash position for the committee: how much the SACCO holds against what it has already
 * committed to files awaiting release, and which file the cash actually reaches.
 *
 * The ratio was computed on the server for weeks with nothing displaying it, so nobody could see
 * the SACCO approaching an over-extended position. The release queue below it is the other half
 * of that answer: a healthy ratio still does not mean *this* file is next.
 */
export function LiquidityIndicator({ highlightReference }: { highlightReference?: string }) {
  const [status, setStatus] = useState<LiquidityStatus | null>(null)
  const [state, setState] = useState<'loading' | 'ready' | 'unavailable'>('loading')
  const [showQueue, setShowQueue] = useState(false)

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

  // The gate fails closed, so an unreadable ledger blocks releases rather than waving them
  // through. The panel has to say that, or the board is told nothing while nothing works.
  if (state === 'unavailable' || !status) {
    return (
      <div className="rounded-2xl border border-rose-300 bg-rose-50 p-4">
        <p className="text-[0.65rem] font-bold uppercase tracking-widest text-rose-700">
          System lock: cash position unavailable
        </p>
        <p className="mt-1 text-xs text-rose-900">
          The ledger could not be read. Disbursement is blocked until the cash position can be
          verified — funds are never released against an unverified position.
        </p>
      </div>
    )
  }

  const safe = !status.isLocked
  const headroom = Math.min(100, Math.round((status.currentLiquidityRatio / (status.minimumSafeRatio * 2)) * 100))
  const queue = status.queue ?? []
  const deferredCount = queue.filter((q) => !q.isWithinSafeCap).length

  return (
    <div className={`rounded-2xl border p-4 ${safe ? 'border-emerald-200 bg-emerald-50' : 'border-rose-300 bg-rose-50'}`}>
      <div className="flex items-start justify-between gap-4">
        <div>
          <p className={`text-[0.65rem] font-bold uppercase tracking-widest ${safe ? 'text-emerald-700' : 'text-rose-700'}`}>
            {safe
              ? 'Liquidity buffer secure: safe to disburse'
              : 'System lock: insufficient liquidity buffer'}
          </p>
          <p className="mt-1 text-2xl font-bold tabular-nums text-[#103a27]">
            {status.currentLiquidityRatio.toFixed(2)}x
            <span className="ml-1 text-xs font-semibold text-gray-500">
              / {status.minimumSafeRatio.toFixed(2)}x minimum
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
        <dt className="text-gray-600">Total available liquid cash</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.totalLiquidCash)}</dd>
        <dt className="text-gray-600">Total pending loans</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.totalPendingLoans)}</dd>
        <dt className="text-gray-600">Maximum safe disbursement cap</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(status.maxSafeDisbursementCap)}</dd>
        {!safe && (
          <>
            <dt className="font-semibold text-rose-700">Shortfall</dt>
            <dd className="text-right font-bold tabular-nums text-rose-700">{formatUGX(status.deficit)}</dd>
          </>
        )}
      </dl>

      {queue.length > 0 && (
        <div className="mt-3 border-t border-black/5 pt-3">
          <button
            type="button"
            onClick={() => setShowQueue((v) => !v)}
            aria-expanded={showQueue}
            className="flex w-full items-center justify-between text-[0.65rem] font-bold uppercase tracking-widest text-gray-600 hover:text-[#103a27]"
          >
            <span>
              Release queue — {queue.length} committed
              {deferredCount > 0 ? `, ${deferredCount} awaiting liquidity` : ''}
            </span>
            <span aria-hidden="true">{showQueue ? '−' : '+'}</span>
          </button>

          {showQueue && (
            <div className="mt-2 overflow-x-auto">
              <table className="w-full min-w-[22rem] text-left text-[0.7rem]">
                <thead>
                  <tr className="text-[0.6rem] uppercase tracking-widest text-gray-500">
                    <th className="py-1 font-semibold">#</th>
                    <th className="py-1 font-semibold">File</th>
                    <th className="py-1 text-right font-semibold">Principal</th>
                    <th className="py-1 text-right font-semibold">Running total</th>
                    <th className="py-1 text-right font-semibold">Reached</th>
                  </tr>
                </thead>
                <tbody>
                  {queue.map((q) => (
                    <tr
                      key={q.reference}
                      className={`border-t border-black/5 ${
                        highlightReference && q.reference === highlightReference ? 'font-bold' : ''
                      }`}
                    >
                      <td className="py-1 tabular-nums text-gray-500">{q.queuePosition}</td>
                      <td className="py-1 font-mono">{q.reference}</td>
                      <td className="py-1 text-right tabular-nums">{formatUGX(q.principal)}</td>
                      <td className="py-1 text-right tabular-nums text-gray-600">{formatUGX(q.cumulativeDemand)}</td>
                      <td className="py-1 text-right">
                        <span
                          className={`rounded-full px-1.5 py-0.5 text-[0.6rem] font-bold ${
                            q.isWithinSafeCap ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-900'
                          }`}
                        >
                          {q.isWithinSafeCap ? 'YES' : 'DEFERRED'}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <p className="mt-1.5 text-[0.65rem] leading-relaxed text-gray-500">
                Released oldest commitment first. A file below the cap waits its turn rather than
                being refused, and goes out once cash recovers.
              </p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
