'use client'

import Link from 'next/link'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  FileText,
  Users,
  Settings,
  LogOut,
  UserCircle
} from 'lucide-react'
import type { NavItem } from '@/components/talenton/floating-nav'

const NAV_ITEMS: { id: NavItem; label: string; icon: typeof LayoutDashboard }[] = [
  { id: 'home',         label: 'Dashboard',      icon: LayoutDashboard },
  { id: 'applications', label: 'Loan Reviews',   icon: FileText },
  { id: 'creditors',    label: 'Credit Passport',icon: Users },
]

export function UnderwriterSidebar({
  active = 'home',
  userName = 'Underwriter',
  onNavigate,
}: {
  active?: NavItem
  userName?: string
  onNavigate?: (section: NavItem) => void
}) {
  const initials = userName
    .split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase()
    .slice(0, 2)

  return (
    <aside className="hidden lg:flex flex-col w-60 xl:w-64 shrink-0 min-h-screen bg-[#0d2a1c] text-white">

      {/* Brand */}
      <div className="px-6 pt-7 pb-6 border-b border-white/10">
        <div className="font-serif text-2xl font-bold text-white tracking-tight">Talanton.</div>
        <p className="mt-1 text-[0.6rem] font-bold uppercase tracking-widest text-[#a4cc44]">
          Trust Audit Engine
        </p>
      </div>

      {/* Navigation */}
      <nav className="flex-1 px-3 py-4 space-y-0.5 overflow-y-auto">
      {NAV_ITEMS.map((item) => {
          const isActive = item.id === active
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onNavigate?.(item.id)}
              className={cn(
                'w-full flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition-all duration-150 text-left',
                isActive
                  ? 'bg-white text-[#103a27]'
                  : 'text-white/55 hover:bg-white/8 hover:text-white/85',
              )}
            >
              <item.icon
                className={cn('size-4 shrink-0', isActive ? 'text-[#103a27]' : 'text-white/45')}
                strokeWidth={isActive ? 2.5 : 2}
              />
              {item.label}
            </button>
          )
        })}
      </nav>

      {/* User + Logout */}
      <div className="border-t border-white/10 px-4 py-4 space-y-3">
        <div className="flex items-center gap-3">
          <span className="flex size-9 shrink-0 items-center justify-center rounded-full bg-[#a4cc44] text-[#0d2a1c] text-xs font-black">
            {initials}
          </span>
          <div className="min-w-0">
            <p className="truncate text-sm font-semibold text-white">{userName}</p>
            <p className="text-xs text-white/50">Underwriter</p>
          </div>
        </div>

        <Link
          href="/logout"
          className="flex items-center gap-2 rounded-lg px-3 py-2 text-xs font-medium text-white/50 hover:bg-white/8 hover:text-white/80 transition-colors"
        >
          <LogOut className="size-3.5" />
          Logout
        </Link>
      </div>
    </aside>
  )
}
