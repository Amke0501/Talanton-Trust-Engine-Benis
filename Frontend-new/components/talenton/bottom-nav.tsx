'use client'

import { Home, Plus } from 'lucide-react'
import { cn } from '@/lib/utils'

type NavView = 'dashboard' | 'apply'

export function BottomNav({
  active,
  onNavigate,
}: {
  active: NavView
  onNavigate: (view: NavView) => void
}) {
  return (
    <nav className="pointer-events-none fixed inset-x-0 bottom-5 z-50 flex justify-center px-4">
      <div className="pointer-events-auto flex items-center gap-1.5 rounded-full border border-glass-border bg-card/80 p-1.5 shadow-[0_20px_60px_-16px_oklch(0.28_0.07_150_/_0.55)] backdrop-blur-xl">
        <button
          type="button"
          onClick={() => onNavigate('dashboard')}
          aria-current={active === 'dashboard' ? 'page' : undefined}
          className={cn(
            'inline-flex items-center gap-2 rounded-full px-4 py-2.5 text-sm font-medium transition-colors',
            active === 'dashboard'
              ? 'bg-secondary text-secondary-foreground'
              : 'text-foreground/70 hover:bg-accent/50 hover:text-foreground',
          )}
        >
          <Home className="size-4" />
          <span className="hidden sm:inline">Home</span>
        </button>

        <button
          type="button"
          onClick={() => onNavigate('apply')}
          aria-current={active === 'apply' ? 'page' : undefined}
          className={cn(
            'inline-flex items-center gap-2 rounded-full px-5 py-2.5 text-sm font-semibold shadow-sm transition-colors',
            active === 'apply'
              ? 'bg-primary text-primary-foreground'
              : 'bg-primary text-primary-foreground hover:bg-primary/90',
          )}
        >
          <Plus className="size-4" />
          New application
        </button>
      </div>
    </nav>
  )
}
