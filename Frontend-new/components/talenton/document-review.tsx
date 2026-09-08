'use client'

import { useState } from 'react'
import { FileText, Eye, Download, Check, X } from 'lucide-react'
import type { DocumentSlot } from '@/lib/talenton-data'

function readableSize(bytes?: number) {
  if (!bytes) return null
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

/**
 * One uploaded document, with the file in front of the reviewer before they decide on it.
 *
 * Verification previously offered a "Mark Verified" button and no way to see what was being
 * verified — the underwriter was asked to attest to a filename. Opening the document is the
 * review, so it belongs in the same row as the decision.
 */
export function DocumentReviewRow({
  doc,
  onDecision,
}: {
  doc: DocumentSlot
  onDecision?: (id: string, status: 'VERIFIED' | 'REJECTED', reason?: string) => void
}) {
  const [previewOpen, setPreviewOpen] = useState(false)
  const [rejecting, setRejecting] = useState(false)
  const [reason, setReason] = useState('')

  const verified = doc.status === 'VERIFIED'
  const rejected = doc.status === 'REJECTED'
  const hasFile = Boolean(doc.fileUrl)
  const size = readableSize(doc.fileSize)
  const isImage = /\.(png|jpe?g|gif|webp|avif)$/i.test(doc.fileName || '')
  const isPdf = /\.pdf$/i.test(doc.fileName || '')

  return (
    <div className="rounded-xl border border-gray-100 bg-[#f4f5f4] overflow-hidden">
      <div className="flex flex-wrap items-center justify-between gap-3 p-3.5">
        <div className="flex items-start gap-2.5 min-w-0">
          <FileText className="size-4 shrink-0 text-[#103a27] mt-0.5" />
          <div className="min-w-0">
            <p className="text-xs font-bold text-[#103a27]">{doc.label}</p>
            <p className="text-[0.65rem] text-gray-500 truncate">
              {doc.fileName || 'No file uploaded'}
              {size && <span className="ml-1.5 text-gray-400">{size}</span>}
            </p>
            {rejected && doc.rejectionReason && (
              <p className="text-[0.65rem] text-rose-700 mt-0.5">Rejected: {doc.rejectionReason}</p>
            )}
          </div>
        </div>

        <div className="flex items-center gap-1.5">
          {hasFile ? (
            <>
              {(isImage || isPdf) && (
                <button
                  type="button"
                  onClick={() => setPreviewOpen((open) => !open)}
                  className="flex items-center gap-1 rounded-full bg-white px-2.5 py-1 text-[0.7rem] font-semibold text-[#103a27] hover:bg-gray-100 cursor-pointer"
                >
                  <Eye className="size-3" />
                  {previewOpen ? 'Hide' : 'View'}
                </button>
              )}
              <a
                href={doc.fileUrl}
                target="_blank"
                rel="noopener noreferrer"
                download={doc.fileName}
                className="flex items-center gap-1 rounded-full bg-white px-2.5 py-1 text-[0.7rem] font-semibold text-[#103a27] hover:bg-gray-100"
              >
                <Download className="size-3" />
                Open
              </a>
            </>
          ) : (
            <span className="text-[0.7rem] text-gray-400 italic">Nothing to review</span>
          )}

          {onDecision && hasFile && (
            <>
              <button
                type="button"
                onClick={() => onDecision(doc.id, 'VERIFIED')}
                className={`flex items-center gap-1 rounded-full px-2.5 py-1 text-[0.7rem] font-bold cursor-pointer transition-all ${
                  verified ? 'bg-emerald-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-emerald-100 hover:text-emerald-800'
                }`}
              >
                <Check className="size-3" />
                {verified ? 'Verified' : 'Verify'}
              </button>
              <button
                type="button"
                onClick={() => setRejecting((r) => !r)}
                className={`flex items-center gap-1 rounded-full px-2.5 py-1 text-[0.7rem] font-bold cursor-pointer transition-all ${
                  rejected ? 'bg-rose-600 text-white' : 'bg-gray-200 text-gray-700 hover:bg-rose-100 hover:text-rose-800'
                }`}
              >
                <X className="size-3" />
                {rejected ? 'Rejected' : 'Reject'}
              </button>
            </>
          )}
        </div>
      </div>

      {rejecting && onDecision && (
        <div className="flex flex-wrap items-center gap-2 border-t border-gray-200 bg-white px-3.5 py-2.5">
          <input
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            placeholder="What is wrong with it? The applicant sees this."
            className="flex-1 min-w-[14rem] rounded-lg border border-gray-200 px-2.5 py-1.5 text-xs outline-none focus:border-[#103a27]"
          />
          <button
            type="button"
            disabled={!reason.trim()}
            onClick={() => { onDecision(doc.id, 'REJECTED', reason.trim()); setRejecting(false); setReason('') }}
            className="rounded-full bg-rose-600 px-3 py-1.5 text-[0.7rem] font-bold text-white disabled:opacity-40 cursor-pointer"
          >
            Confirm rejection
          </button>
        </div>
      )}

      {previewOpen && doc.fileUrl && (
        <div className="border-t border-gray-200 bg-white p-3">
          {isImage ? (
            <img src={doc.fileUrl} alt={doc.label} className="max-h-[28rem] w-auto mx-auto rounded-lg" />
          ) : (
            <object data={doc.fileUrl} type="application/pdf" className="h-[28rem] w-full rounded-lg">
              <p className="p-4 text-xs text-gray-600">
                This document cannot be shown inline.{' '}
                <a href={doc.fileUrl} target="_blank" rel="noopener noreferrer" className="underline">
                  Open it in a new tab
                </a>{' '}
                to review it.
              </p>
            </object>
          )}
        </div>
      )}
    </div>
  )
}
