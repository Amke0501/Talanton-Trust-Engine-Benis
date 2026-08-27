'use client'

import { useEffect, useState } from 'react'
import { Search, UserPlus, Eye, ArrowUpRight, ShieldCheck } from 'lucide-react'
import {
  formatUGX,
  SEED_PASSPORT_MEMBERS,
  type CreditPassportMember,
} from '@/lib/talenton-data'
import { fetchCreditPassportMembers } from '@/lib/api-service'

export function CreditPassportPanel({ sectionId = 'credit-passport-section' }: { sectionId?: string }) {
  const [passportMembers, setPassportMembers] = useState<CreditPassportMember[]>(SEED_PASSPORT_MEMBERS)
  const [searchQuery, setSearchQuery] = useState('')

  useEffect(() => {
    let isMounted = true

    async function loadPassportMembers() {
      const members = await fetchCreditPassportMembers()
      if (isMounted) {
        setPassportMembers(Array.isArray(members) && members.length > 0 ? members : SEED_PASSPORT_MEMBERS)
      }
    }

    loadPassportMembers()

    return () => {
      isMounted = false
    }
  }, [])

  const avgTrustScore =
    passportMembers.length > 0
      ? Math.round(
          passportMembers.reduce((sum, member) => sum + (Number(member.trustScore) || 0), 0) /
            passportMembers.length
        )
      : 0

  const totalRepaidToDate = passportMembers.reduce(
    (sum, member) => sum + (Number(member.totalRepaid) || 0),
    0
  )

  const filteredPassport = passportMembers.filter(
    (member) =>
      member.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      member.memberId.toLowerCase().includes(searchQuery.toLowerCase())
  )

  return (
    <div id={sectionId} className="space-y-6">

      {/* Page Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h2 className="text-2xl font-serif font-bold text-[#103a27]">TCredit Passport</h2>
          <p className="text-sm text-gray-500 mt-1">
            Verified good creditors — repayment history visible to every credit officer in the network.
          </p>
        </div>

        <button className="flex items-center gap-2 px-5 py-2.5 rounded-full bg-[#a4cc44] text-[#0d2a1c] text-sm font-bold hover:bg-[#b5d956] transition-colors shadow-sm w-full md:w-auto justify-center">
          <UserPlus className="size-4" />
          Onboard Member
        </button>
      </div>

      {/* KPI Strip */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="rounded-2xl bg-[#0d2a1c] p-5 text-white">
          <p className="text-[0.6rem] font-bold uppercase tracking-widest text-[#a4cc44]">Passport Holders</p>
          <p className="text-3xl font-extrabold mt-2">{passportMembers.length}</p>
          <p className="text-xs text-white/50 mt-1">Active in network</p>
        </div>
        <div className="rounded-2xl bg-[#103a27] p-5 text-white">
          <p className="text-[0.6rem] font-bold uppercase tracking-widest text-emerald-400">Avg Trust Score</p>
          <p className="text-3xl font-extrabold mt-2">{avgTrustScore}<span className="text-base font-bold text-white/40">/100</span></p>
          <p className="text-xs text-white/50 mt-1">Network average</p>
        </div>
        <div className="rounded-2xl bg-[#1a4d35] p-5 text-white">
          <p className="text-[0.6rem] font-bold uppercase tracking-widest text-[#a4cc44]">Repaid to Date</p>
          <p className="text-2xl font-extrabold mt-2">{formatUGX(totalRepaidToDate)}</p>
          <p className="text-xs text-white/50 mt-1">Cumulative repayments</p>
        </div>
      </div>

      {/* Search */}
      <div className="relative">
        <Search className="absolute left-4 top-3.5 size-4 text-gray-400" />
        <input
          type="text"
          placeholder="Search by member name or ID..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="w-full rounded-xl bg-white pl-11 pr-4 py-3 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-[#103a27] transition-all shadow-sm"
        />
      </div>

      {/* Member Cards Grid */}
      <div className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
        {filteredPassport.map((member) => (
          <div
            key={member.id}
            className="rounded-2xl bg-[#0d2a1c] p-5 space-y-4 hover:bg-[#0f3020] transition-colors group"
          >
            {/* Header */}
            <div className="flex items-start justify-between">
              <div className="flex items-center gap-3">
                <span className="flex size-11 items-center justify-center rounded-xl bg-[#a4cc44]/20 text-[#a4cc44] font-bold text-sm">
                  {member.name.split(' ').map((part) => part[0]).join('').slice(0, 2)}
                </span>
                <div>
                  <p className="font-bold text-sm text-white">{member.name}</p>
                  <p className="text-[0.65rem] text-white/40">
                    {member.memberId} • {member.classification}
                  </p>
                </div>
              </div>
              <span
                className={`rounded-full px-2.5 py-0.5 text-[0.6rem] font-bold ${
                  member.tier === 'PLATINUM'
                    ? 'bg-emerald-500/20 text-emerald-400'
                    : member.tier === 'GOLD'
                      ? 'bg-amber-500/20 text-amber-400'
                      : 'bg-slate-500/20 text-slate-400'
                }`}
              >
                {member.tier}
              </span>
            </div>

            {/* Trust Score & On-Time Rate */}
            <div className="grid grid-cols-2 gap-3">
              <div className="rounded-xl bg-white/5 p-3 text-center">
                <span className="text-[0.6rem] text-white/40 block uppercase font-bold tracking-wider">
                  Trust Score
                </span>
                <span className="text-xl font-extrabold text-[#a4cc44]">{member.trustScore}</span>
                <span className="text-[0.6rem] text-white/30">/100</span>
              </div>
              <div className="rounded-xl bg-white/5 p-3 text-center">
                <span className="text-[0.6rem] text-white/40 block uppercase font-bold tracking-wider">
                  On-Time Rate
                </span>
                <span className="text-xl font-extrabold text-emerald-400">{member.onTimeRatePct}%</span>
              </div>
            </div>

            {/* Stats */}
            <div className="space-y-2 text-xs">
              <div className="flex justify-between text-white/40 text-[0.65rem]">
                <span>Loans completed:</span>
                <span className="font-bold text-white/80">{member.loansCompleted}</span>
              </div>
              <div className="flex justify-between text-white/40 text-[0.65rem]">
                <span>Total repaid:</span>
                <span className="font-mono font-bold text-white/80">{formatUGX(member.totalRepaid)}</span>
              </div>
              <div className="flex justify-between text-white/40 text-[0.65rem]">
                <span>Current limit:</span>
                <span className="font-mono font-bold text-[#a4cc44]">{formatUGX(member.currentLimit)}</span>
              </div>
              <div className="flex justify-between text-white/40 text-[0.65rem]">
                <span>Last loan:</span>
                <span className="font-bold text-white/80">{member.lastLoanDate}</span>
              </div>
            </div>

            {/* Progress bar */}
            <div className="w-full h-1.5 rounded-full bg-white/10 overflow-hidden">
              <div
                className="h-full rounded-full bg-gradient-to-r from-[#a4cc44] to-emerald-400 transition-all"
                style={{ width: `${member.onTimeRatePct}%` }}
              />
            </div>

            {/* Action Buttons */}
            <div className="flex gap-2 pt-1">
              <button className="flex-1 flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl bg-[#a4cc44]/15 text-[#a4cc44] text-xs font-bold hover:bg-[#a4cc44]/25 transition-colors">
                <Eye className="size-3.5" />
                View Profile
              </button>
              <button className="flex-1 flex items-center justify-center gap-1.5 px-3 py-2 rounded-xl bg-white/10 text-white/80 text-xs font-bold hover:bg-white/15 transition-colors">
                <ArrowUpRight className="size-3.5" />
                Audit Trail
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}