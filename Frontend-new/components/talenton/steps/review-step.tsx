import { CheckCircle2, CircleAlert } from 'lucide-react'
import {
  CLASSIFICATION_LABEL,
  formatUGX,
  savingsCap,
  type ApplicationDraft,
} from '@/lib/talenton-data'

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between gap-4 py-2 text-sm">
      <span className="text-muted-foreground">{label}</span>
      <span className="text-right font-medium text-foreground">{value}</span>
    </div>
  )
}

export function ReviewStep({ draft }: { draft: ApplicationDraft }) {
  const requiredDocs = draft.documents.filter((d) => d.required)
  const uploadedRequired = requiredDocs.filter((d) => d.fileName)
  const cap = savingsCap(draft.savingsBalance, draft.multiplier)

  const groups = [
    {
      title: 'Applicant',
      rows: [
        { label: 'Type', value: CLASSIFICATION_LABEL[draft.applicantType] },
        { label: 'Name', value: draft.fullName || '—' },
        { label: 'Member ID', value: draft.memberId || '—' },
        { label: 'Savings balance', value: formatUGX(draft.savingsBalance) },
      ],
    },
    {
      title: 'Loan request',
      rows: [
        { label: 'Amount', value: formatUGX(draft.principal) },
        { label: 'Repayment period', value: `${draft.tenureMonths} months` },
        { label: 'Multiplier', value: `${draft.multiplier}×` },
        {
          label:
            draft.applicantType === 'individual'
              ? 'Monthly net pay'
              : 'Monthly revenue',
          value: formatUGX(draft.monthlyIncome),
        },
        { label: 'Savings cap', value: formatUGX(cap) },
      ],
    },
  ]

  return (
    <div className="space-y-5">
      {groups.map((g) => (
        <div key={g.title} className="rounded-xl border border-border bg-background p-4">
          <p className="mb-1 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            {g.title}
          </p>
          <div className="divide-y divide-border">
            {g.rows.map((r) => (
              <Row key={r.label} label={r.label} value={r.value} />
            ))}
          </div>
        </div>
      ))}

      <div className="rounded-xl border border-border bg-background p-4">
        <p className="mb-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
          Documents
        </p>
        <ul className="space-y-2">
          {draft.documents.map((d) => {
            const ok = Boolean(d.fileName)
            return (
              <li key={d.id} className="flex items-center gap-2 text-sm">
                {ok ? (
                  <CheckCircle2 className="size-4 text-primary" />
                ) : (
                  <CircleAlert
                    className={
                      d.required
                        ? 'size-4 text-warning-foreground'
                        : 'size-4 text-muted-foreground'
                    }
                  />
                )}
                <span className="text-foreground">{d.label}</span>
                <span className="ml-auto text-xs text-muted-foreground">
                  {ok ? 'Attached' : d.required ? 'Missing' : 'Optional'}
                </span>
              </li>
            )
          })}
        </ul>
      </div>

      <div className="rounded-xl bg-header p-4 text-header-foreground">
        <div className="flex items-center justify-between text-sm">
          <span className="text-header-muted">Guarantors</span>
          <span className="font-semibold">
            {draft.guarantors.length} added
          </span>
        </div>
        <p className="mt-2 text-xs leading-relaxed text-header-muted">
          Once submitted, a credit owner will verify your documents and review
          your request. You&apos;ll be able to track progress here — the detailed
          underwriting review is handled privately by the SACCO.
        </p>
      </div>

      {uploadedRequired.length < requiredDocs.length ? (
        <div className="flex items-start gap-3 rounded-xl border border-warning/40 bg-warning/15 p-4 text-sm text-warning-foreground">
          <CircleAlert className="mt-0.5 size-4 shrink-0" />
          <p className="leading-relaxed">
            {requiredDocs.length - uploadedRequired.length} required document(s)
            still missing. Add them before submitting.
          </p>
        </div>
      ) : null}
    </div>
  )
}
