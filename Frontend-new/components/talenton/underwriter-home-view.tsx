'use client'

import { FileText, Clock, AlertTriangle, ShieldCheck, CheckCircle2, TrendingUp, ArrowRight } from 'lucide-react'
import type { Application } from '@/lib/talenton-data'
import { formatUGX } from '@/lib/talenton-data'

export function UnderwriterHomeView({
  applications,
  onNavigateToApplications,
}: {
  applications: Application[]
  onNavigateToApplications: () => void
}) {
  const pendingApps = applications.filter(
    (a) => a.status !== 'draft' && a.stage !== 'draft' && a.stage !== 'committee' && a.stage !== 'disbursed'
  )
  const verifiedApps = applications.filter(
    (a) => a.stage === 'committee' || a.stage === 'disbursed' || a.status === 'approved'
  )
  
  const totalExposure = pendingApps.reduce((acc, app) => acc + (app.principal || 0), 0)

  return (
    <div className="space-y-6">
      
      {/* Header section */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-4">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Trust Engine Overview</h2>
          <p className="text-sm text-gray-500 mt-1">
            Real-time pipeline metrics and underwriting queue.
          </p>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Awaiting Audit</p>
            <span className="p-2 bg-white/10 rounded-xl text-[#a4cc44]">
              <Clock className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-3xl font-extrabold text-white">{pendingApps.length}</h3>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Total Exposure</p>
            <span className="p-2 bg-white/10 rounded-xl text-[#a4cc44]">
              <TrendingUp className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-2xl font-extrabold text-white">{formatUGX(totalExposure)}</h3>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[120px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Cleared Files</p>
            <span className="p-2 bg-white/10 rounded-xl text-[#a4cc44]">
              <CheckCircle2 className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-3xl font-extrabold text-white">{verifiedApps.length}</h3>
          </div>
        </div>
      </div>

      {/* Priority Queue / Recent */}
      <div className="mt-8">
        <h3 className="text-sm font-bold uppercase tracking-widest text-gray-500 mb-4">Priority Audit Queue</h3>
        
        {pendingApps.length === 0 ? (
          <div className="bg-white rounded-2xl p-10 text-center border-dashed border-2 border-gray-200">
            <FileText className="size-8 mx-auto text-gray-300 mb-3" />
            <p className="text-sm font-semibold text-gray-600">No applications awaiting audit.</p>
            <p className="text-xs text-gray-400 mt-1">You're all caught up!</p>
          </div>
        ) : (
          <div className="bg-white rounded-2xl border shadow-sm overflow-hidden">
            <div className="divide-y">
              {pendingApps.slice(0, 5).map((app) => (
                <div key={app.id} className="p-4 sm:p-5 flex items-center justify-between hover:bg-gray-50 transition-colors">
                  <div className="flex items-center gap-4">
                    <div className="size-10 rounded-xl bg-[#a4cc44]/10 text-[#0d2a1c] flex items-center justify-center font-bold text-xs shrink-0">
                      {app.applicantType === 'individual' ? 'IND' : 'SME'}
                    </div>
                    <div>
                      <h4 className="font-semibold text-[#103a27] text-sm">{app.fullName}</h4>
                      <p className="text-xs text-gray-500 mt-0.5">{app.reference} • {formatUGX(app.principal || 0)}</p>
                    </div>
                  </div>
                  <button
                    onClick={onNavigateToApplications}
                    className="px-4 py-2 rounded-lg bg-gray-100 text-xs font-semibold text-gray-700 hover:bg-gray-200 transition-colors"
                  >
                    Review
                  </button>
                </div>
              ))}
            </div>
            {pendingApps.length > 5 && (
              <div className="bg-gray-50 p-3 text-center border-t">
                <button
                  onClick={onNavigateToApplications}
                  className="text-xs font-semibold text-[#103a27] hover:underline"
                >
                  View {pendingApps.length - 5} more
                </button>
              </div>
            )}
          </div>
        )}
      </div>

    </div>
  )
}
