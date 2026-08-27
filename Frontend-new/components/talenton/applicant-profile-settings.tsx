'use client'

import { useState } from 'react'
import { 
  User, 
  Mail, 
  Phone, 
  MapPin, 
  Briefcase, 
  Wallet, 
  Sparkles, 
  CheckCircle2, 
  Save, 
  ShieldCheck 
} from 'lucide-react'
import type { UserProfile } from '@/lib/talenton-data'
import { formatUGX } from '@/lib/talenton-data'

export function ApplicantProfileSettings({
  profile,
  onSaveProfile,
}: {
  profile: UserProfile
  onSaveProfile: (updated: Partial<UserProfile>) => Promise<void>
}) {
  const [fullName, setFullName] = useState(profile.fullName || '')
  const [phone, setPhone] = useState(profile.phone || '')
  const [address, setAddress] = useState(profile.address || '')
  const [employer, setEmployer] = useState(profile.employerOrBusiness || '')
  const [income, setIncome] = useState<number | string>(profile.monthlyIncome || '')
  const [savedSuccess, setSavedSuccess] = useState(false)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    setLoading(true)
    await onSaveProfile({
      fullName,
      phone,
      address,
      employerOrBusiness: employer,
      monthlyIncome: Number(income) || 0,
    })
    setLoading(false)
    setSavedSuccess(true)
    setTimeout(() => setSavedSuccess(false), 3000)
  }

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      
      {/* Header */}
      <div className="pb-4 border-b border-gray-200">
        <h2 className="text-2xl font-serif font-bold text-[#103a27]">Account & Member Profile</h2>
        <p className="text-sm text-gray-500 mt-1">
          Manage your verified SACCO membership identity, employment status, and contact details.
        </p>
      </div>

      {/* OCR AI Autofill Notice Banner */}
      <div className="rounded-2xl border border-[#a4cc44]/40 bg-[#0d2a1c] p-5 text-white shadow-lg relative overflow-hidden">
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 relative z-10">
          <div className="flex items-start gap-3.5">
            <div className="p-2.5 rounded-xl bg-[#a4cc44]/20 text-[#a4cc44] shrink-0 mt-0.5">
              <Sparkles className="size-5" />
            </div>
            <div>
              <div className="flex items-center gap-2">
                <h4 className="font-bold text-base text-white">Smart OCR Form Autofill</h4>
                <span className="bg-[#a4cc44] text-[#0d2a1c] text-[0.65rem] font-extrabold uppercase px-2 py-0.5 rounded-full">
                  Upcoming Feature
                </span>
              </div>
              <p className="text-xs text-white/70 mt-1 leading-relaxed max-w-xl">
                Soon you will be able to simply scan your National ID or business license, and our AI vision engine will automatically extract and populate your application forms with zero manual typing.
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Profile Form */}
      <form onSubmit={handleSubmit} className="bg-white rounded-2xl p-6 sm:p-8 shadow-sm border border-gray-150 space-y-6">
        
        <div className="flex items-center justify-between pb-4 border-b border-gray-100">
          <div className="flex items-center gap-3">
            <div className="size-12 rounded-full bg-[#103a27]/10 text-[#103a27] flex items-center justify-center font-bold text-sm">
              <User className="size-6 text-[#103a27]" />
            </div>
            <div>
              <h3 className="font-bold text-base text-[#103a27]">{profile.fullName}</h3>
              <p className="text-xs text-gray-500 font-mono">SACCO Member ID: {profile.memberId}</p>
            </div>
          </div>
          <span className="inline-flex items-center gap-1 text-xs font-bold text-emerald-800 bg-emerald-50 px-3 py-1 rounded-full border border-emerald-200">
            <ShieldCheck className="size-3.5" />
            Active Member
          </span>
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          {/* Full Name */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <User className="size-3.5 text-gray-400" />
              Full Name (as on National ID)
            </label>
            <input
              type="text"
              value={fullName}
              onChange={(e) => setFullName(e.target.value)}
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-800 focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] bg-[#fdfdfd]"
              required
            />
          </div>

          {/* Email */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <Mail className="size-3.5 text-gray-400" />
              Email Address (Read-only)
            </label>
            <input
              type="email"
              value={profile.email}
              disabled
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-400 bg-gray-50 cursor-not-allowed"
            />
          </div>

          {/* Phone */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <Phone className="size-3.5 text-gray-400" />
              Mobile Phone Number
            </label>
            <input
              type="text"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="+256 701 000 000"
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-800 focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] bg-[#fdfdfd]"
            />
          </div>

          {/* Physical Address */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <MapPin className="size-3.5 text-gray-400" />
              Physical / Business Address
            </label>
            <input
              type="text"
              value={address}
              onChange={(e) => setAddress(e.target.value)}
              placeholder="e.g. Plot 14 Jinja Road, Kampala"
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-800 focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] bg-[#fdfdfd]"
            />
          </div>

          {/* Employer / Business */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <Briefcase className="size-3.5 text-gray-400" />
              Employer / Business Enterprise
            </label>
            <input
              type="text"
              value={employer}
              onChange={(e) => setEmployer(e.target.value)}
              placeholder="e.g. Grace Retail & General Supplies"
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-800 focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] bg-[#fdfdfd]"
            />
          </div>

          {/* Monthly Income */}
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-gray-700 flex items-center gap-1.5">
              <Wallet className="size-3.5 text-gray-400" />
              Declared Monthly Income (UGX)
            </label>
            <input
              type="number"
              value={income}
              onChange={(e) => setIncome(e.target.value)}
              placeholder="e.g. 2500000"
              className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27] focus:ring-1 focus:ring-[#103a27] bg-[#fdfdfd]"
            />
          </div>
        </div>

        {/* Action & Feedback */}
        <div className="flex items-center justify-between pt-4 border-t border-gray-100">
          <div>
            {savedSuccess && (
              <span className="inline-flex items-center gap-1.5 text-xs font-bold text-emerald-700 bg-emerald-50 px-3 py-1.5 rounded-xl border border-emerald-200 animate-fadeIn">
                <CheckCircle2 className="size-4" />
                Profile updated successfully!
              </span>
            )}
          </div>

          <button
            type="submit"
            disabled={loading}
            className="flex items-center gap-2 rounded-xl bg-[#103a27] text-white px-6 py-2.5 text-xs font-bold shadow-md hover:bg-[#1a5235] transition-all cursor-pointer"
          >
            <Save className="size-3.5" />
            {loading ? 'Saving...' : 'Save Profile Changes'}
          </button>
        </div>

      </form>
    </div>
  )
}
