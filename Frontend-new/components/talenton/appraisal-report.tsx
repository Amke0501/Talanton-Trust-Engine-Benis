'use client'

import { Check, FileText, X } from 'lucide-react'
import { formatUGX, type AppraisalReport } from '@/lib/talenton-data'

/**
 * The underwriter's findings, shown to the applicant with a revised offer.
 *
 * The client asked for the revised amount to arrive "with an appraisal report explaining the
 * underwriter's reason". A single sentence is a reason, not a report — and someone being asked to
 * consent to a smaller loan cannot weigh that from one line. Every figure here was already on the
 * file; the gap was that none of it travelled with the offer.
 */
export function AppraisalReportPanel({ report }: { report: AppraisalReport }) {
  const reduced = report.revisedPrincipal < report.originalPrincipal
  const termChanged = report.revisedTenureMonths !== report.originalTenureMonths

  return (
    <div className="rounded-xl border border-amber-200 bg-white/70 p-4">
      <div className="flex items-center gap-2 border-b border-amber-100 pb-2.5">
        <FileText className="size-3.5 text-amber-800" />
        <p className="text-[0.65rem] font-bold uppercase tracking-widest text-amber-800">
          Appraisal report
        </p>
        {report.preparedAt && (
          <span className="ml-auto text-[0.65rem] text-gray-500">
            {new Date(report.preparedAt).toLocaleDateString('en-GB', {
              day: '2-digit',
              month: 'short',
              year: 'numeric',
            })}
          </span>
        )}
      </div>

      <dl className="mt-3 grid grid-cols-2 gap-x-4 gap-y-1 text-xs">
        <dt className="text-gray-600">You asked for</dt>
        <dd className="text-right font-mono tabular-nums text-gray-700">
          {formatUGX(report.originalPrincipal)} over {report.originalTenureMonths} months
        </dd>
        <dt className="font-semibold text-amber-900">{reduced ? 'Offered' : 'Revised to'}</dt>
        <dd className="text-right font-mono font-bold tabular-nums text-amber-950">
          {formatUGX(report.revisedPrincipal)} over {report.revisedTenureMonths} months
        </dd>
      </dl>

      {(reduced || termChanged) && report.reason && (
        <p className="mt-3 rounded-lg bg-amber-50 px-3 py-2 text-xs leading-relaxed text-amber-900">
          {report.reason}
        </p>
      )}

      {report.guardrails?.length > 0 && (
        <div className="mt-3">
          <p className="text-[0.65rem] font-bold uppercase tracking-wider text-gray-500">
            Checks on your application
          </p>
          <ul className="mt-1.5 space-y-1.5">
            {report.guardrails.map((finding) => (
              <li key={finding.check} className="flex items-start gap-2 text-xs">
                <span
                  className={`mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-full ${
                    finding.passed ? 'bg-emerald-100 text-emerald-700' : 'bg-rose-100 text-rose-700'
                  }`}
                >
                  {finding.passed ? <Check className="size-2.5" /> : <X className="size-2.5" />}
                </span>
                <span>
                  <span className="font-semibold text-[#103a27]">{finding.check}</span>
                  <span className="block text-[0.7rem] leading-snug text-gray-600">{finding.detail}</span>
                </span>
              </li>
            ))}
          </ul>
        </div>
      )}

      {(report.crbCategory || report.fieldAuditCharacter) && (
        <div className="mt-3 space-y-1.5 border-t border-amber-100 pt-2.5">
          {report.crbCategory && (
            <p className="text-[0.7rem] text-gray-600">
              <span className="font-semibold text-gray-800">Credit record: </span>
              {report.crbCategory}
              {report.crbScore ? ` (score ${report.crbScore}/900)` : ''}
            </p>
          )}
          {report.fieldAuditCharacter && (
            <p className="text-[0.7rem] text-gray-600">
              <span className="font-semibold text-gray-800">Character: </span>
              {report.fieldAuditCharacter}
            </p>
          )}
          {report.fieldAuditCapacity && (
            <p className="text-[0.7rem] text-gray-600">
              <span className="font-semibold text-gray-800">Capacity: </span>
              {report.fieldAuditCapacity}
            </p>
          )}
          {report.fieldAuditCollateral && (
            <p className="text-[0.7rem] text-gray-600">
              <span className="font-semibold text-gray-800">Collateral: </span>
              {report.fieldAuditCollateral}
            </p>
          )}
        </div>
      )}

      {report.preparedBy && (
        <p className="mt-3 text-[0.65rem] text-gray-500">Prepared by {report.preparedBy}</p>
      )}
    </div>
  )
}
