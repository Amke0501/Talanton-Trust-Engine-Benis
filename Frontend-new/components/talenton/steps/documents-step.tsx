'use client'

import { useRef } from 'react'
import { Check, FileText, Upload, X } from 'lucide-react'
import type { ApplicationDraft, DocumentSlot } from '@/lib/talenton-data'
import { Badge } from '@/components/talenton/primitives'
import { cn } from '@/lib/utils'

export function DocumentsStep({
  draft,
  update,
}: {
  draft: ApplicationDraft
  update: (patch: Partial<ApplicationDraft>) => void
}) {
  function setFile(id: string, fileName?: string) {
    update({
      documents: draft.documents.map((d) =>
        d.id === id ? { ...d, fileName } : d,
      ),
    })
  }

  return (
    <div className="space-y-3">
      <p className="text-sm leading-relaxed text-muted-foreground">
        Upload clear copies of each document. Required items must be provided
        before you can submit.
      </p>
      {draft.documents.map((doc) => (
        <DocumentRow
          key={doc.id}
          doc={doc}
          onUpload={(name) => setFile(doc.id, name)}
          onClear={() => setFile(doc.id, undefined)}
        />
      ))}
    </div>
  )
}

function DocumentRow({
  doc,
  onUpload,
  onClear,
}: {
  doc: DocumentSlot
  onUpload: (fileName: string) => void
  onClear: () => void
}) {
  const inputRef = useRef<HTMLInputElement>(null)
  const uploaded = Boolean(doc.fileName)

  return (
    <div
      className={cn(
        'flex flex-col gap-3 rounded-xl border p-4 sm:flex-row sm:items-center sm:justify-between',
        uploaded ? 'border-primary/30 bg-accent/25' : 'border-border bg-background',
      )}
    >
      <div className="flex items-start gap-3">
        <span
          className={cn(
            'mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-lg',
            uploaded
              ? 'bg-primary text-primary-foreground'
              : 'bg-muted text-muted-foreground',
          )}
        >
          {uploaded ? <Check className="size-4" /> : <FileText className="size-4" />}
        </span>
        <div>
          <div className="flex flex-wrap items-center gap-2">
            <p className="text-sm font-semibold text-foreground">{doc.label}</p>
            {doc.required ? (
              <Badge tone="muted">Required</Badge>
            ) : (
              <Badge tone="muted">Optional</Badge>
            )}
          </div>
          <p className="mt-1 text-xs leading-relaxed text-muted-foreground">
            {uploaded ? doc.fileName : doc.hint}
          </p>
        </div>
      </div>

      <div className="flex items-center gap-2 sm:shrink-0">
        <input
          ref={inputRef}
          type="file"
          className="sr-only"
          onChange={(e) => {
            const file = e.target.files?.[0]
            if (file) onUpload(file.name)
          }}
        />
        {uploaded ? (
          <button
            type="button"
            onClick={onClear}
            className="inline-flex items-center gap-1.5 rounded-lg border border-border bg-background px-3 py-2 text-xs font-medium text-muted-foreground transition-colors hover:text-destructive"
          >
            <X className="size-3.5" />
            Remove
          </button>
        ) : (
          <button
            type="button"
            onClick={() => inputRef.current?.click()}
            className="inline-flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-xs font-semibold text-primary-foreground transition-colors hover:bg-primary/90"
          >
            <Upload className="size-3.5" />
            Upload
          </button>
        )}
      </div>
    </div>
  )
}
