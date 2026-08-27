import { Info } from 'lucide-react'
import {
  formatUGX,
  savingsCap,
  type ApplicationDraft,
} from '@/lib/talenton-data'
import { Field, Input, Select, Textarea } from '@/components/talenton/primitives'

export function LoanStep({
  draft,
  update,
}: {
  draft: ApplicationDraft
  update: (patch: Partial<ApplicationDraft>) => void
}) {
  const cap = savingsCap(draft.savingsBalance, draft.multiplier)
  const overCap = draft.principal > cap && cap > 0

  return (
    <div className="space-y-6">
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="Requested loan amount (UGX)">
          <Input
            type="number"
            min={0}
            value={draft.principal || ''}
            onChange={(e) => update({ principal: Number(e.target.value) })}
            placeholder="0"
          />
        </Field>
        <Field label="Savings multiplier">
          <Select
            value={draft.multiplier}
            onChange={(e) => update({ multiplier: Number(e.target.value) })}
          >
            <option value={3}>3× savings balance</option>
            <option value={4}>4× savings balance</option>
          </Select>
        </Field>
        <Field label="Repayment period (months)">
          <Input
            type="number"
            min={1}
            max={36}
            value={draft.tenureMonths || ''}
            onChange={(e) => update({ tenureMonths: Number(e.target.value) })}
            placeholder="12"
          />
        </Field>
        <Field
          label={
            draft.applicantType === 'individual'
              ? 'Monthly net pay (UGX)'
              : 'Monthly revenue (UGX)'
          }
        >
          <Input
            type="number"
            min={0}
            value={draft.monthlyIncome || ''}
            onChange={(e) => update({ monthlyIncome: Number(e.target.value) })}
            placeholder="0"
          />
        </Field>
        <Field
          label="Existing monthly debt (UGX)"
          hint="Loan repayments or deductions you already have."
        >
          <Input
            type="number"
            min={0}
            value={draft.monthlyDebt || ''}
            onChange={(e) => update({ monthlyDebt: Number(e.target.value) })}
            placeholder="0"
          />
        </Field>
      </div>

      <Field label="What is the loan for?">
        <Textarea
          value={draft.purpose}
          onChange={(e) => update({ purpose: e.target.value })}
          placeholder="Briefly describe how you will use the funds and how it will generate income."
        />
      </Field>

      <div className="flex items-start gap-3 rounded-xl border border-border bg-muted/60 p-4">
        <span className="mt-0.5 flex size-8 shrink-0 items-center justify-center rounded-lg bg-accent text-accent-foreground">
          <Info className="size-4" />
        </span>
        <div className="text-sm">
          <p className="font-medium text-foreground">
            Your savings cap is {formatUGX(cap)}
          </p>
          <p className="mt-1 leading-relaxed text-muted-foreground">
            {overCap
              ? 'Your requested amount is above your current cap. You can still apply — the SACCO may ask for extra guarantors or a larger savings anchor.'
              : 'This is the maximum you can borrow against your savings at the selected multiplier. The final decision is made by the SACCO.'}
          </p>
        </div>
      </div>
    </div>
  )
}
