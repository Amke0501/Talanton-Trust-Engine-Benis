'use client'

import { useState } from 'react'
import { KeyRound, ShieldAlert } from 'lucide-react'
import type { EmergencyRelease } from '@/lib/api-service'

/**
 * The dual-key release: two different officers, together, may release funds the liquidity gate
 * has locked.
 *
 * QA found the override missing entirely, so a locked file had no route forward short of waiting
 * for cash. The rules below mirror the server's — it re-checks all of them — and the reason is
 * written into the audit trail against both officers, because an override that leaves no trace is
 * indistinguishable from a bug.
 */
const KEY_HOLDER_SEATS = ['Chairperson', 'Treasurer', 'Secretary'] as const
const MINIMUM_REASON_LENGTH = 15

export function EmergencyReleaseDialog({
  reference,
  shortfallMessage,
  currentSeat,
  keyHolders = KEY_HOLDER_SEATS as unknown as string[],
  onCancel,
  onAuthorize,
  busy,
}: {
  reference: string
  shortfallMessage: string
  currentSeat: string | null
  keyHolders?: string[]
  onCancel: () => void
  onAuthorize: (release: EmergencyRelease) => Promise<void>
  busy?: boolean
}) {
  const seatIsKeyHolder = currentSeat != null && keyHolders.includes(currentSeat)
  const [firstSeat, setFirstSeat] = useState(seatIsKeyHolder ? (currentSeat as string) : keyHolders[0])
  const [secondSeat, setSecondSeat] = useState(
    keyHolders.find((s) => s !== (seatIsKeyHolder ? currentSeat : keyHolders[0])) ?? keyHolders[1]
  )
  const [reason, setReason] = useState('')
  const [error, setError] = useState<string | null>(null)

  async function submit() {
    setError(null)
    if (firstSeat === secondSeat) {
      setError('An emergency release needs two different officers.')
      return
    }
    if (reason.trim().length < MINIMUM_REASON_LENGTH) {
      setError(`Give a written reason of at least ${MINIMUM_REASON_LENGTH} characters. It is recorded against both officers.`)
      return
    }
    await onAuthorize({ firstSeat, secondSeat, reason: reason.trim() })
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="emergency-release-title"
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 p-4 backdrop-blur-sm"
    >
      <div className="w-full max-w-lg space-y-4 rounded-2xl bg-white p-6 shadow-2xl">
        <div className="flex items-start gap-3">
          <span className="flex size-10 shrink-0 items-center justify-center rounded-full bg-rose-100">
            <ShieldAlert className="size-5 text-rose-700" />
          </span>
          <div>
            <h4 id="emergency-release-title" className="font-serif text-base font-bold text-[#103a27]">
              Emergency release &mdash; {reference}
            </h4>
            <p className="mt-1 text-xs leading-relaxed text-gray-600">
              This releases funds against the liquidity lock. Two different key-holding officers must
              authorise it, and the reason is written to the audit trail under both names.
            </p>
          </div>
        </div>

        <p className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-xs leading-relaxed text-rose-900">
          {shortfallMessage}
        </p>

        <div className="grid gap-3 sm:grid-cols-2">
          <label className="space-y-1">
            <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
              First signature
            </span>
            <select
              value={firstSeat}
              onChange={(e) => setFirstSeat(e.target.value)}
              className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-semibold text-[#103a27] focus:border-[#103a27] focus:outline-none"
            >
              {keyHolders.map((seat) => (
                <option key={seat} value={seat}>
                  {seat}
                </option>
              ))}
            </select>
          </label>

          <label className="space-y-1">
            <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
              Second signature
            </span>
            <select
              value={secondSeat}
              onChange={(e) => setSecondSeat(e.target.value)}
              className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-semibold text-[#103a27] focus:border-[#103a27] focus:outline-none"
            >
              {keyHolders.map((seat) => (
                <option key={seat} value={seat}>
                  {seat}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="block space-y-1">
          <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
            Reason for overriding the cash lock
          </span>
          <textarea
            rows={3}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="e.g. Board resolution 14/2026 authorises bridging finance already contracted."
            className="w-full rounded-xl border border-gray-200 p-2.5 text-xs focus:border-[#103a27] focus:outline-none"
          />
        </label>

        {!seatIsKeyHolder && currentSeat && (
          <p className="text-[0.7rem] text-amber-800">
            You are signed in as {currentSeat}, which does not hold an emergency key. Both signatures
            must come from: {keyHolders.join(', ')}.
          </p>
        )}

        {error && <p className="text-xs font-semibold text-rose-700">{error}</p>}

        <div className="flex justify-end gap-2 pt-1">
          <button
            type="button"
            onClick={onCancel}
            className="rounded-full bg-gray-100 px-4 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-200"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={submit}
            disabled={busy}
            className="flex items-center gap-1.5 rounded-full bg-rose-700 px-5 py-2 text-xs font-bold text-white hover:bg-rose-800 disabled:cursor-not-allowed disabled:opacity-50"
          >
            <KeyRound className="size-3.5" />
            {busy ? 'Authorising…' : 'Authorise emergency release'}
          </button>
        </div>
      </div>
    </div>
  )
}
