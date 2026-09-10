'use client'

import { useState } from 'react'
import { CheckCircle2, Coins, Lock, Unlock } from 'lucide-react'
import { formatUGX, type Application } from '@/lib/talenton-data'

/**
 * Repayment against a disbursed loan, and the guarantor shares it releases.
 *
 * QA: "No confirmed repayment workflow that calls the share-unlock service." The unlock has
 * existed since the lock did, with nothing to trigger it — so shares pledged against a loan that
 * was long since repaid stayed committed and could not back anyone else. Settling the balance
 * here is what calls it.
 */
export function RepaymentPanel({
  application,
  seat,
  onRecordRepayment,
}: {
  application: Application
  seat: string | null
  onRecordRepayment: (
    reference: string,
    amount: number,
    recordedByRole: string
  ) => Promise<{ ok: boolean; reason?: string }>
}) {
  const [amount, setAmount] = useState('')
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const repaid = application.amountRepaid ?? 0
  const outstanding = Math.max(0, application.principal - repaid)
  const settled = Boolean(application.repaidAt) || outstanding <= 0
  const progressPct = application.principal > 0 ? Math.min(100, (repaid / application.principal) * 100) : 0

  const guarantors = application.guarantors || []
  const lockedTotal = guarantors.reduce((sum, g) => sum + (g.lockedShares ?? 0), 0)
  const anyLocked = lockedTotal > 0
  // "Nothing is locked" and "everything has been released" are the same balance but very
  // different facts, and the second is the one a board wants confirmed after a settlement.
  const anyReleased = guarantors.some((g) => Boolean(g.sharesReleasedAt))
  const releasedTotal = guarantors
    .filter((g) => g.sharesReleasedAt)
    .reduce((sum, g) => sum + g.pledgedShares, 0)

  async function submit() {
    setError(null)
    setMessage(null)

    const value = Number(amount)
    if (!Number.isFinite(value) || value <= 0) {
      setError('Enter the amount received.')
      return
    }

    setBusy(true)
    const outcome = await onRecordRepayment(application.reference, value, seat || 'Treasurer')
    setBusy(false)

    if (outcome.ok) {
      setAmount('')
      setMessage(outcome.reason || 'Repayment recorded.')
    } else {
      setError(outcome.reason || 'The repayment was not recorded.')
    }
  }

  return (
    <div className="space-y-4 rounded-2xl border border-gray-200 bg-white p-5">
      <div className="flex items-center justify-between border-b border-gray-100 pb-3">
        <div className="flex items-center gap-2">
          <Coins className="size-4 text-[#103a27]" />
          <h3 className="font-serif text-sm font-bold text-[#103a27]">Repayment & share release</h3>
        </div>
        <span
          className={`rounded-full px-3 py-1 text-[0.65rem] font-bold ${
            settled ? 'bg-emerald-600 text-white' : 'bg-amber-100 text-amber-900'
          }`}
        >
          {settled ? 'SETTLED' : 'REPAYING'}
        </span>
      </div>

      <div>
        <div className="flex items-baseline justify-between text-xs">
          <span className="text-gray-600">Repaid</span>
          <span className="font-mono font-bold text-[#103a27]">
            {formatUGX(repaid)} / {formatUGX(application.principal)}
          </span>
        </div>
        <div className="mt-2 h-2 w-full overflow-hidden rounded-full bg-gray-100">
          <div
            className={`h-full rounded-full ${settled ? 'bg-emerald-500' : 'bg-[#a4cc44]'}`}
            style={{ width: `${Math.max(2, progressPct)}%` }}
          />
        </div>
        {!settled && (
          <p className="mt-1.5 text-[0.7rem] text-gray-500">
            {formatUGX(outstanding)} outstanding. Guarantor shares stay locked until the balance clears.
          </p>
        )}
      </div>

      <div className="rounded-xl border border-gray-100 bg-[#f4f5f4] p-3.5">
        <div className="flex items-center gap-2">
          {anyLocked ? (
            <Lock className="size-3.5 text-amber-700" />
          ) : (
            <Unlock className="size-3.5 text-emerald-700" />
          )}
          <p className="text-xs font-bold text-[#103a27]">
            {anyLocked
              ? `${formatUGX(lockedTotal)} of guarantor shares committed`
              : anyReleased
              ? `${formatUGX(releasedTotal)} of guarantor shares released`
              : 'No guarantor shares are committed against this file'}
          </p>
        </div>

        {guarantors.length > 0 && (
          <ul className="mt-2 space-y-1">
            {guarantors.map((g) => (
              <li key={g.id} className="flex items-center justify-between text-[0.7rem]">
                <span className="text-gray-600">
                  {g.name} <span className="font-mono text-gray-400">({g.memberId})</span>
                </span>
                <span className={(g.lockedShares ?? 0) > 0 ? 'font-semibold text-amber-800' : 'text-emerald-700'}>
                  {(g.lockedShares ?? 0) > 0
                    ? `${formatUGX(g.lockedShares ?? 0)} locked`
                    : g.sharesReleasedAt
                    ? 'released'
                    : 'not locked'}
                </span>
              </li>
            ))}
          </ul>
        )}
      </div>

      {settled ? (
        <p className="flex items-start gap-2 rounded-xl border border-emerald-200 bg-emerald-50 p-3 text-xs text-emerald-900">
          <CheckCircle2 className="mt-0.5 size-3.5 shrink-0 text-emerald-700" />
          <span>
            Repaid in full
            {application.repaidAt
              ? ` on ${new Date(application.repaidAt).toLocaleDateString('en-GB', {
                  day: '2-digit',
                  month: 'short',
                  year: 'numeric',
                })}`
              : ''}
            . The guarantors' pledged shares have been released and are free to back another loan.
          </span>
        </p>
      ) : (
        <div className="flex flex-wrap items-end gap-2">
          <label className="flex-1 space-y-1">
            <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
              Amount received (UGX)
            </span>
            <input
              type="number"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              placeholder={String(outstanding)}
              className="w-full rounded-xl border border-gray-200 p-2.5 font-mono text-xs font-bold text-[#103a27] focus:border-[#103a27] focus:outline-none"
            />
          </label>
          <button
            type="button"
            onClick={submit}
            disabled={busy}
            className="rounded-xl bg-[#103a27] px-4 py-2.5 text-xs font-bold text-white transition-colors hover:bg-[#1a5235] disabled:cursor-not-allowed disabled:opacity-50"
          >
            {busy ? 'Recording…' : 'Record repayment'}
          </button>
        </div>
      )}

      {message && <p className="text-xs font-semibold text-emerald-800">{message}</p>}
      {error && <p className="text-xs font-semibold text-rose-700">{error}</p>}
    </div>
  )
}
