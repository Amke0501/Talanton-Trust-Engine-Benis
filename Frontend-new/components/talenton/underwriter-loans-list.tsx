'use client'

import { FileText, ArrowRight, Search, Filter } from 'lucide-react'
import type { Application } from '@/lib/talenton-data'
import { formatUGX } from '@/lib/talenton-data'

export function UnderwriterLoansList({
  applications,
  onSelectApplication,
}: {
  applications: Application[]
  onSelectApplication: (app: Application) => void
}) {
  const pendingApps = applications.filter(
    (a) => a.status !== 'draft' && a.stage !== 'draft' && a.stage !== 'committee' && a.stage !== 'disbursed'
  )

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-4 border-b border-gray-200">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Loan Reviews Queue</h2>
          <p className="text-sm text-gray-500 mt-1">
            Applications awaiting underwriting approval and routing.
          </p>
        </div>
        
        <div className="flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 size-4 text-gray-400" />
            <input 
              type="text" 
              placeholder="Search reference..." 
              className="pl-9 pr-4 py-2 text-sm rounded-full border border-gray-200 bg-white focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] transition-all w-full md:w-64"
            />
          </div>
          <button className="p-2 rounded-full border border-gray-200 bg-white hover:bg-gray-50 text-gray-600 transition-colors">
            <Filter className="size-4" />
          </button>
        </div>
      </div>

      {/* List */}
      {pendingApps.length === 0 ? (
        <div className="bg-white rounded-2xl p-12 text-center border-dashed border-2 border-gray-200 mt-8">
          <FileText className="size-10 mx-auto text-gray-300 mb-4" />
          <p className="text-base font-semibold text-gray-600">No applications awaiting review.</p>
          <p className="text-sm text-gray-400 mt-2">Check back later or review verified applications.</p>
        </div>
      ) : (
        <div className="grid gap-4 mt-6">
          {pendingApps.map((app) => (
            <div key={app.id} className="p-0 border-none shadow-sm rounded-2xl bg-white overflow-hidden hover:shadow-md transition-shadow group">
              <div className="flex flex-col md:flex-row md:items-center p-5 md:p-6 gap-4 md:gap-6">
                
                {/* Meta */}
                <div className="flex items-center gap-4 min-w-[240px]">
                  <div className="size-12 rounded-xl bg-[#a4cc44]/10 text-[#0d2a1c] flex items-center justify-center font-bold text-sm shrink-0">
                    {app.applicantType === 'individual' ? 'IND' : 'SME'}
                  </div>
                  <div>
                    <h4 className="font-bold text-[#103a27] text-base">{app.fullName}</h4>
                    <p className="text-xs text-gray-500 mt-1 font-mono bg-gray-100 inline-block px-2 py-0.5 rounded-md">{app.reference}</p>
                  </div>
                </div>

                {/* Financials */}
                <div className="flex-1 grid grid-cols-2 md:grid-cols-3 gap-4 border-t md:border-t-0 md:border-l border-gray-100 pt-4 md:pt-0 md:pl-6">
                  <div>
                    <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Principal</p>
                    <p className="text-sm font-bold text-[#103a27] mt-1">{formatUGX(app.principal || 0)}</p>
                  </div>
                  <div>
                    <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">DTI Ratio</p>
                    <p className="text-sm font-bold text-amber-600 mt-1">
                      {app.monthlyIncome && app.monthlyDebt 
                        ? `${Math.round((app.monthlyDebt / app.monthlyIncome) * 100)}%` 
                        : 'N/A'}
                    </p>
                  </div>
                  <div className="hidden md:block">
                    <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Tenure</p>
                    <p className="text-sm font-bold text-[#103a27] mt-1">{app.tenureMonths || 12} Mos</p>
                  </div>
                </div>

                {/* Action */}
                <div className="flex justify-end mt-4 md:mt-0">
                  <button
                    onClick={() => onSelectApplication(app)}
                    className="flex items-center justify-center gap-2 w-full md:w-auto px-6 py-2.5 rounded-xl bg-[#103a27] text-white text-sm font-bold hover:bg-[#124a31] transition-colors group-hover:bg-[#a4cc44] group-hover:text-[#0d2a1c]"
                  >
                    Open Audit
                    <ArrowRight className="size-4" />
                  </button>
                </div>
                
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}
