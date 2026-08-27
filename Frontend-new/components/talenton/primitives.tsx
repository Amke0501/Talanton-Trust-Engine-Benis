import type * as React from 'react'
import { cn } from '@/lib/utils'

export function Card({
  className,
  ...props
}: React.ComponentProps<'div'>) {
  return (
    <div
      className={cn(
        'rounded-2xl border border-glass-border bg-card text-card-foreground backdrop-blur-xl',
        'shadow-[0_18px_50px_-24px_oklch(0.3_0.06_150_/_0.45)]',
        className,
      )}
      {...props}
    />
  )
}

export function CardBody({
  className,
  ...props
}: React.ComponentProps<'div'>) {
  return <div className={cn('p-5 sm:p-6', className)} {...props} />
}

export function SectionTitle({
  icon,
  title,
  hint,
  aside,
}: {
  icon?: React.ReactNode
  title: string
  hint?: string
  aside?: React.ReactNode
}) {
  return (
    <div className="flex items-start justify-between gap-4">
      <div className="flex items-start gap-3">
        {icon ? (
          <span className="mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-lg bg-accent text-accent-foreground">
            {icon}
          </span>
        ) : null}
        <div>
          <h2 className="font-serif text-lg font-semibold leading-tight text-foreground text-balance">
            {title}
          </h2>
          {hint ? (
            <p className="mt-1 text-sm leading-relaxed text-muted-foreground text-pretty">
              {hint}
            </p>
          ) : null}
        </div>
      </div>
      {aside ? <div className="shrink-0">{aside}</div> : null}
    </div>
  )
}

export function Label({
  className,
  ...props
}: React.ComponentProps<'label'>) {
  return (
    <label
      className={cn(
        'mb-1.5 block text-xs font-semibold uppercase tracking-wide text-muted-foreground',
        className,
      )}
      {...props}
    />
  )
}

export function Input({
  className,
  ...props
}: React.ComponentProps<'input'>) {
  return (
    <input
      className={cn(
        'h-11 w-full rounded-lg border border-input bg-field px-3 text-sm text-foreground shadow-sm outline-none transition-colors',
        'placeholder:text-muted-foreground/70',
        'focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/25',
        className,
      )}
      {...props}
    />
  )
}

export function Textarea({
  className,
  ...props
}: React.ComponentProps<'textarea'>) {
  return (
    <textarea
      className={cn(
        'min-h-24 w-full rounded-lg border border-input bg-field px-3 py-2.5 text-sm text-foreground shadow-sm outline-none transition-colors',
        'placeholder:text-muted-foreground/70',
        'focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/25',
        className,
      )}
      {...props}
    />
  )
}

export function Select({
  className,
  children,
  ...props
}: React.ComponentProps<'select'>) {
  return (
    <div className="relative">
      <select
        className={cn(
          'h-11 w-full appearance-none rounded-lg border border-input bg-field pl-3 pr-9 text-sm text-foreground shadow-sm outline-none transition-colors',
          'focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/25',
          className,
        )}
        {...props}
      >
        {children}
      </select>
      <svg
        aria-hidden="true"
        viewBox="0 0 20 20"
        className="pointer-events-none absolute right-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground"
        fill="none"
        stroke="currentColor"
        strokeWidth="1.6"
      >
        <path d="m6 8 4 4 4-4" strokeLinecap="round" strokeLinejoin="round" />
      </svg>
    </div>
  )
}

export function Field({
  label,
  htmlFor,
  children,
  hint,
  className,
}: {
  label: string
  htmlFor?: string
  children: React.ReactNode
  hint?: string
  className?: string
}) {
  return (
    <div className={className}>
      <Label htmlFor={htmlFor}>{label}</Label>
      {children}
      {hint ? (
        <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
          {hint}
        </p>
      ) : null}
    </div>
  )
}

const toneStyles = {
  muted: 'bg-muted text-muted-foreground',
  warning: 'bg-warning/20 text-warning-foreground',
  primary: 'bg-accent text-accent-foreground',
  success: 'bg-success/15 text-success',
  destructive: 'bg-destructive/12 text-destructive',
} as const

export function Badge({
  tone = 'muted',
  className,
  children,
}: {
  tone?: keyof typeof toneStyles
  className?: string
  children: React.ReactNode
}) {
  return (
    <span
      className={cn(
        'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold',
        toneStyles[tone],
        className,
      )}
    >
      {children}
    </span>
  )
}
