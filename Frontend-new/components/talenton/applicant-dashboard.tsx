'use client'

import { 
  ArrowRight, 
  FileText, 
  Plus, 
  ShieldCheck, 
  Wallet, 
  CheckCircle2, 
  Clock, 
  FileCheck, 
  ChevronRight, 
  Check, 
  Save, 
  AlertCircle 
} from 'lucide-react'
import {
  formatUGX,
  STATUS_META,
  type Application,
  type Guarantor,
} from '@/lib/talenton-data'
import { ReplacementGuarantorsPanel } from '@/components/talenton/replacement-guarantors-panel'

export function ApplicantDashboard({
  userName,
  applications,
  onNew,
  onResumeDraft,
  onCounterOfferDecision,
  onResubmitWithGuarantors,
  busyReference,
  feedback,
}: {
  userName: string
  applications: Application[]
  onNew: () => void
  onResumeDraft?: (app: Application) => void
  /**
   * The reference is passed explicitly. It used to be omitted, so the handler acted on whichever
   * application happened to be selected in the parent — usually the first in the list — and
   * pressing Accept on any other file appeared to do nothing at all.
   */
  onCounterOfferDecision?: (reference: string, decision: 'ACCEPT' | 'DECLINE') => Promise<void>
  onResubmitWithGuarantors?: (
    reference: string,
    guarantors: Guarantor[]
  ) => Promise<{ ok: boolean; reason?: string }>
  busyReference?: string | null
  feedback?: { reference: string; tone: 'ok' | 'error'; message: string } | null
}) {
  const firstName = userName.split(' ')[0] || 'there'

  // Drafts
  const draftApplications = applications.filter((a) => a.status === 'draft' || a.stage === 'draft')

  // Active submitted / in-review / committee pipeline
  const activePipelineApplications = applications.filter(
    (a) => a.status !== 'draft' && a.stage !== 'draft' && a.stage !== 'disbursed'
  )

  function isAwaitingGuarantors(app: Application): boolean {
    return (
      app.status === 'awaiting_guarantors' ||
      (app.counterOfferStatus === 'DECLINED' && (app.minimumAdditionalGuarantorsRequired ?? 0) > 0)
    )
  }

  // Approved & disbursed loans
  const approvedOrDisbursed = applications.filter(
    (a) => a.status === 'approved' || a.status === 'disbursed' || a.stage === 'disbursed'
  )
  const totalBorrowed = approvedOrDisbursed.reduce((s, a) => s + a.principal, 0)

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      
      {/* Welcome Banner */}
      <div className="rounded-3xl bg-[#0d2a1c] p-8 text-white shadow-xl relative overflow-hidden">
        <div className="absolute right-8 top-1/2 -translate-y-1/2 opacity-10 flex items-center justify-center pointer-events-none">
          <ShieldCheck className="size-48" />
        </div>
        <div className="relative z-10 max-w-xl space-y-4">
          <p className="text-[0.7rem] font-bold uppercase tracking-widest text-[#a4cc44]">SACCO MEMBER PORTAL</p>
          <h2 className="font-serif text-3xl md:text-4xl font-bold leading-tight">Hello {firstName}, ready to grow?</h2>
          <p className="text-sm text-white/80 leading-relaxed">
            Apply for credit from your SACCO in a few guided steps, save your application progress, or track underwriting and board quorum in real time.
          </p>
          <button
            type="button"
            onClick={onNew}
            className="inline-flex items-center gap-2 rounded-full bg-[#a4cc44] text-[#0d2a1c] px-6 py-3 text-xs font-bold shadow-lg hover:bg-[#b5dc55] transition-all cursor-pointer"
          >
            <Plus className="size-4" />
            Start a new credit application
          </button>
        </div>
      </div>

      {/* Summary KPI Row */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="p-5 border-none shadow-sm rounded-2xl bg-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-gray-400">Total Borrowed</p>
            <span className="p-2 bg-emerald-50 rounded-xl text-emerald-600">
              <Wallet className="size-4" />
            </span>
          </div>
          <div className="mt-3">
            <h3 className="text-2xl font-extrabold text-[#103a27] font-mono">{formatUGX(totalBorrowed)}</h3>
            <p className="text-xs text-gray-400 mt-0.5">{approvedOrDisbursed.length} facilities active/cleared</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-gray-400">Active Requests</p>
            <span className="p-2 bg-amber-50 rounded-xl text-amber-600">
              <Clock className="size-4" />
            </span>
          </div>
          <div className="mt-3">
            <h3 className="text-2xl font-extrabold text-[#103a27] font-mono">{activePipelineApplications.length}</h3>
            <p className="text-xs text-gray-400 mt-0.5">In verification or committee vote</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-gray-400">Saved Drafts</p>
            <span className="p-2 bg-gray-100 rounded-xl text-gray-600">
              <Save className="size-4" />
            </span>
          </div>
          <div className="mt-3">
            <h3 className="text-2xl font-extrabold text-[#103a27] font-mono">{draftApplications.length}</h3>
            <p className="text-xs text-gray-400 mt-0.5">Ready to complete and submit</p>
          </div>
        </div>
      </div>

      {/* SECTION 1: Saved Drafts Section */}
      {draftApplications.length > 0 && (
        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <h3 className="text-sm font-bold uppercase tracking-widest text-[#103a27]">
                Saved Application Drafts
              </h3>
              <span className="bg-gray-200 text-gray-800 text-xs font-bold px-2 py-0.5 rounded-full">
                {draftApplications.length}
              </span>
            </div>
          </div>

          <div className="grid gap-3">
            {draftApplications.map((draft) => (
              <div 
                key={draft.id} 
                className="bg-white rounded-2xl p-5 shadow-sm border border-gray-150 flex flex-col sm:flex-row sm:items-center justify-between gap-4 hover:shadow-md transition-shadow"
              >
                <div className="flex items-center gap-4">
                  <div className="size-11 rounded-xl bg-gray-100 text-gray-600 flex items-center justify-center font-bold text-xs shrink-0">
                    <Save className="size-5 text-gray-600" />
                  </div>
                  <div>
                    <div className="flex items-center gap-2">
                      <h4 className="font-bold text-[#103a27] text-base">{draft.purpose || 'Untitled Credit Draft'}</h4>
                      <span className="text-[0.65rem] font-bold uppercase px-2.5 py-0.5 rounded-full bg-gray-100 text-gray-700">
                        Draft
                      </span>
                    </div>
                    <p className="text-xs text-gray-500 mt-0.5">
                      Requested: <strong className="text-gray-800 font-mono">{formatUGX(draft.principal || 0)}</strong> &bull; {draft.tenureMonths || 12} Months tenure &bull; {draft.applicantType === 'individual' ? 'Individual' : 'SME'}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-3">
                  <button
                    type="button"
                    onClick={() => onResumeDraft ? onResumeDraft(draft) : onNew()}
                    className="px-4 py-2 rounded-xl bg-[#103a27] text-white text-xs font-bold hover:bg-[#1a5235] transition-colors flex items-center gap-1.5 shrink-0"
                  >
                    Resume & Submit
                    <ArrowRight className="size-3.5" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* SECTION 2: Active Pipeline & Status Tracking */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="text-sm font-bold uppercase tracking-widest text-[#103a27]">
            Active Credit Applications & Tracking
          </h3>
        </div>

        {activePipelineApplications.length === 0 ? (
          <div className="bg-white rounded-2xl p-10 text-center border-dashed border-2 border-gray-200">
            <FileText className="size-8 mx-auto text-gray-300 mb-2" />
            <p className="text-sm font-semibold text-gray-700">No active applications in review.</p>
            <p className="text-xs text-gray-400 mt-1">Start a new application above to request SACCO financing.</p>
          </div>
        ) : (
          <div className="grid gap-3">
            {activePipelineApplications.map((app) => {
              const isCommittee = app.stage === 'committee'
              const isUnderwriting = app.stage === 'underwriting' || app.stage === 'verification'
              const isCounterOfferPending = app.counterOfferStatus === 'PENDING' || app.status === 'counter_offer_pending'
              const needsGuarantors = isAwaitingGuarantors(app)
              // A file that ended at the underwriting desk was still labelled "Stage 3:
              // Underwriting Risk Audit", which reads as work in progress on a decision that has
              // already been made.
              const isDeclined = !needsGuarantors && (app.status === 'declined' || app.verdict === 'DECLINED')
              const isBusy = busyReference === app.reference
              const appFeedback = feedback && feedback.reference === app.reference ? feedback : null

              return (
                <div 
                  key={app.id} 
                  className="bg-white rounded-2xl p-6 shadow-sm border border-gray-150 space-y-4"
                >
                  <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-bold text-lg text-[#103a27]">{app.fullName}</h4>
                        <span className="font-mono text-xs bg-gray-100 text-gray-700 px-2 py-0.5 rounded-md font-bold">
                          {app.reference}
                        </span>
                      </div>
                      <p className="text-xs text-gray-500 mt-0.5">
                        {app.purpose} &bull; Principal: <strong className="text-gray-800 font-mono">{formatUGX(app.principal)}</strong>
                      </p>
                    </div>

                    <div>
                      <span className={`px-3 py-1 rounded-full text-xs font-bold uppercase ${
                        isCounterOfferPending
                          ? 'bg-amber-100 text-amber-900'
                          : needsGuarantors || isDeclined
                          ? 'bg-rose-100 text-rose-900'
                          : isCommittee
                          ? 'bg-amber-100 text-amber-900'
                          : 'bg-emerald-100 text-emerald-900'
                      }`}>
                        {isCounterOfferPending
                          ? 'Applicant consent required'
                          : needsGuarantors
                          ? 'Additional guarantors required'
                          : isDeclined
                          ? 'Declined at underwriting'
                          : isCommittee
                          ? 'Stage 4: Committee Quorum Vote'
                          : 'Stage 3: Underwriting Risk Audit'}
                      </span>
                    </div>
                  </div>

                  {isCounterOfferPending && (
                    <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 space-y-3">
                      <p className="text-sm font-bold text-amber-950">Your revised offer is ready for consent.</p>
                      <p className="text-xs text-amber-900">
                        Revised principal: <strong className="font-mono">{formatUGX(app.counterOfferPrincipal ?? app.principal)}</strong>
                        {' '}&bull; Tenure: <strong>{app.counterOfferTenureMonths ?? app.tenureMonths} months</strong>
                      </p>
                      {app.counterOfferReason && <p className="text-xs text-amber-900">{app.counterOfferReason}</p>}
                      <p className="text-[0.7rem] text-amber-800">
                        Declining keeps the file open, but {app.minimumAdditionalGuarantorsRequired || 2} additional
                        guarantors will then be required before it can be reconsidered.
                      </p>
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          disabled={isBusy}
                          onClick={() => onCounterOfferDecision?.(app.reference, 'ACCEPT')}
                          className="rounded-lg bg-[#103a27] px-4 py-2 text-xs font-bold text-white transition-colors hover:bg-[#1a5235] disabled:cursor-not-allowed disabled:opacity-50"
                        >
                          {isBusy ? 'Recording…' : 'Accept revised offer'}
                        </button>
                        <button
                          type="button"
                          disabled={isBusy}
                          onClick={() => onCounterOfferDecision?.(app.reference, 'DECLINE')}
                          className="rounded-lg border border-amber-300 px-4 py-2 text-xs font-bold text-amber-950 transition-colors hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-50"
                        >
                          Decline offer
                        </button>
                      </div>
                    </div>
                  )}

                  {app.counterOfferStatus === 'ACCEPTED' && app.applicantConsentAt && (
                    <div className="rounded-xl border border-emerald-200 bg-emerald-50 p-4">
                      <p className="text-xs font-bold text-emerald-950">Revised offer accepted</p>
                      <p className="mt-0.5 text-xs text-emerald-900">
                        Consent recorded on{' '}
                        {new Date(app.applicantConsentAt).toLocaleString('en-GB', {
                          day: '2-digit',
                          month: 'short',
                          year: 'numeric',
                          hour: '2-digit',
                          minute: '2-digit',
                        })}{' '}
                        against <strong className="font-mono">{formatUGX(app.principal)}</strong> over{' '}
                        <strong>{app.tenureMonths} months</strong>.
                      </p>
                    </div>
                  )}

                  {appFeedback && (
                    <p
                      role="status"
                      className={`rounded-xl border px-3.5 py-2.5 text-xs font-semibold ${
                        appFeedback.tone === 'ok'
                          ? 'border-emerald-200 bg-emerald-50 text-emerald-900'
                          : 'border-rose-200 bg-rose-50 text-rose-900'
                      }`}
                    >
                      {appFeedback.message}
                    </p>
                  )}

                  {needsGuarantors && onResubmitWithGuarantors && (
                    <ReplacementGuarantorsPanel
                      application={app}
                      busy={isBusy}
                      onResubmit={(guarantors) => onResubmitWithGuarantors(app.reference, guarantors)}
                    />
                  )}

                  {/* Note & Status */}
                  <div className="p-3.5 rounded-xl bg-gray-50 border border-gray-100 flex items-start gap-2.5">
                    <ShieldCheck className="size-4 text-[#103a27] mt-0.5 shrink-0" />
                    <p className="text-xs text-gray-600 leading-relaxed">
                      <strong>Current Status:</strong> {app.statusNote || 'Your application is progressing through our automated risk guardrails and underwriting audit.'}
                    </p>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

    </div>
  )
}
