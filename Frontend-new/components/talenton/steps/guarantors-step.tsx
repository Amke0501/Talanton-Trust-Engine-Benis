'use client'

import { useState } from 'react'
import { Plus, Trash2, Users } from 'lucide-react'
import {
  formatUGX,
  type ApplicationDraft,
  type Guarantor,
} from '@/lib/talenton-data'
import { Field, Input } from '@/components/talenton/primitives'

export function GuarantorsStep({
  draft,
  update,
}: {
  draft: ApplicationDraft
  update: (patch: Partial<ApplicationDraft>) => void
}) {
  const [name, setName] = useState('')
  const [memberId, setMemberId] = useState('')
  const [shares, setShares] = useState('')

  const canAdd = name.trim() && memberId.trim()

  function addGuarantor() {
    if (!canAdd) return
    const g: Guarantor = {
      id: crypto.randomUUID(),
      name: name.trim(),
      memberId: memberId.trim(),
      pledgedShares: Number(shares) || 0,
    }
    update({ guarantors: [...draft.guarantors, g] })
    setName('')
    setMemberId('')
    setShares('')
  }

  function remove(id: string) {
    update({ guarantors: draft.guarantors.filter((g) => g.id !== id) })
  }

  const totalPledged = draft.guarantors.reduce(
    (sum, g) => sum + g.pledgedShares,
    0,
  )

  return (
    <div className="space-y-6">
      <p className="text-sm leading-relaxed text-muted-foreground">
        Guarantors are fellow SACCO members who pledge their shares to back your
        loan. Adding guarantors strengthens your application.
      </p>

      <div className="grid gap-4 rounded-xl border border-border bg-muted/50 p-4 sm:grid-cols-3">
        <Field label="Guarantor name" className="sm:col-span-1">
          <Input value={name} onChange={(e) => setName(e.target.value)} placeholder="Full name" />
        </Field>
        <Field label="Member ID" className="sm:col-span-1">
          <Input
            value={memberId}
            onChange={(e) => setMemberId(e.target.value)}
            placeholder="e.g. M-1104"
          />
        </Field>
        <Field label="Pledged shares (UGX)" className="sm:col-span-1">
          <Input
            type="number"
            min={0}
            value={shares}
            onChange={(e) => setShares(e.target.value)}
            placeholder="0"
          />
        </Field>
        <div className="sm:col-span-3">
          <button
            type="button"
            onClick={addGuarantor}
            disabled={!canAdd}
            className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-3.5 py-2.5 text-sm font-semibold text-primary-foreground transition-colors hover:bg-primary/90 disabled:pointer-events-none disabled:opacity-50"
          >
            <Plus className="size-4" />
            Add guarantor
          </button>
        </div>
      </div>

      {draft.guarantors.length > 0 ? (
        <div className="space-y-2">
          {draft.guarantors.map((g) => (
            <div
              key={g.id}
              className="flex items-center justify-between gap-3 rounded-xl border border-border bg-background p-4"
            >
              <div className="flex items-center gap-3">
                <span className="flex size-9 items-center justify-center rounded-full bg-accent text-xs font-semibold text-accent-foreground">
                  {g.name.slice(0, 2).toUpperCase()}
                </span>
                <div>
                  <p className="text-sm font-semibold text-foreground">
                    {g.name}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {g.memberId} · Pledged {formatUGX(g.pledgedShares)}
                  </p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => remove(g.id)}
                className="flex size-8 items-center justify-center rounded-lg text-muted-foreground transition-colors hover:bg-destructive/10 hover:text-destructive"
                aria-label={`Remove ${g.name}`}
              >
                <Trash2 className="size-4" />
              </button>
            </div>
          ))}
          <div className="flex items-center justify-between rounded-xl bg-accent/40 px-4 py-3 text-sm">
            <span className="font-medium text-foreground">Total pledged</span>
            <span className="font-semibold text-primary">
              {formatUGX(totalPledged)}
            </span>
          </div>
        </div>
      ) : (
        <div className="flex flex-col items-center gap-2 rounded-xl border border-dashed border-border py-10 text-center">
          <Users className="size-6 text-muted-foreground" />
          <p className="text-sm text-muted-foreground">
            No guarantors added yet.
          </p>
        </div>
      )}
    </div>
  )
}
