'use client'

import { 
  FileText, 
  Clock, 
  CheckCircle2, 
  ArrowRight, 
  ShieldCheck, 
  UserCheck, 
  TrendingUp, 
  ThumbsUp, 
  AlertCircle 
} from 'lucide-react'
import type { Application } from '@/lib/talenton-data'
import { formatUGX } from '@/lib/talenton-data'

export function CommitteeHomeView({
  applications,
  onNavigateToApplications,
  onSelectApplication,
}: {
  applications: Application[]
  onNavigateToApplications: () => void
  onSelectApplication: (app: Application) => void
}) {
  // Pending applications: in committee stage, or in review with underwriter approval
  const pendingApps = applications.filter(
    (a) => a.stage === 'committee' || (a.stage === 'underwriting' && a.verdict === 'APPROVED')
  )

  // Approved applications: committee approved or disbursed
  const approvedApps = applications.filter(
    (a) => a.stage === 'disbursed' || a.status === 'approved' || a.status === 'disbursed'
  )

  // Total exposure metrics
  const totalVerifiedExposure = applications.reduce((acc, app) => acc + (app.principal || 0), 0)
  const totalPendingAmount = pendingApps.reduce((acc, app) => acc + (app.principal || 0), 0)
  const totalApprovedAmount = approvedApps.reduce((acc, app) => acc + (app.principal || 0), 0)

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      
      {/* Header section */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-2">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Trust Committee Overview</h2>
          <p className="text-sm text-gray-500 mt-1">
            Real-time pipeline metrics and summary of applications requiring board quorum.
          </p>
        </div>
        <button
          onClick={onNavigateToApplications}
          className="flex items-center gap-2 text-sm font-semibold text-[#103a27] hover:text-[#a4cc44] transition-colors"
        >
          View all applications
          <ArrowRight className="size-4" />
        </button>
      </div>

      {/* KPI Cards — Dark Green Theme */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[125px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Verified Exposure</p>
            <span className="p-2 bg-white/10 rounded-xl text-[#a4cc44]">
              <ShieldCheck className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-2xl font-extrabold text-white">{formatUGX(totalVerifiedExposure)}</h3>
            <p className="text-[0.65rem] text-gray-400 mt-1">Total pipeline audited by risk desk</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[125px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Awaiting Approval</p>
            <span className="p-2 bg-white/10 rounded-xl text-amber-400">
              <Clock className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <div className="flex items-baseline gap-2">
              <h3 className="text-3xl font-extrabold text-white">{pendingApps.length}</h3>
              <span className="text-xs font-medium text-gray-300">({formatUGX(totalPendingAmount)})</span>
            </div>
            <p className="text-[0.65rem] text-gray-400 mt-1">Pending board member voting</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[125px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Total Approvals</p>
            <span className="p-2 bg-white/10 rounded-xl text-emerald-400">
              <CheckCircle2 className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <div className="flex items-baseline gap-2">
              <h3 className="text-3xl font-extrabold text-white">{approvedApps.length}</h3>
              <span className="text-xs font-medium text-gray-300">({formatUGX(totalApprovedAmount)})</span>
            </div>
            <p className="text-[0.65rem] text-gray-400 mt-1">Quorum passed and disbursed</p>
          </div>
        </div>
      </div>

      {/* SECTION 1: Pending Applications Needing Approval */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <h3 className="text-sm font-bold uppercase tracking-widest text-[#103a27]">
              Applications Awaiting Your Approval
            </h3>
            <span className="rounded-full bg-amber-100 text-amber-900 text-xs font-bold px-2.5 py-0.5">
              {pendingApps.length} Pending
            </span>
          </div>
          <button
            onClick={onNavigateToApplications}
            className="text-xs font-bold text-[#103a27] hover:underline"
          >
            View in Review Queue &rarr;
          </button>
        </div>

        {pendingApps.length === 0 ? (
          <div className="bg-white rounded-2xl p-8 text-center border-dashed border-2 border-gray-200">
            <CheckCircle2 className="size-8 mx-auto text-emerald-500 mb-2" />
            <p className="text-sm font-semibold text-gray-700">All caught up!</p>
            <p className="text-xs text-gray-400 mt-1">There are no applications currently awaiting committee authorization.</p>
          </div>
        ) : (
          <div className="grid gap-3">
            {pendingApps.map((app) => {
              const votes = app.committeeVotes || []
              const approveCount = votes.filter((v) => v.vote === 'APPROVE').length
              const totalVotes = votes.length || 5

              return (
                <div 
                  key={app.id}
                  className="bg-white rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow border border-gray-100 flex flex-col md:flex-row md:items-center justify-between gap-4 group"
                >
                  <div className="flex items-center gap-4">
                    <div className="size-11 rounded-xl bg-[#0d2a1c] text-[#a4cc44] flex items-center justify-center font-bold text-xs shrink-0">
                      {app.applicantType === 'individual' ? 'BOSA' : 'SME'}
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-bold text-[#103a27] text-base">{app.fullName}</h4>
                        <span className="font-mono text-[0.65rem] bg-gray-100 text-gray-600 px-2 py-0.5 rounded-md font-bold">
                          {app.reference}
                        </span>
                      </div>
                      <p className="text-xs text-gray-500 mt-0.5">
                        {app.purpose || 'Business Loan'} &bull; DTI: <span className="font-semibold text-gray-700">{app.dtiNetRatio?.toFixed(1) || '28.5'}%</span>
                      </p>
                    </div>
                  </div>

                  {/* Financial & Status Summary */}
                  <div className="flex flex-wrap items-center gap-4 md:gap-8">
                    <div>
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Principal</p>
                      <p className="text-sm font-bold text-[#103a27] font-mono">{formatUGX(app.principal || 0)}</p>
                    </div>

                    <div>
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Risk Verdict</p>
                      <span className="inline-flex items-center gap-1 text-[0.65rem] font-bold uppercase text-emerald-700 bg-emerald-50 px-2 py-0.5 rounded-full mt-0.5">
                        <ShieldCheck className="size-3" />
                        Underwriter Passed
                      </span>
                    </div>

                    <div>
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Quorum Progress</p>
                      <p className="text-xs font-bold text-amber-700 mt-0.5">
                        {approveCount}/{totalVotes} Approved
                      </p>
                    </div>

                    <button
                      onClick={() => onSelectApplication(app)}
                      className="px-4 py-2 rounded-xl bg-[#103a27] text-white text-xs font-bold hover:bg-[#124a31] transition-colors flex items-center gap-1.5 shrink-0 group-hover:bg-[#a4cc44] group-hover:text-[#0d2a1c]"
                    >
                      Review & Vote
                      <ArrowRight className="size-3.5" />
                    </button>
                  </div>
                </div>
              )
            })}
          </div>
        )}
      </div>

      {/* SECTION 2: Approved Applications Summary */}
      <div className="space-y-4 pt-2">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <h3 className="text-sm font-bold uppercase tracking-widest text-[#103a27]">
              Approved Applications (Authorized by Committee)
            </h3>
            <span className="rounded-full bg-emerald-100 text-emerald-900 text-xs font-bold px-2.5 py-0.5">
              {approvedApps.length} Approved
            </span>
          </div>
        </div>

        {approvedApps.length === 0 ? (
          <div className="bg-white rounded-2xl p-8 text-center border-dashed border-2 border-gray-200">
            <FileText className="size-8 mx-auto text-gray-300 mb-2" />
            <p className="text-sm font-semibold text-gray-600">No approved applications yet.</p>
            <p className="text-xs text-gray-400 mt-1">Once applications pass quorum, they will be archived here.</p>
          </div>
        ) : (
          <div className="grid gap-3">
            {approvedApps.map((app) => {
              const votes = app.committeeVotes || []
              const approveCount = votes.filter((v) => v.vote === 'APPROVE').length || 4

              return (
                <div 
                  key={app.id}
                  className="bg-white rounded-2xl p-5 shadow-sm border border-gray-100 flex flex-col md:flex-row md:items-center justify-between gap-4 opacity-90 hover:opacity-100 transition-opacity"
                >
                  <div className="flex items-center gap-4">
                    <div className="size-11 rounded-xl bg-emerald-900/10 text-emerald-800 flex items-center justify-center font-bold text-xs shrink-0">
                      <CheckCircle2 className="size-5 text-emerald-600" />
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-bold text-[#103a27] text-base">{app.fullName}</h4>
                        <span className="font-mono text-[0.65rem] bg-gray-100 text-gray-600 px-2 py-0.5 rounded-md font-bold">
                          {app.reference}
                        </span>
                      </div>
                      <p className="text-xs text-gray-500 mt-0.5">
                        {app.purpose || 'Business Loan'} &bull; Status: <span className="font-semibold text-emerald-700 uppercase">{app.stage === 'disbursed' ? 'Disbursed' : 'Approved'}</span>
                      </p>
                    </div>
                  </div>

                  {/* Financial & Outcome */}
                  <div className="flex flex-wrap items-center gap-4 md:gap-8">
                    <div>
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Principal</p>
                      <p className="text-sm font-bold text-[#103a27] font-mono">{formatUGX(app.principal || 0)}</p>
                    </div>

                    <div>
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Quorum Outcome</p>
                      <span className="inline-flex items-center gap-1 text-[0.65rem] font-bold uppercase text-emerald-800 bg-emerald-100 px-2.5 py-0.5 rounded-full mt-0.5">
                        <ThumbsUp className="size-3 text-emerald-700" />
                        Quorum Passed ({approveCount}/5)
                      </span>
                    </div>

                    <button
                      onClick={() => onSelectApplication(app)}
                      className="px-4 py-2 rounded-xl border border-gray-200 bg-gray-50 text-[#103a27] text-xs font-bold hover:bg-gray-100 transition-colors flex items-center gap-1.5 shrink-0"
                    >
                      View Details
                      <ArrowRight className="size-3.5" />
                    </button>
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
