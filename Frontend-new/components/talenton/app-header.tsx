import { Scale } from 'lucide-react'

export function AppHeader({
  userName,
  userMeta,
}: {
  userName: string
  userMeta: string
}) {
  const initials = userName
    .split(' ')
    .map((p) => p[0])
    .filter(Boolean)
    .slice(0, 2)
    .join('')
    .toUpperCase()

  return (
    <header className="mx-auto flex max-w-5xl items-center justify-between gap-4 px-4 py-5 sm:px-6">
      <div className="flex items-center gap-3">
        <span className="flex size-10 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-sm">
          <Scale className="size-5" />
        </span>
        <div>
          <p className="font-serif text-lg font-semibold leading-none tracking-tight text-foreground">
            Talanton
          </p>
          <p className="mt-1 text-[0.7rem] font-medium uppercase tracking-widest text-muted-foreground">
            SACCO Credit Pipeline
          </p>
        </div>
      </div>

      <div className="flex items-center gap-3 rounded-full border border-glass-border bg-card px-2 py-1.5 pr-4 shadow-sm backdrop-blur-xl">
        <span className="flex size-8 items-center justify-center rounded-full bg-accent text-sm font-semibold text-accent-foreground">
          {initials || 'A'}
        </span>
        <div className="hidden text-left leading-tight sm:block">
          <p className="text-sm font-semibold text-foreground">{userName}</p>
          <p className="text-xs text-muted-foreground">{userMeta}</p>
        </div>
      </div>
    </header>
  )
}
