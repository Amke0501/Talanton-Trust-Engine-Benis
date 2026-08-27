'use client'

import { useMemo, useState } from 'react'
import { LayoutGrid, FileText, Menu, Settings, UserCircle, X, Users } from 'lucide-react'
import { cn } from '@/lib/utils'

export type NavItem = 'home' | 'applications' | 'settings' | 'profile' | 'creditors'

const NAV_ITEMS: { id: NavItem; icon: typeof LayoutGrid; label: string }[] = [
  { id: 'home', icon: LayoutGrid, label: 'Home' },
  { id: 'applications', icon: FileText, label: 'Applications' },
  { id: 'creditors', icon: Users, label: 'Creditors' },
  { id: 'settings', icon: Settings, label: 'Settings' },
  { id: 'profile', icon: UserCircle, label: 'Profile' },
]

export function FloatingNav({
  active,
  onNavigate,
  allowedItems,
  labelOverrides,
}: {
  active: NavItem
  onNavigate: (item: NavItem) => void
  allowedItems?: NavItem[]
  labelOverrides?: Partial<Record<NavItem, string>>
}) {
  const [mobileOpen, setMobileOpen] = useState(false)

  const visibleNavItems = useMemo(() => {
    if (!allowedItems || allowedItems.length === 0) return NAV_ITEMS
    return NAV_ITEMS.filter((item) => allowedItems.includes(item.id))
  }, [allowedItems])

  function getLabel(item: (typeof NAV_ITEMS)[number]) {
    return labelOverrides?.[item.id] ?? item.label
  }

  function handleNavigate(item: NavItem) {
    onNavigate(item)
    setMobileOpen(false)
  }

  return (
    <>
      <nav className="pointer-events-none fixed inset-x-0 bottom-8 z-50 hidden justify-center px-4 sm:flex">
        <div className="pointer-events-auto flex w-full max-w-6xl items-center gap-1 overflow-x-auto rounded-full border border-black/5 bg-white/90 p-1.5 shadow-2xl backdrop-blur-xl">
        {/* Logo */}
        <div className="flex items-center justify-center px-3">
          <span className="font-serif text-lg font-bold text-[#103a27]">
            T<span className="text-[#a4cc44]">.</span>
          </span>
        </div>

        <div className="w-px h-5 bg-black/10" />

        {/* Nav items */}
        {visibleNavItems.map((item) => {
          const isActive = active === item.id
          const label = getLabel(item)
          return (
            <button
              key={item.id}
              type="button"
              onClick={() => handleNavigate(item.id)}
              aria-current={isActive ? 'page' : undefined}
              aria-label={label}
              className={cn(
                'group flex shrink-0 items-center gap-2 rounded-full px-3 py-2.5 transition-all duration-300 lg:px-4',
                isActive
                  ? 'bg-[#103a27] text-white'
                  : 'text-[#103a27]/60 hover:bg-[#dbead5] hover:text-[#103a27]',
              )}
            >
              <item.icon className="h-[18px] w-[18px] shrink-0" strokeWidth={2.5} />
              <span className="text-xs font-semibold whitespace-nowrap">
                {label}
              </span>
            </button>
          )
        })}
      </div>
      </nav>

      <div className="fixed bottom-5 right-4 z-50 sm:hidden">
        <button
          type="button"
          onClick={() => setMobileOpen((prev) => !prev)}
          aria-label={mobileOpen ? 'Close navigation menu' : 'Open navigation menu'}
          className="flex h-12 w-12 items-center justify-center rounded-full border border-black/10 bg-white text-[#103a27] shadow-xl"
        >
          {mobileOpen ? <X className="size-5" /> : <Menu className="size-5" />}
        </button>
      </div>

      {mobileOpen && (
        <div className="fixed inset-0 z-40 bg-black/30 sm:hidden" onClick={() => setMobileOpen(false)}>
          <div
            className="absolute inset-x-0 bottom-0 rounded-t-2xl border border-black/10 bg-white p-4 shadow-2xl"
            onClick={(event) => event.stopPropagation()}
          >
            <div className="mb-3 flex items-center justify-between">
              <p className="text-sm font-bold text-[#103a27]">Navigation</p>
              <button
                type="button"
                onClick={() => setMobileOpen(false)}
                className="rounded-full border border-border px-3 py-1 text-xs font-semibold text-muted-foreground"
              >
                Close
              </button>
            </div>
            <div className="grid grid-cols-1 gap-2">
              {visibleNavItems.map((item) => {
                const isActive = active === item.id
                const label = getLabel(item)
                return (
                  <button
                    key={item.id}
                    type="button"
                    onClick={() => handleNavigate(item.id)}
                    aria-current={isActive ? 'page' : undefined}
                    className={cn(
                      'flex w-full items-center justify-between rounded-xl border px-3 py-3 text-left text-sm font-semibold',
                      isActive
                        ? 'border-[#103a27] bg-[#103a27] text-white'
                        : 'border-border bg-white text-[#103a27]',
                    )}
                  >
                    <span>{label}</span>
                    <item.icon className="size-4" strokeWidth={2.5} />
                  </button>
                )
              })}
            </div>
          </div>
        </div>
      )}
    </>
  )
}
