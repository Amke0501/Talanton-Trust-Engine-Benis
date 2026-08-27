'use client'

import { Download, Scale, ShieldCheck, UserCheck, Vote } from 'lucide-react'
import { STAGES, type RoleType, type StageKey } from '@/lib/talenton-data'
import { cn } from '@/lib/utils'

export function RoleHeader({
  activeRole,
  currentStage = 'verification',
  fileReference = 'LA-2026-0941A',
  title,
  subtitle,
  onRoleChange,
  showRoleSwitcher = true,
}: {
  activeRole: RoleType
  onRoleChange?: (role: RoleType) => void
  currentStage?: StageKey
  fileReference?: string
  title: string
  subtitle: string
  showRoleSwitcher?: boolean
}) {
  const roles: { id: RoleType; label: string; icon: typeof UserCheck }[] = [
    { id: 'applicant', label: '1. Applicant', icon: UserCheck },
    { id: 'underwriter', label: '2. Underwriter', icon: ShieldCheck },
    { id: 'committee', label: '3. Committee', icon: Vote },
  ]

  return (
    <div className="space-y-6">
      {/* Main Top Header Bar */}
      <header className="flex flex-wrap items-center justify-between gap-3 rounded-2xl bg-[#0d2a1c] px-4 py-4 text-white shadow-xl border border-white/10 backdrop-blur-xl sm:gap-4 sm:px-6">
        {/* Brand */}
        <div className="flex items-center gap-3">
          <span className="flex size-10 items-center justify-center rounded-xl bg-[#a4cc44] text-[#103a27] shadow-md">
            <Scale className="size-5" />
          </span>
          <div>
            <h1 className="font-serif text-lg font-bold tracking-tight text-white">
              Talanton Trust Engine
            </h1>
            <p className="text-[0.65rem] font-bold uppercase tracking-widest text-[#a4cc44]">
              SACCO & SME CREDIT PIPELINE
            </p>
          </div>
        </div>

        {showRoleSwitcher ? (
          <div className="flex w-full items-center gap-1.5 overflow-x-auto rounded-full bg-[#103a27]/80 p-1.5 border border-white/10 sm:w-auto">
            {roles.map((r) => {
              const isActive = activeRole === r.id
              return (
                <button
                  key={r.id}
                  type="button"
                  onClick={() => onRoleChange?.(r.id)}
                  className={cn(
                    'flex shrink-0 items-center gap-2 rounded-full px-3 py-1.5 text-xs font-bold transition-all duration-200 sm:px-4',
                    isActive
                      ? 'bg-[#a4cc44] text-[#103a27] shadow-md'
                      : 'text-white/70 hover:bg-white/10 hover:text-white',
                  )}
                >
                  <r.icon className="size-3.5" />
                  {r.label}
                </button>
              )
            })}
          </div>
        ) : (
          <div className="inline-flex items-center gap-2 rounded-full border border-white/10 bg-[#103a27]/80 px-3 py-1.5 text-xs font-bold uppercase tracking-wide text-[#a4cc44]">
            Current Role
            <span className="rounded-full bg-[#a4cc44] px-2 py-0.5 text-[#103a27]">
              {activeRole}
            </span>
          </div>
        )}

        {/* Engine Status */}
        <div className="flex items-center gap-2">
          <div className="hidden sm:flex items-center gap-2 rounded-full bg-emerald-500/20 px-3.5 py-1.5 text-xs font-semibold text-emerald-400 border border-emerald-500/30">
            <span className="size-2 rounded-full bg-emerald-400 animate-pulse" />
            Unified Engine Active
          </div>
          <a
            href="/logout"
            className="inline-flex items-center rounded-full border border-white/15 bg-white/5 px-3 py-1.5 text-xs font-semibold text-white hover:bg-white/10"
          >
            Logout
          </a>
        </div>
      </header>

      {/* View Title & File Reference Bar */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between px-1">
        <div>
          <h2 className="font-serif text-2xl font-bold tracking-tight text-[#103a27] sm:text-3xl">
            {title}
          </h2>
          <p className="mt-1 text-sm text-[#2a5040]/80">
            {subtitle}
          </p>
        </div>

        <div className="flex w-full flex-col items-stretch gap-2 sm:w-auto sm:flex-row sm:items-center sm:gap-3">
          <div className="flex items-center gap-2 rounded-xl border border-[#103a27]/20 bg-white px-3.5 py-2 text-xs font-medium text-[#103a27] shadow-sm">
            <span className="text-muted-foreground">File Reference:</span>
            <span className="truncate font-mono font-bold text-[#103a27]">{fileReference}</span>
          </div>
          <button
            type="button"
            onClick={() => alert(`Exporting report for ${fileReference}...`)}
            className="flex w-full items-center justify-center gap-2 rounded-xl border border-[#103a27]/20 bg-white px-3.5 py-2 text-xs font-semibold text-[#103a27] shadow-sm hover:bg-[#eaf4e5] transition-colors sm:w-auto"
          >
            <Download className="size-3.5" />
            Export
          </button>
        </div>
      </div>

      {/* Stage Tracker */}
      <div className="rounded-2xl border border-black/5 bg-white p-4 shadow-sm">
        <div className="flex flex-col items-start justify-between gap-2 pb-3 border-b border-border/60 sm:flex-row sm:items-center">
          <div className="flex items-center gap-2">
            <span className="flex size-6 items-center justify-center rounded-full bg-[#103a27]/10 text-[#103a27]">
              <Scale className="size-3" />
            </span>
            <span className="text-xs font-bold uppercase tracking-wider text-[#103a27]">
              APPRAISAL STAGE TRACKER
            </span>
          </div>
          <span className="text-xs font-medium text-muted-foreground">
            Current Phase: <strong className="text-[#103a27]">Documents Uploaded & Auditing</strong>
          </span>
        </div>

        <div className="mt-4 grid grid-cols-2 gap-2 sm:grid-cols-3 lg:grid-cols-5">
          {STAGES.map((s, idx) => {
            const isCompleted = idx <= 1 // verification is stage 2
            const isCurrent = s.key === currentStage
            return (
              <div
                key={s.key}
                className={cn(
                  'flex flex-col items-center rounded-xl p-2 text-center transition-all',
                  isCurrent
                    ? 'bg-[#103a27] text-white ring-2 ring-[#a4cc44]'
                    : isCompleted
                      ? 'bg-[#eaf4e5] text-[#103a27]'
                      : 'bg-muted/40 text-muted-foreground',
                )}
              >
                <span className="text-xs font-bold">{s.label}</span>
              </div>
            )
          })}
        </div>
      </div>
    </div>
  )
}
