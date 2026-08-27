import { Check } from 'lucide-react'
import { STAGES, stageIndex, type StageKey } from '@/lib/talenton-data'
import { cn } from '@/lib/utils'

export function StageTracker({
  stage,
  declined = false,
}: {
  stage: StageKey
  declined?: boolean
}) {
  const current = stageIndex(stage)

  return (
    <ol className="flex flex-col gap-3 sm:flex-row sm:items-start sm:gap-0">
      {STAGES.map((s, i) => {
        const done = i < current
        const active = i === current
        const isDeclined = declined && active

        return (
          <li
            key={s.key}
            className="relative flex items-start gap-3 sm:flex-1 sm:flex-col sm:items-center sm:text-center"
          >
            {/* connector */}
            {i < STAGES.length - 1 ? (
              <span
                aria-hidden="true"
                className={cn(
                  'absolute left-[15px] top-8 h-[calc(100%-1rem)] w-0.5 sm:left-auto sm:top-4 sm:h-0.5 sm:w-full sm:translate-x-1/2',
                  done ? 'bg-primary' : 'bg-border',
                )}
              />
            ) : null}

            <span
              className={cn(
                'relative z-10 flex size-8 shrink-0 items-center justify-center rounded-full border-2 text-xs font-semibold',
                done && 'border-primary bg-primary text-primary-foreground',
                active &&
                  !isDeclined &&
                  'border-primary bg-background text-primary',
                active &&
                  isDeclined &&
                  'border-destructive bg-background text-destructive',
                !done && !active && 'border-border bg-background text-muted-foreground',
              )}
            >
              {done ? <Check className="size-4" /> : i + 1}
            </span>

            <div className="pt-0.5 sm:pt-2">
              <p
                className={cn(
                  'text-sm font-semibold leading-none',
                  active && !isDeclined && 'text-primary',
                  isDeclined && 'text-destructive',
                  !active && 'text-foreground',
                )}
              >
                {s.label}
              </p>
              <p className="mt-1 text-xs leading-snug text-muted-foreground">
                {s.caption}
              </p>
            </div>
          </li>
        )
      })}
    </ol>
  )
}
