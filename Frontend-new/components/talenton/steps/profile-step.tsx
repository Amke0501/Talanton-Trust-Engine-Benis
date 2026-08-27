import { Building2, User } from 'lucide-react'
import {
  CLASSIFICATION_LABEL,
  makeDocumentSlots,
  type ApplicantType,
  type ApplicationDraft,
} from '@/lib/talenton-data'
import { Field, Input } from '@/components/talenton/primitives'
import { cn } from '@/lib/utils'

export function ProfileStep({
  draft,
  update,
}: {
  draft: ApplicationDraft
  update: (patch: Partial<ApplicationDraft>) => void
}) {
  const types: { key: ApplicantType; icon: typeof User }[] = [
    { key: 'individual', icon: User },
    { key: 'cooperative', icon: Building2 },
  ]

  return (
    <div className="space-y-6">
      <div>
        <p className="mb-2 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Who is applying?
        </p>
        <div className="grid gap-3 sm:grid-cols-2">
          {types.map(({ key, icon: Icon }) => {
            const selected = draft.applicantType === key
            return (
              <button
                key={key}
                type="button"
                onClick={() =>
                  update({
                    applicantType: key,
                    // reset the required document set for the new type
                    documents: makeDocumentSlots(key),
                  })
                }
                className={cn(
                  'flex items-center gap-3 rounded-xl border p-4 text-left transition-colors',
                  selected
                    ? 'border-primary bg-accent/40 ring-1 ring-primary'
                    : 'border-border bg-background hover:border-primary/40',
                )}
                aria-pressed={selected}
              >
                <span
                  className={cn(
                    'flex size-10 items-center justify-center rounded-lg',
                    selected
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted text-muted-foreground',
                  )}
                >
                  <Icon className="size-5" />
                </span>
                <span className="text-sm font-medium text-foreground">
                  {CLASSIFICATION_LABEL[key]}
                </span>
              </button>
            )
          })}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <Field label={draft.applicantType === 'individual' ? 'Full name' : 'Cooperative / business name'}>
          <Input
            value={draft.fullName}
            onChange={(e) => update({ fullName: e.target.value })}
            placeholder={
              draft.applicantType === 'individual'
                ? 'e.g. Auma Florence'
                : 'e.g. Ssemakula Agro Ltd'
            }
          />
        </Field>
        <Field label="SACCO member ID">
          <Input
            value={draft.memberId}
            onChange={(e) => update({ memberId: e.target.value })}
            placeholder="e.g. M-4511"
          />
        </Field>
        <Field label="Phone number">
          <Input
            value={draft.phone}
            onChange={(e) => update({ phone: e.target.value })}
            placeholder="+256 7XX XXX XXX"
            inputMode="tel"
          />
        </Field>
        <Field label="Email address">
          <Input
            type="email"
            value={draft.email}
            onChange={(e) => update({ email: e.target.value })}
            placeholder="you@example.com"
          />
        </Field>
        <Field
          label="Current savings balance (UGX)"
          hint="Your savings anchor determines how much you can borrow."
          className="sm:col-span-2"
        >
          <Input
            type="number"
            min={0}
            value={draft.savingsBalance || ''}
            onChange={(e) => update({ savingsBalance: Number(e.target.value) })}
            placeholder="0"
          />
        </Field>
      </div>
    </div>
  )
}
