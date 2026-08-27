'use client'

import { useMemo, useState } from 'react'
import {
  ArrowLeft,
  ArrowRight,
  Check,
  FileCheck2,
  FileText,
  Send,
  UserRound,
  Users,
  Wallet,
} from 'lucide-react'
import {
  emptyDraft,
  type ApplicationDraft,
} from '@/lib/talenton-data'
import { Card, CardBody, SectionTitle } from '@/components/talenton/primitives'
import { ProfileStep } from '@/components/talenton/steps/profile-step'
import { LoanStep } from '@/components/talenton/steps/loan-step'
import { DocumentsStep } from '@/components/talenton/steps/documents-step'
import { GuarantorsStep } from '@/components/talenton/steps/guarantors-step'
import { ReviewStep } from '@/components/talenton/steps/review-step'
import { cn } from '@/lib/utils'

type StepDef = {
  id: string
  title: string
  hint: string
  icon: typeof UserRound
  isValid: (d: ApplicationDraft) => boolean
}

const STEP_DEFS: StepDef[] = [
  {
    id: 'profile',
    title: 'Your profile',
    hint: 'Tell us who is applying and your membership details.',
    icon: UserRound,
    isValid: (d) =>
      Boolean(d.fullName && d.memberId && d.phone && d.email) &&
      d.savingsBalance > 0,
  },
  {
    id: 'loan',
    title: 'Loan details',
    hint: 'The amount you need and how you plan to use it.',
    icon: Wallet,
    isValid: (d) => d.principal > 0 && Boolean(d.purpose) && d.tenureMonths > 0,
  },
  {
    id: 'documents',
    title: 'Documents',
    hint: 'Upload the paperwork the SACCO needs to verify.',
    icon: FileText,
    isValid: (d) => d.documents.filter((x) => x.required).every((x) => x.fileName),
  },
  {
    id: 'guarantors',
    title: 'Guarantors',
    hint: 'Add members who will back your loan (optional).',
    icon: Users,
    isValid: () => true,
  },
  {
    id: 'review',
    title: 'Review & submit',
    hint: 'Check everything before sending it to the SACCO.',
    icon: FileCheck2,
    isValid: (d) => d.documents.filter((x) => x.required).every((x) => x.fileName),
  },
]

export function ApplicationWizard({
  onSubmit,
  onCancel,
}: {
  onSubmit: (draft: ApplicationDraft) => void
  onCancel: () => void
}) {
  const [draft, setDraft] = useState<ApplicationDraft>(() => emptyDraft())
  const [stepIndex, setStepIndex] = useState(0)

  const step = STEP_DEFS[stepIndex]
  const isLast = stepIndex === STEP_DEFS.length - 1
  const canAdvance = useMemo(() => step.isValid(draft), [step, draft])

  function update(patch: Partial<ApplicationDraft>) {
    setDraft((prev) => ({ ...prev, ...patch }))
  }

  function next() {
    if (!canAdvance) return
    if (isLast) {
      onSubmit(draft)
      return
    }
    setStepIndex((i) => Math.min(i + 1, STEP_DEFS.length - 1))
  }

  function back() {
    if (stepIndex === 0) {
      onCancel()
      return
    }
    setStepIndex((i) => Math.max(i - 1, 0))
  }

  return (
    <div className="grid gap-6 lg:grid-cols-[240px_1fr]">
      {/* Step rail */}
      <nav aria-label="Application steps" className="lg:sticky lg:top-6 lg:self-start">
        <ol className="flex gap-2 overflow-x-auto pb-2 lg:flex-col lg:gap-1 lg:overflow-visible lg:pb-0">
          {STEP_DEFS.map((s, i) => {
            const done = i < stepIndex && s.isValid(draft)
            const active = i === stepIndex
            const reachable = i <= stepIndex
            return (
              <li key={s.id} className="shrink-0">
                <button
                  type="button"
                  onClick={() => reachable && setStepIndex(i)}
                  disabled={!reachable}
                  className={cn(
                    'flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition-colors',
                    active && 'bg-card font-semibold text-foreground shadow-sm ring-1 ring-border',
                    !active && reachable && 'text-muted-foreground hover:bg-card/60',
                    !reachable && 'cursor-not-allowed text-muted-foreground/50',
                  )}
                  aria-current={active ? 'step' : undefined}
                >
                  <span
                    className={cn(
                      'flex size-7 shrink-0 items-center justify-center rounded-full border text-xs font-semibold',
                      done && 'border-primary bg-primary text-primary-foreground',
                      active && !done && 'border-primary bg-background text-primary',
                      !active && !done && 'border-border bg-background text-muted-foreground',
                    )}
                  >
                    {done ? <Check className="size-3.5" /> : i + 1}
                  </span>
                  <span className="whitespace-nowrap lg:whitespace-normal">
                    {s.title}
                  </span>
                </button>
              </li>
            )
          })}
        </ol>
      </nav>

      {/* Active step */}
      <Card>
        <CardBody className="space-y-6">
          <SectionTitle
            icon={<step.icon className="size-4" />}
            title={step.title}
            hint={step.hint}
            aside={
              <span className="text-xs font-medium text-muted-foreground">
                Step {stepIndex + 1} of {STEP_DEFS.length}
              </span>
            }
          />

          <div className="border-t border-border pt-6">
            {step.id === 'profile' && <ProfileStep draft={draft} update={update} />}
            {step.id === 'loan' && <LoanStep draft={draft} update={update} />}
            {step.id === 'documents' && (
              <DocumentsStep draft={draft} update={update} />
            )}
            {step.id === 'guarantors' && (
              <GuarantorsStep draft={draft} update={update} />
            )}
            {step.id === 'review' && <ReviewStep draft={draft} />}
          </div>

          <div className="flex items-center justify-between gap-3 border-t border-border pt-5">
            <button
              type="button"
              onClick={back}
              className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-background px-4 py-2.5 text-sm font-medium text-foreground transition-colors hover:bg-muted"
            >
              <ArrowLeft className="size-4" />
              {stepIndex === 0 ? 'Cancel' : 'Back'}
            </button>

            <button
              type="button"
              onClick={next}
              disabled={!canAdvance}
              className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-5 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:pointer-events-none disabled:opacity-50"
            >
              {isLast ? (
                <>
                  <Send className="size-4" />
                  Submit application
                </>
              ) : (
                <>
                  Continue
                  <ArrowRight className="size-4" />
                </>
              )}
            </button>
          </div>
        </CardBody>
      </Card>
    </div>
  )
}
