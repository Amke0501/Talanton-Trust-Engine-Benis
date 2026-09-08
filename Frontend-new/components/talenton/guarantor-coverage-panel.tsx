'use client'

import { useEffect, useState } from 'react'
import { fetchGuarantorCoverage, type GuarantorCoverage } from '@/lib/api-service'
import { formatUGX } from '@/lib/talenton-data'

/**
 * What the guarantors actually cover on this file, and whose shares are committed.
 *
 * The server has computed this for weeks with no screen showing it, so the protection was real
 * but invisible — the progress report could only offer testers a raw API link.
 */
export function GuarantorCoveragePanel({ reference }: { reference: string }) {
  const [coverage, setCoverage] = useState<GuarantorCoverage | null>(null)
  const [state, setState] = useState<'loading' | 'ready' | 'unknown-file' | 'unavailable'>('loading')

  useEffect(() => {
    let active = true
    setState('loading')
    fetchGuarantorCoverage(reference)
      .then((result) => {
        if (!active) return
        if (result.state === 'ok') { setCoverage(result.coverage); setState('ready') }
        else { setState(result.state) }
      })
      .catch(() => { if (active) setState('unavailable') })
    return () => { active = false }
  }, [reference])

  if (state === 'loading') {
    return <p className="text-xs text-gray-500">Checking guarantor coverage…</p>
  }

  if (state === 'unknown-file') {
    return (
      <p className="text-xs text-gray-600">
        This application was created in this browser and has not been submitted to the server yet,
        so there are no guarantor records to check against.
      </p>
    )
  }

  if (state === 'unavailable' || !coverage) {
    return (
      <p className="text-xs text-amber-700">
        Guarantor coverage could not be retrieved &mdash; the server did not respond.
      </p>
    )
  }

  const covered = coverage.isCovered

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-500">
            Uncollateralised gap
          </p>
          <p className="text-xl font-bold tabular-nums text-[#103a27]">{formatUGX(coverage.loanGap)}</p>
        </div>
        <span
          className={`rounded-full px-3 py-1 text-[0.65rem] font-bold ${
            covered ? 'bg-emerald-600 text-white' : 'bg-rose-600 text-white'
          }`}
        >
          {covered ? 'COVERED' : `SHORT BY ${formatUGX(coverage.deficit)}`}
        </span>
      </div>

      <dl className="grid grid-cols-2 gap-x-4 gap-y-1.5 text-xs">
        <dt className="text-gray-600">Total pledged</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(coverage.totalPledgedShares)}</dd>
        <dt className="text-gray-600">Still available to pledge</dt>
        <dd className="text-right font-semibold tabular-nums text-[#103a27]">{formatUGX(coverage.totalAvailableShares)}</dd>
      </dl>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[26rem] text-left text-xs">
          <thead>
            <tr className="border-b border-gray-200 text-[0.6rem] uppercase tracking-widest text-gray-500">
              <th className="py-2 font-semibold">Guarantor</th>
              <th className="py-2 text-right font-semibold">Pledged</th>
              <th className="py-2 text-right font-semibold">Available</th>
              <th className="py-2 text-right font-semibold">Capacity</th>
            </tr>
          </thead>
          <tbody>
            {coverage.guarantors.map((g) => (
              <tr key={g.id} className="border-b border-gray-100 last:border-none">
                <td className="py-2">
                  <span className="font-semibold text-[#103a27]">{g.name}</span>
                  <span className="ml-1.5 font-mono text-[0.65rem] text-gray-500">{g.memberId}</span>
                </td>
                <td className="py-2 text-right tabular-nums">{formatUGX(g.pledgedShares)}</td>
                <td className="py-2 text-right tabular-nums">{formatUGX(g.availableShares)}</td>
                <td className="py-2 text-right">
                  <span
                    className={`rounded-full px-2 py-0.5 text-[0.6rem] font-bold ${
                      g.isCapacitySufficient ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'
                    }`}
                  >
                    {g.isCapacitySufficient ? 'OK' : 'SHORT'}
                  </span>
                </td>
              </tr>
            ))}
            {coverage.guarantors.length === 0 && (
              <tr>
                <td colSpan={4} className="py-3 text-center text-gray-500">
                  No guarantors pledged against this file.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <p className="text-[0.7rem] leading-relaxed text-gray-600">{coverage.reason}</p>
    </div>
  )
}
