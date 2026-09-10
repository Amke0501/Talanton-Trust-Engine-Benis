'use client'

import { useMemo, useState } from 'react'
import { AlertTriangle, Plus, Trash2, Users } from 'lucide-react'
import { formatUGX, type Application, type Guarantor } from '@/lib/talenton-data'
import { MINIMUM_ADDITIONAL_GUARANTORS } from '@/lib/api-service'

/**
 * What an applicant does after declining a revised offer.
 *
 * QA reported this whole step as missing: the file sat in underwriting with nothing saying it
 * needed more guarantors and no way to supply them. The server has always required two guarantors
 * who are not already on the file — this is the screen that says so and collects them.
 */
export function ReplacementGuarantorsPanel({
  application,
  onResubmit,
  busy,
}: {
  application: Application
  onResubmit: (guarantors: Guarantor[]) => Promise<{ ok: boolean; reason?: string }>
  busy?: boolean
}) {
  const required = application.minimumAdditionalGuarantorsRequired || MINIMUM_ADDITIONAL_GUARANTORS
  const existingIds = useMemo(
    () => new Set((application.guarantors || []).map((g) => g.memberId.toLowerCase())),
    [application.guarantors]
  )

  const [rows, setRows] = useState<Guarantor[]>([])
  const [name, setName] = useState('')
  const [memberId, setMemberId] = useState('')
  const [pledged, setPledged] = useState('')
  const [available, setAvailable] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [result, setResult] = useState<string | null>(null)

  const uncollateralised = Math.max(0, application.principal - application.savingsBalance)
  const alreadyPledged = (application.guarantors || []).reduce((sum, g) => sum + g.pledgedShares, 0)
  const newlyPledged = rows.reduce((sum, g) => sum + g.pledgedShares, 0)
  const coverAfter = alreadyPledged + newlyPledged

  function addRow() {
    setError(null)
    setResult(null)

    const trimmedName = name.trim()
    const trimmedId = memberId.trim()
    const pledgedShares = Number(pledged)
    const availableShares = Number(available || pledged)

    if (!trimmedName || !trimmedId) {
      setError('A name and a membership number are both needed.')
      return
    }
    // The server counts only guarantors who are not already on the file, so a duplicate is
    // rejected here rather than being silently discarded on submission.
    if (existingIds.has(trimmedId.toLowerCase())) {
      setError(`${trimmedId} already guarantees this application, so they do not count as a new guarantor.`)
      return
    }
    if (rows.some((r) => r.memberId.toLowerCase() === trimmedId.toLowerCase())) {
      setError(`${trimmedId} is already in this list.`)
      return
    }
    if (!Number.isFinite(pledgedShares) || pledgedShares <= 0) {
      setError('Enter the shares this guarantor is pledging.')
      return
    }
    if (availableShares < pledgedShares) {
      setError('A guarantor cannot pledge more shares than they hold.')
      return
    }

    setRows((prev) => [
      ...prev,
      { id: `new-${trimmedId}`, name: trimmedName, memberId: trimmedId, pledgedShares, availableShares },
    ])
    setName('')
    setMemberId('')
    setPledged('')
    setAvailable('')
  }

  async function submit() {
    setError(null)
    setResult(null)
    if (rows.length < required) {
      setError(`${required} additional guarantors are required; ${rows.length} added so far.`)
      return
    }
    const outcome = await onResubmit(rows)
    if (outcome.ok) {
      setRows([])
      setResult('Sent back to the underwriting desk for a fresh decision.')
    } else {
      setError(outcome.reason || 'The application was not resubmitted.')
    }
  }

  return (
    <div className="rounded-xl border border-rose-200 bg-rose-50 p-4 space-y-4">
      <div className="flex items-start gap-2.5">
        <AlertTriangle className="mt-0.5 size-4 shrink-0 text-rose-700" />
        <div>
          <p className="text-sm font-bold text-rose-950">
            {required} additional guarantor{required === 1 ? '' : 's'} required
          </p>
          <p className="mt-0.5 text-xs text-rose-900">
            You declined the revised offer, so this file stays at the underwriting desk. Add{' '}
            {required} guarantor{required === 1 ? '' : 's'} who do not already back this application
            and resubmit it for a fresh decision. Without them it terminates here.
          </p>
        </div>
      </div>

      <dl className="grid grid-cols-2 gap-x-4 gap-y-1 rounded-lg bg-white/70 p-3 text-xs">
        <dt className="text-rose-900">Uncollateralised gap</dt>
        <dd className="text-right font-semibold tabular-nums text-rose-950">{formatUGX(uncollateralised)}</dd>
        <dt className="text-rose-900">Cover already pledged</dt>
        <dd className="text-right font-semibold tabular-nums text-rose-950">{formatUGX(alreadyPledged)}</dd>
        <dt className="text-rose-900">Cover after these additions</dt>
        <dd
          className={`text-right font-bold tabular-nums ${
            coverAfter >= uncollateralised ? 'text-emerald-700' : 'text-rose-950'
          }`}
        >
          {formatUGX(coverAfter)}
        </dd>
      </dl>

      {rows.length > 0 && (
        <ul className="space-y-2">
          {rows.map((row) => (
            <li
              key={row.memberId}
              className="flex items-center justify-between rounded-lg border border-rose-200 bg-white px-3 py-2"
            >
              <div className="min-w-0">
                <p className="truncate text-xs font-bold text-[#103a27]">
                  {row.name}{' '}
                  <span className="font-mono text-[0.65rem] font-normal text-gray-500">({row.memberId})</span>
                </p>
                <p className="text-[0.65rem] text-gray-500">
                  Pledging <strong className="font-mono text-gray-800">{formatUGX(row.pledgedShares)}</strong> of{' '}
                  {formatUGX(row.availableShares ?? row.pledgedShares)}
                </p>
              </div>
              <button
                type="button"
                aria-label={`Remove ${row.name}`}
                onClick={() => setRows((prev) => prev.filter((r) => r.memberId !== row.memberId))}
                className="p-1 text-gray-400 hover:text-rose-700"
              >
                <Trash2 className="size-3.5" />
              </button>
            </li>
          ))}
        </ul>
      )}

      <div className="grid gap-2 sm:grid-cols-2">
        <input
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="Guarantor name"
          aria-label="Guarantor name"
          className="rounded-lg border border-rose-200 bg-white p-2 text-xs focus:border-[#103a27] focus:outline-none"
        />
        <input
          value={memberId}
          onChange={(e) => setMemberId(e.target.value)}
          placeholder="Membership number (e.g. M-1104)"
          aria-label="Guarantor membership number"
          className="rounded-lg border border-rose-200 bg-white p-2 font-mono text-xs focus:border-[#103a27] focus:outline-none"
        />
        <input
          type="number"
          value={pledged}
          onChange={(e) => setPledged(e.target.value)}
          placeholder="Shares pledged (UGX)"
          aria-label="Shares pledged"
          className="rounded-lg border border-rose-200 bg-white p-2 font-mono text-xs focus:border-[#103a27] focus:outline-none"
        />
        <input
          type="number"
          value={available}
          onChange={(e) => setAvailable(e.target.value)}
          placeholder="Shares they hold (defaults to pledged)"
          aria-label="Shares held"
          className="rounded-lg border border-rose-200 bg-white p-2 font-mono text-xs focus:border-[#103a27] focus:outline-none"
        />
      </div>

      {error && <p className="text-xs font-semibold text-rose-800">{error}</p>}
      {result && <p className="text-xs font-semibold text-emerald-800">{result}</p>}

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={addRow}
          className="flex items-center gap-1.5 rounded-lg border border-rose-300 bg-white px-3 py-2 text-xs font-bold text-rose-950 hover:bg-rose-100"
        >
          <Plus className="size-3.5" />
          Add guarantor
        </button>

        <button
          type="button"
          onClick={submit}
          disabled={busy || rows.length < required}
          className="flex items-center gap-1.5 rounded-lg bg-[#103a27] px-4 py-2 text-xs font-bold text-white transition-colors hover:bg-[#1a5235] disabled:cursor-not-allowed disabled:opacity-45"
        >
          <Users className="size-3.5" />
          {busy ? 'Resubmitting…' : `Resubmit with ${rows.length}/${required} guarantors`}
        </button>
      </div>
    </div>
  )
}
