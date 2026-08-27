'use client'

import { Users, Building2, TrendingUp, ShieldCheck, ArrowRight, ExternalLink } from 'lucide-react'
import { formatUGX } from '@/lib/talenton-data'

const MOCK_CREDITORS = [
  { id: '1', name: 'MicroLend Africa', type: 'Institutional', exposure: 125000000, status: 'Active', rating: 'AAA' },
  { id: '2', name: 'EastCoop Financial', type: 'Cooperative', exposure: 45000000, status: 'Active', rating: 'AA' },
  { id: '3', name: 'Global Impact Fund', type: 'Investment Fund', exposure: 300000000, status: 'Active', rating: 'AAA' },
  { id: '4', name: 'Kampa Credit Union', type: 'Credit Union', exposure: 15000000, status: 'Onboarding', rating: 'Pending' },
]

export function CreditorsView() {
  const totalExposure = MOCK_CREDITORS.reduce((acc, c) => acc + c.exposure, 0)

  return (
    <div className="space-y-6 max-w-5xl mx-auto">
      
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-end justify-between gap-4 pb-4 border-b border-gray-200">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">Creditors Network</h2>
          <p className="text-sm text-gray-500 mt-1">
            Manage institutional partners, funds, and liquidity providers.
          </p>
        </div>
        
        <button className="flex items-center gap-2 px-5 py-2.5 rounded-full bg-[#103a27] text-white text-sm font-bold hover:bg-[#124a31] transition-colors shadow-sm w-full md:w-auto justify-center">
          <Building2 className="size-4" />
          Onboard Creditor
        </button>
      </div>

      {/* KPI Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-6">
        <div className="p-5 border-none shadow-sm rounded-2xl bg-[#0d2a1c] text-white flex flex-col justify-between min-h-[130px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-[#a4cc44]">Total Network Liquidity</p>
            <span className="p-2 bg-white/10 rounded-xl text-[#a4cc44]">
              <TrendingUp className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-3xl font-extrabold">{formatUGX(totalExposure)}</h3>
            <p className="text-xs text-gray-400 mt-1">Deployed capital across network</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-white flex flex-col justify-between min-h-[130px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-gray-400">Active Partners</p>
            <span className="p-2 bg-[#a4cc44]/10 rounded-xl text-[#a4cc44]">
              <Users className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-3xl font-extrabold text-[#103a27]">{MOCK_CREDITORS.filter(c => c.status === 'Active').length}</h3>
            <p className="text-xs text-gray-500 mt-1">Institutional providers</p>
          </div>
        </div>

        <div className="p-5 border-none shadow-sm rounded-2xl bg-white flex flex-col justify-between min-h-[130px]">
          <div className="flex items-center justify-between">
            <p className="text-xs font-bold uppercase tracking-widest text-gray-400">Trust Index</p>
            <span className="p-2 bg-emerald-50 rounded-xl text-emerald-600">
              <ShieldCheck className="size-4" />
            </span>
          </div>
          <div className="mt-4">
            <h3 className="text-3xl font-extrabold text-[#103a27]">99.8%</h3>
            <p className="text-xs text-gray-500 mt-1">Network repayment rate</p>
          </div>
        </div>
      </div>

      {/* Creditors List */}
      <div className="mt-8 bg-white rounded-2xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b bg-gray-50/50">
          <h3 className="text-sm font-bold uppercase tracking-widest text-[#103a27]">Registered Providers</h3>
        </div>
        
        <div className="divide-y">
          {MOCK_CREDITORS.map((creditor) => (
            <div key={creditor.id} className="p-5 sm:p-6 flex flex-col md:flex-row md:items-center justify-between gap-4 hover:bg-gray-50/50 transition-colors group">
              <div className="flex items-center gap-4">
                <div className="size-12 rounded-xl bg-gray-100 border text-gray-400 flex items-center justify-center shrink-0">
                  <Building2 className="size-6" />
                </div>
                <div>
                  <h4 className="font-bold text-[#103a27] text-base flex items-center gap-2">
                    {creditor.name}
                    {creditor.status === 'Active' && (
                      <span className="size-2 rounded-full bg-emerald-500" title="Active"></span>
                    )}
                  </h4>
                  <p className="text-xs font-semibold text-gray-500 mt-0.5">{creditor.type}</p>
                </div>
              </div>
              
              <div className="flex flex-1 md:justify-center gap-8 md:px-8 border-t md:border-t-0 pt-4 md:pt-0">
                <div>
                  <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Capital Exposure</p>
                  <p className="text-sm font-bold text-[#103a27] mt-1">{formatUGX(creditor.exposure)}</p>
                </div>
                <div>
                  <p className="text-[0.65rem] font-bold uppercase tracking-widest text-gray-400">Risk Rating</p>
                  <p className="text-sm font-bold text-gray-700 mt-1">{creditor.rating}</p>
                </div>
              </div>

              <div className="flex justify-end mt-2 md:mt-0">
                <button className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-semibold text-gray-500 hover:bg-gray-100 hover:text-[#103a27] transition-colors">
                  Details
                  <ExternalLink className="size-4" />
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
      
    </div>
  )
}
