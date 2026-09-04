'use client'

import { useState } from 'react'
import { 
  FileText, 
  ArrowRight, 
  Search, 
  Filter, 
  ShieldCheck, 
  CheckCircle2, 
  XCircle, 
  Minus, 
  ThumbsUp, 
  Users 
} from 'lucide-react'
import type { Application } from '@/lib/talenton-data'
import { formatUGX } from '@/lib/talenton-data'

export function CommitteeLoansList({
  applications,
  onSelectApplication,
}: {
  applications: Application[]
  onSelectApplication: (app: Application) => void
}) {
  const [filter, setFilter] = useState<'ALL' | 'PENDING' | 'APPROVED' | 'DISBURSED'>('ALL')
  const [searchQuery, setSearchQuery] = useState('')

  const filteredApps = applications.filter((app) => {
    // A file declined at underwriting terminates there. The default "All" tab applied no verdict
    // filter, so declined files were listed in the committee queue alongside live ones.
    const isDeclined = app.verdict === 'DECLINED' || app.status === 'declined'
    if (isDeclined && app.stage !== 'disbursed') return false

    // Filter status
    if (filter === 'PENDING') {
      if (app.stage !== 'committee' && !(app.stage === 'underwriting' && app.verdict === 'APPROVED')) return false
    }
    if (filter === 'APPROVED') {
      if (app.status !== 'approved' && app.stage !== 'disbursed') return false
    }
    if (filter === 'DISBURSED') {
      if (app.stage !== 'disbursed') return false
    }

    // Search query
    if (searchQuery.trim()) {
      const q = searchQuery.toLowerCase()
      return (
        app.fullName?.toLowerCase().includes(q) ||
        app.reference?.toLowerCase().includes(q) ||
        app.purpose?.toLowerCase().includes(q)
      )
    }

    return true
  })

  return (
    <div className="space-y-6 max-w-6xl mx-auto">
      
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-4 border-b border-gray-200">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Committee Review Queue</h2>
          <p className="text-sm text-gray-500 mt-1">
            Review applicant files, monitor board sign-offs, and cast quorum votes.
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400" />
            <input 
              type="text" 
              placeholder="Search reference or name..." 
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9 pr-4 py-2 text-sm rounded-full border border-gray-200 bg-white focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] transition-all w-full md:w-64"
            />
          </div>
        </div>
      </div>

      {/* Filter Tabs */}
      <div className="flex flex-wrap gap-2">
        {[
          { id: 'ALL', label: `All Files (${applications.length})` },
          { id: 'PENDING', label: `Awaiting Quorum (${applications.filter(a => a.stage === 'committee').length})` },
          { id: 'APPROVED', label: `Approved (${applications.filter(a => a.status === 'approved' || a.stage === 'disbursed').length})` },
          { id: 'DISBURSED', label: `Disbursed (${applications.filter(a => a.stage === 'disbursed').length})` },
        ].map((tab) => (
          <button
            key={tab.id}
            type="button"
            onClick={() => setFilter(tab.id as any)}
            className={`rounded-full px-4 py-1.5 text-xs font-semibold transition-all ${
              filter === tab.id
                ? 'bg-[#103a27] text-white shadow-sm'
                : 'bg-white text-gray-600 hover:bg-gray-100 border border-gray-200'
            }`}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {/* List */}
      {filteredApps.length === 0 ? (
        <div className="bg-white rounded-2xl p-12 text-center border-dashed border-2 border-gray-200 mt-4">
          <FileText className="size-10 mx-auto text-gray-300 mb-4" />
          <p className="text-base font-semibold text-gray-600">No applications match your filter.</p>
          <p className="text-sm text-gray-400 mt-2">Try clearing your search query or selecting a different category.</p>
        </div>
      ) : (
        <div className="grid gap-4 mt-2">
          {filteredApps.map((app) => {
            const votes = app.committeeVotes || [
              { id: 'v1', name: 'Chairperson', role: 'Chairperson', vote: 'APPROVE' },
              { id: 'v2', name: 'Risk Head', role: 'Risk Head', vote: 'APPROVE' },
              { id: 'v3', name: 'Credit Officer', role: 'Credit Officer', vote: 'APPROVE' },
              { id: 'v4', name: 'Treasurer', role: 'Treasurer', vote: 'ABSTAIN' },
              { id: 'v5', name: 'Board Member', role: 'Board Member', vote: 'ABSTAIN' },
            ]
            const approveCount = votes.filter((v) => v.vote === 'APPROVE').length
            const isApproved = approveCount >= 4 || app.status === 'approved' || app.stage === 'disbursed'

            return (
              <div 
                key={app.id} 
                className="border-none shadow-sm rounded-2xl bg-white overflow-hidden hover:shadow-md transition-shadow group p-5 md:p-6"
              >
                <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-6">
                  
                  {/* Left: Applicant Identity */}
                  <div className="flex items-start gap-4 min-w-[280px]">
                    <div className="size-12 rounded-xl bg-[#0d2a1c] text-[#a4cc44] flex items-center justify-center font-bold text-sm shrink-0 mt-0.5">
                      {app.applicantType === 'individual' ? 'BOSA' : 'SME'}
                    </div>
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-bold text-[#103a27] text-base">{app.fullName}</h4>
                        <span className="text-xs font-mono bg-gray-100 px-2 py-0.5 rounded text-gray-600 font-semibold">
                          {app.reference}
                        </span>
                      </div>
                      <p className="text-xs text-gray-500 mt-1">
                        Purpose: <span className="text-gray-700 font-medium">{app.purpose || 'Working Capital'}</span>
                      </p>
                      <div className="flex items-center gap-3 mt-2 text-xs text-gray-500">
                        <span>DTI: <strong className="text-gray-800">{app.dtiNetRatio?.toFixed(1) || '28.5'}%</strong></span>
                        <span>Multiplier: <strong className="text-gray-800">{app.multiplier || 3}x</strong></span>
                      </div>
                    </div>
                  </div>

                  {/* Middle: Who Has Approved Breakdown */}
                  <div className="flex-1 border-t lg:border-t-0 lg:border-l lg:border-r border-gray-100 pt-4 lg:pt-0 lg:px-6">
                    <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400 mb-2.5">
                      Board Sign-off Votes ({approveCount}/5 Approved)
                    </p>
                    <div className="flex flex-wrap gap-2">
                      {votes.map((v) => {
                        const isApp = v.vote === 'APPROVE'
                        const isRej = v.vote === 'REJECT'
                        return (
                          <div 
                            key={v.id || v.role}
                            className={`flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-medium border ${
                              isApp 
                                ? 'bg-emerald-50 border-emerald-200 text-emerald-800' 
                                : isRej 
                                  ? 'bg-rose-50 border-rose-200 text-rose-800' 
                                  : 'bg-gray-50 border-gray-200 text-gray-600'
                            }`}
                            title={`${v.name || v.role}: ${v.vote || 'PENDING'}`}
                          >
                            {isApp ? (
                              <CheckCircle2 className="size-3 text-emerald-600" />
                            ) : isRej ? (
                              <XCircle className="size-3 text-rose-600" />
                            ) : (
                              <Minus className="size-3 text-gray-400" />
                            )}
                            <span className="text-[0.7rem] font-semibold">{v.role}</span>
                          </div>
                        )
                      })}
                    </div>
                  </div>

                  {/* Right: Principal & Action */}
                  <div className="flex flex-row lg:flex-col items-center lg:items-end justify-between gap-4 shrink-0">
                    <div className="text-left lg:text-right">
                      <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Principal</p>
                      <p className="text-base font-bold text-[#103a27] font-mono">{formatUGX(app.principal || 0)}</p>
                      <span className={`inline-block mt-1 text-[0.65rem] font-bold uppercase px-2 py-0.5 rounded-full ${
                        isApproved ? 'bg-emerald-100 text-emerald-800' : 'bg-amber-100 text-amber-800'
                      }`}>
                        {isApproved ? 'Quorum Passed' : 'Pending Quorum'}
                      </span>
                    </div>

                    <button
                      onClick={() => onSelectApplication(app)}
                      className="flex items-center justify-center gap-2 px-5 py-2 rounded-xl bg-[#103a27] text-white text-xs font-bold hover:bg-[#124a31] transition-colors group-hover:bg-[#a4cc44] group-hover:text-[#0d2a1c]"
                    >
                      View Approval Details
                      <ArrowRight className="size-3.5" />
                    </button>
                  </div>
                  
                </div>
              </div>
            )
          })}
        </div>
      )}
    </div>
  )
}
