'use client'

import { useState, useMemo } from 'react'
import { FileText, Clock, CheckCircle2, AlertTriangle, Eye, Search, Plus, Sparkles, ArrowRight } from 'lucide-react'
import { formatUGX, type Application } from '@/lib/talenton-data'

export function LoanApplicationsList({
  applications,
  onNew,
  onViewDetails,
}: {
  applications: Application[]
  onNew: () => void
  onViewDetails?: (app: Application) => void
}) {
  const [searchQuery, setSearchQuery] = useState('')

  // Filter out drafts — Loan Applications page is strictly for submitted / active pipeline loans
  const realSubmittedApps = useMemo(() => {
    return applications
      .filter((app) => app.status !== 'draft' && app.stage !== 'draft')
      .map((app) => {
        let stepText = 'Underwriting Verification'
        let stepNum = 2

        if (app.stage === 'verification') {
          stepText = 'Document Verification'
          stepNum = 2
        } else if (app.stage === 'underwriting') {
          stepText = 'Risk Desk Audit'
          stepNum = 3
        } else if (app.stage === 'committee') {
          stepText = 'Committee Board Quorum'
          stepNum = 4
        } else if (app.stage === 'disbursed' || app.status === 'disbursed') {
          stepText = 'Disbursed & Active'
          stepNum = 5
        }

        if (app.status === 'declined') {
          stepText = 'Declined'
          stepNum = 5
        }

        return {
          id: app.id,
          reference: app.reference,
          applicantName: app.fullName || 'Member Applicant',
          purpose: app.purpose || 'Working Capital',
          loanType: app.applicantType === 'cooperative' ? 'SME Business Facility' : 'BOSA Member Credit',
          typeCode: app.applicantType === 'cooperative' ? 'business' : 'personal',
          principal: app.principal,
          status: app.status,
          stepText,
          stepNum,
          submittedOn: app.submittedOn || 'Submitted',
          statusNote: app.statusNote,
          rawApp: app,
        }
      })
  }, [applications])

  const filteredApps = useMemo(() => {
    return realSubmittedApps.filter((app) => {
      return (
        app.reference.toLowerCase().includes(searchQuery.toLowerCase()) ||
        app.applicantName.toLowerCase().includes(searchQuery.toLowerCase()) ||
        app.purpose.toLowerCase().includes(searchQuery.toLowerCase()) ||
        app.loanType.toLowerCase().includes(searchQuery.toLowerCase())
      )
    })
  }, [realSubmittedApps, searchQuery])

  // Count summaries
  const totalCount = realSubmittedApps.length
  const pendingVerificationCount = realSubmittedApps.filter(a => a.status === 'submitted' || a.rawApp.stage === 'verification').length
  const committeeCount = realSubmittedApps.filter(a => a.rawApp.stage === 'committee').length
  const disbursedCount = realSubmittedApps.filter(a => a.status === 'disbursed' || a.rawApp.stage === 'disbursed').length

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      {/* Header section with Search and Action on the same line */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 pb-2 border-b border-gray-200">
        <div>
          <h1 className="font-serif text-3xl font-bold text-[#103a27]">Submitted Loan Applications</h1>
          <p className="mt-1 text-sm text-[#2a5040]/70">
            Real-time tracking of all active credit applications submitted to underwriting and the committee board.
          </p>
        </div>
        
        <div className="flex items-center gap-3">
          {/* Search bar */}
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400" />
            <input
              type="text"
              placeholder="Search by reference or name..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9 pr-4 py-2 w-64 rounded-xl border border-gray-200 text-xs font-semibold focus:outline-none focus:border-[#103a27] transition-all bg-white"
            />
          </div>
          {/* Add New Application Button */}
          <button
            type="button"
            onClick={onNew}
            className="flex items-center gap-2 rounded-xl bg-[#0d2a1c] hover:bg-[#153e2a] text-white px-4 py-2.5 text-xs font-bold transition-all shadow-md cursor-pointer shrink-0"
          >
            <Plus className="size-4" />
            New Application
          </button>
        </div>
      </div>

      {/* KPI Stats Row */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        <div className="bg-white p-4 rounded-2xl border border-gray-150 shadow-sm">
          <p className="text-[0.65rem] font-bold uppercase text-gray-400">Total Submitted</p>
          <p className="text-2xl font-extrabold text-[#103a27] font-mono mt-1">{totalCount}</p>
        </div>
        <div className="bg-white p-4 rounded-2xl border border-gray-150 shadow-sm">
          <p className="text-[0.65rem] font-bold uppercase text-gray-400">In Underwriting</p>
          <p className="text-2xl font-extrabold text-amber-700 font-mono mt-1">{pendingVerificationCount}</p>
        </div>
        <div className="bg-white p-4 rounded-2xl border border-gray-150 shadow-sm">
          <p className="text-[0.65rem] font-bold uppercase text-gray-400">In Committee Vote</p>
          <p className="text-2xl font-extrabold text-blue-700 font-mono mt-1">{committeeCount}</p>
        </div>
        <div className="bg-white p-4 rounded-2xl border border-gray-150 shadow-sm">
          <p className="text-[0.65rem] font-bold uppercase text-gray-400">Disbursed & Active</p>
          <p className="text-2xl font-extrabold text-emerald-700 font-mono mt-1">{disbursedCount}</p>
        </div>
      </div>

      {/* Empty State */}
      {filteredApps.length === 0 ? (
        <div className="bg-white rounded-3xl p-12 text-center border border-gray-200 shadow-sm space-y-4">
          <div className="size-16 rounded-full bg-[#103a27]/5 text-[#103a27] mx-auto flex items-center justify-center">
            <FileText className="size-8 text-[#103a27]" />
          </div>
          <div className="max-w-md mx-auto">
            <h3 className="font-serif text-xl font-bold text-[#103a27]">No Active Submitted Applications</h3>
            <p className="text-xs text-gray-500 mt-1">
              You do not have any applications currently in the underwriting pipeline. Start a new credit application to request financing.
            </p>
          </div>
          <button
            type="button"
            onClick={onNew}
            className="inline-flex items-center gap-2 rounded-full bg-[#103a27] text-white px-6 py-2.5 text-xs font-bold shadow-md hover:bg-[#1a5235] transition-all cursor-pointer"
          >
            <Plus className="size-4" />
            Create Application
          </button>
        </div>
      ) : (
        /* Applications Table */
        <div className="bg-white rounded-2xl shadow-sm border border-gray-150 overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-left text-xs">
              <thead className="bg-[#f4f5f4] uppercase font-bold text-gray-500 tracking-wider border-b border-gray-200">
                <tr>
                  <th className="p-4">Reference</th>
                  <th className="p-4">Applicant / Facility</th>
                  <th className="p-4">Requested Principal</th>
                  <th className="p-4">Pipeline Stage</th>
                  <th className="p-4">Submitted Date</th>
                  <th className="p-4 text-right">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 font-medium">
                {filteredApps.map((app) => (
                  <tr key={app.id} className="hover:bg-gray-50/80 transition-colors">
                    <td className="p-4">
                      <span className="font-mono font-bold text-xs bg-gray-100 text-gray-800 px-2.5 py-1 rounded-md">
                        {app.reference}
                      </span>
                    </td>
                    <td className="p-4">
                      <p className="font-bold text-[#103a27] text-sm">{app.applicantName}</p>
                      <p className="text-[0.65rem] text-gray-500">{app.purpose} &bull; <span className="font-semibold text-emerald-800">{app.loanType}</span></p>
                    </td>
                    <td className="p-4">
                      <span className="font-mono font-bold text-sm text-[#103a27]">
                        {formatUGX(app.principal)}
                      </span>
                    </td>
                    <td className="p-4">
                      <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-[0.65rem] font-bold uppercase ${
                        app.rawApp.stage === 'disbursed'
                          ? 'bg-emerald-100 text-emerald-900 border border-emerald-300'
                          : app.rawApp.stage === 'committee'
                          ? 'bg-blue-100 text-blue-900 border border-blue-200'
                          : 'bg-amber-100 text-amber-900 border border-amber-200'
                      }`}>
                        <Clock className="size-3" />
                        {app.stepText}
                      </span>
                    </td>
                    <td className="p-4 text-gray-500 font-medium">
                      {app.submittedOn}
                    </td>
                    <td className="p-4 text-right">
                      <button
                        type="button"
                        onClick={() => onViewDetails ? onViewDetails(app.rawApp) : null}
                        className="inline-flex items-center gap-1 px-3 py-1.5 rounded-lg bg-gray-100 text-gray-700 hover:bg-[#103a27] hover:text-white transition-all text-xs font-bold cursor-pointer"
                      >
                        <Eye className="size-3.5" />
                        Details
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  )
}
