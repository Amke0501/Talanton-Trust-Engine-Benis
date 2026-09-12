'use client'

import { useEffect, useState } from 'react'
import { AlertTriangle, Check, Copy, UserPlus } from 'lucide-react'
import {
  fetchProvisioningStatus,
  registerMember,
  type ProvisioningStatus,
  type RegisteredMember,
} from '@/lib/api-service'
import { Card, CardBody } from '@/components/talenton/primitives'

/**
 * Registering a vetted member, and creating the login that goes with it.
 *
 * This is the only way an account comes into existence, because SACCO membership is granted
 * offline: someone is admitted to the cooperative first, and the login follows. There is no public
 * sign-up, so the landing page does not offer one — a stranger who created a login would get a
 * working password and no access to anything, which is a worse experience than being told plainly
 * that accounts come from the SACCO.
 */
const PORTALS = [
  { value: 'applicant', label: 'Applicant', hint: 'Applies for credit and answers revised offers' },
  { value: 'underwriter', label: 'Underwriter', hint: 'Runs the guardrail checks and routes files' },
  { value: 'committee', label: 'Committee', hint: 'Votes on files and releases funds' },
] as const

export function RegisterMemberView() {
  const [status, setStatus] = useState<ProvisioningStatus | null>(null)
  const [loadingStatus, setLoadingStatus] = useState(true)

  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [portalRole, setPortalRole] = useState<string>('applicant')
  const [committeeSeat, setCommitteeSeat] = useState('')
  const [password, setPassword] = useState('')

  // Only the portals the server says this member may enrol into. An underwriting desk enrols
  // borrowers; appointing a board seat is the committee's to do, because whoever registers an
  // account also sets its first password.
  const offerablePortals = PORTALS.filter(
    (p) => !status || status.registerablePortals?.includes(p.value)
  )

  const [busy, setBusy] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [registered, setRegistered] = useState<RegisteredMember | null>(null)
  const [copied, setCopied] = useState(false)

  useEffect(() => {
    let active = true
    fetchProvisioningStatus()
      .then((result) => {
        if (!active) return
        setStatus(result ?? null)
        if (result?.seats?.length && !committeeSeat) setCommitteeSeat(result.seats[0])
        // Start on a portal this member may actually create, so the form is never pre-filled
        // with a choice the server will refuse.
        const allowed = result?.registerablePortals ?? []
        if (allowed.length && !allowed.includes(portalRole)) setPortalRole(allowed[0])
      })
      .finally(() => active && setLoadingStatus(false))
    return () => {
      active = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  async function submit() {
    setError(null)
    setRegistered(null)
    setCopied(false)

    if (!fullName.trim()) return setError("Enter the member's full name.")
    if (!email.trim() || !email.includes('@')) return setError('Enter a valid email address.')
    if (portalRole === 'committee' && !committeeSeat) {
      return setError('Choose the seat this member will hold on the board.')
    }

    setBusy(true)
    const outcome = await registerMember({
      fullName: fullName.trim(),
      email: email.trim(),
      portalRole,
      committeeSeat: portalRole === 'committee' ? committeeSeat : undefined,
      password: password.trim() || undefined,
    })
    setBusy(false)

    if (!outcome.ok) {
      setError(outcome.reason)
      return
    }

    setRegistered(outcome.member)
    setFullName('')
    setEmail('')
    setPassword('')
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div className="border-b border-gray-200 pb-4">
        <h2 className="font-serif text-2xl font-bold text-[#103a27]">Register a member</h2>
        <p className="mt-1 text-sm text-gray-500">
          Creates the member&rsquo;s SACCO record and their sign-in together. Members are admitted to
          the cooperative first; this gives them a way in.
        </p>
      </div>

      {loadingStatus && <p className="text-sm text-gray-500">Checking whether accounts can be created…</p>}

      {!loadingStatus && status && !status.canCreateAccounts && (
        <div className="flex items-start gap-2.5 rounded-xl border border-amber-300 bg-amber-50 p-4">
          <AlertTriangle className="mt-0.5 size-4 shrink-0 text-amber-700" />
          <div>
            <p className="text-sm font-bold text-amber-950">Accounts cannot be created from here yet</p>
            <p className="mt-1 text-xs leading-relaxed text-amber-900">{status.message}</p>
          </div>
        </div>
      )}

      {!loadingStatus && status?.canCreateAccounts && !status.usingAdminKey && (
        <p className="rounded-xl border border-gray-200 bg-[#f4f5f4] px-4 py-3 text-xs leading-relaxed text-gray-600">
          {status.message}
        </p>
      )}

      {registered && (
        <div className="space-y-3 rounded-xl border border-emerald-200 bg-emerald-50 p-4">
          <div className="flex items-start gap-2.5">
            <Check className="mt-0.5 size-4 shrink-0 text-emerald-700" />
            <div>
              <p className="text-sm font-bold text-emerald-950">{registered.message}</p>
              <p className="mt-0.5 text-xs text-emerald-900">
                {registered.fullName} &middot; {registered.portalRole}
                {registered.committeeSeat ? ` · ${registered.committeeSeat}` : ''}
              </p>
            </div>
          </div>

          {registered.initialPassword && (
            <div className="rounded-lg border border-emerald-300 bg-white p-3">
              <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-500">
                Initial password &mdash; shown once
              </p>
              <div className="mt-1.5 flex items-center justify-between gap-3">
                <code className="font-mono text-sm font-bold text-[#103a27]">
                  {registered.initialPassword}
                </code>
                <button
                  type="button"
                  onClick={() => {
                    navigator.clipboard?.writeText(registered.initialPassword!).then(
                      () => setCopied(true),
                      () => setCopied(false)
                    )
                  }}
                  className="flex items-center gap-1.5 rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs font-semibold text-gray-600 hover:bg-gray-50"
                >
                  <Copy className="size-3" />
                  {copied ? 'Copied' : 'Copy'}
                </button>
              </div>
              <p className="mt-2 text-[0.7rem] text-gray-500">
                Pass this to {registered.fullName} directly. It is not stored anywhere and cannot be
                shown again &mdash; they should change it after signing in.
              </p>
            </div>
          )}
        </div>
      )}

      <Card className="rounded-2xl border-none bg-white shadow-sm">
        <CardBody className="space-y-4 p-6">
          <div className="grid gap-4 sm:grid-cols-2">
            <label className="space-y-1.5">
              <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
                Full name
              </span>
              <input
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="e.g. Nakamya Grace"
                className="w-full rounded-xl border border-gray-200 p-2.5 text-sm focus:border-[#103a27] focus:outline-none"
              />
            </label>

            <label className="space-y-1.5">
              <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
                Email address
              </span>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="name@example.com"
                className="w-full rounded-xl border border-gray-200 p-2.5 font-mono text-sm focus:border-[#103a27] focus:outline-none"
              />
            </label>
          </div>

          <fieldset className="space-y-2">
            <legend className="text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
              Portal
            </legend>
            {status && offerablePortals.length < PORTALS.length && (
              <p className="text-[0.7rem] leading-relaxed text-gray-500">
                Your portal may enrol{' '}
                {offerablePortals.map((p) => p.label.toLowerCase()).join(', ')} accounts. Appointing
                someone to a board seat is the committee&rsquo;s to do &mdash; a seat carries the
                power to approve loans and release funds.
              </p>
            )}
            <div className="grid gap-2 sm:grid-cols-3">
              {offerablePortals.map((p) => (
                <button
                  key={p.value}
                  type="button"
                  onClick={() => setPortalRole(p.value)}
                  aria-pressed={portalRole === p.value}
                  className={`rounded-xl border p-3 text-left transition-colors ${
                    portalRole === p.value
                      ? 'border-[#103a27] bg-[#f2f7e4]'
                      : 'border-gray-200 bg-white hover:bg-gray-50'
                  }`}
                >
                  <span className="block text-xs font-bold text-[#103a27]">{p.label}</span>
                  <span className="mt-0.5 block text-[0.65rem] leading-snug text-gray-500">{p.hint}</span>
                </button>
              ))}
            </div>
          </fieldset>

          {portalRole === 'committee' && (
            <label className="space-y-1.5">
              <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
                Seat on the board
              </span>
              <select
                value={committeeSeat}
                onChange={(e) => setCommitteeSeat(e.target.value)}
                className="w-full rounded-xl border border-gray-200 p-2.5 text-sm focus:border-[#103a27] focus:outline-none"
              >
                {(status?.seats ?? []).map((seat) => (
                  <option key={seat} value={seat}>
                    {seat}
                  </option>
                ))}
              </select>
              <span className="block text-[0.7rem] text-gray-500">
                Decides whose approval counts toward quorum and who may release funds. One member per
                seat.
              </span>
            </label>
          )}

          <label className="space-y-1.5">
            <span className="block text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
              Initial password <span className="font-normal normal-case">(optional)</span>
            </span>
            <input
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Leave blank to generate a strong one"
              className="w-full rounded-xl border border-gray-200 p-2.5 font-mono text-sm focus:border-[#103a27] focus:outline-none"
            />
          </label>

          {error && (
            <p role="alert" className="rounded-xl border border-rose-200 bg-rose-50 px-3.5 py-2.5 text-xs font-semibold text-rose-900">
              {error}
            </p>
          )}

          <button
            type="button"
            onClick={submit}
            disabled={busy || (status !== null && !status.canCreateAccounts)}
            className="flex items-center gap-2 rounded-full bg-[#103a27] px-5 py-3 text-xs font-bold text-white transition-colors hover:bg-[#1a5235] disabled:cursor-not-allowed disabled:opacity-50"
          >
            <UserPlus className="size-3.5" />
            {busy ? 'Registering…' : 'Register member'}
          </button>
        </CardBody>
      </Card>
    </div>
  )
}
