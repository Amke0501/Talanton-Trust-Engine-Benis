'use client'

import { useState } from 'react'
import { 
  CheckCircle2, 
  FileText, 
  Send, 
  Sparkles, 
  X, 
  ChevronRight, 
  ChevronLeft, 
  Upload, 
  Save, 
  ShieldCheck, 
  User, 
  Wallet, 
  Clock, 
  Edit3 
} from 'lucide-react'
import {
  formatUGX,
  makeDocumentSlots,
  savingsCap,
  type Application,
  type ApplicantType,
  type DocumentSlot,
} from '@/lib/talenton-data'

export function ApplicantDashboardView({
  application,
  onUpdateApplication,
  onSubmitToUnderwriter,
  onSaveDraft,
  onClose,
}: {
  application: Application
  onUpdateApplication: (updated: Partial<Application>) => void
  onSubmitToUnderwriter: (appData: Partial<Application>) => Promise<void>
  onSaveDraft?: (appData: Partial<Application>) => Promise<void>
  onClose?: () => void
}) {
  // Step indicator state: 1: Basics, 2: Financials, 3: Documents, 4: Preview
  const [step, setStep] = useState(1)

  const [classification, setClassification] = useState<ApplicantType>(application.applicantType || 'individual')
  const [multiplier, setMultiplier] = useState<number>(application.multiplier || 3)
  const [purpose, setPurpose] = useState<string>(application.purpose || '')
  
  const [principal, setPrincipal] = useState<number | ''>(application.principal || '')
  const [savings, setSavings] = useState<number | ''>(application.savingsBalance || '')
  const [monthlyPay, setMonthlyPay] = useState<number | ''>(application.monthlyIncome || '')
  const [monthlyDebt, setMonthlyDebt] = useState<number | ''>(application.monthlyDebt || '')
  const [tenure, setTenure] = useState<number | ''>(application.tenureMonths || '')
  
  const [documents, setDocuments] = useState<DocumentSlot[]>(
    application.documents && application.documents.length > 0
      ? application.documents
      : makeDocumentSlots(classification)
  )

  const [isSubmitting, setIsSubmitting] = useState(false)
  const [isSavingDraft, setIsSavingDraft] = useState(false)

  const calculatedCap = savingsCap(Number(savings) || 0, multiplier)
  const estDTI = (Number(monthlyPay) > 0 && Number(monthlyDebt) >= 0)
    ? Math.round((Number(monthlyDebt) / Number(monthlyPay)) * 100)
    : 0

  function handleFieldChange(field: string, val: any) {
    if (field === 'classification') {
      setClassification(val)
      const newSlots = makeDocumentSlots(val)
      setDocuments(newSlots)
      onUpdateApplication({ applicantType: val, documents: newSlots })
    } else if (field === 'multiplier') {
      setMultiplier(val)
      onUpdateApplication({ multiplier: val })
    } else if (field === 'purpose') {
      setPurpose(val)
      onUpdateApplication({ purpose: val })
    } else if (field === 'principal') {
      const num = val === '' ? '' : Number(val)
      setPrincipal(num)
      onUpdateApplication({ principal: Number(num) || 0 })
    } else if (field === 'savings') {
      const num = val === '' ? '' : Number(val)
      setSavings(num)
      onUpdateApplication({ savingsBalance: Number(num) || 0 })
    } else if (field === 'monthlyPay') {
      const num = val === '' ? '' : Number(val)
      setMonthlyPay(num)
      onUpdateApplication({ monthlyIncome: Number(num) || 0 })
    } else if (field === 'monthlyDebt') {
      const num = val === '' ? '' : Number(val)
      setMonthlyDebt(num)
      onUpdateApplication({ monthlyDebt: Number(num) || 0 })
    } else if (field === 'tenure') {
      const num = val === '' ? '' : Number(val)
      setTenure(num)
      onUpdateApplication({ tenureMonths: Number(num) || 0 })
    }
  }

  function toggleDocumentStatus(slotId: string) {
    const updated = documents.map((d) => {
      if (d.id === slotId) {
        const nextStatus = d.status === 'VERIFIED' ? 'PENDING' : 'VERIFIED'
        return {
          ...d,
          status: nextStatus as any,
          fileName: nextStatus === 'VERIFIED' ? (d.fileName || `${d.id}_upload.pdf`) : undefined,
        }
      }
      return d
    })
    setDocuments(updated)
    onUpdateApplication({ documents: updated })
  }

  function handleOCRSimulate(type: 'salary' | 'sme') {
    if (type === 'salary') {
      setMonthlyPay(2500000)
      setSavings(4000000)
      setMonthlyDebt(500000)
      onUpdateApplication({
        monthlyIncome: 2500000,
        savingsBalance: 4000000,
        monthlyDebt: 500000,
      })
    } else {
      setMonthlyPay(7500000)
      setSavings(12000000)
      setMonthlyDebt(1800000)
      onUpdateApplication({
        monthlyIncome: 7500000,
        savingsBalance: 12000000,
        monthlyDebt: 1800000,
      })
    }
    // Mark docs as uploaded
    const updated = documents.map((d) => ({
      ...d,
      status: 'VERIFIED' as const,
      fileName: d.fileName || `${d.id}_ocr_scanned.pdf`,
    }))
    setDocuments(updated)
    onUpdateApplication({ documents: updated })
  }

  async function handleSaveDraftClick() {
    setIsSavingDraft(true)
    const appData = {
      reference: application.reference,
      applicantType: classification,
      multiplier,
      purpose,
      principal: Number(principal) || 0,
      savingsBalance: Number(savings) || 0,
      monthlyIncome: Number(monthlyPay) || 0,
      monthlyDebt: Number(monthlyDebt) || 0,
      tenureMonths: Number(tenure) || 12,
      documents,
      status: 'draft' as const,
      stage: 'draft' as const,
    }
    if (onSaveDraft) {
      await onSaveDraft(appData)
    }
    setIsSavingDraft(false)
    if (onClose) onClose()
  }

  async function handleFinalSubmit() {
    setIsSubmitting(true)
    const appData = {
      reference: application.reference,
      applicantType: classification,
      multiplier,
      purpose,
      principal: Number(principal) || 0,
      savingsBalance: Number(savings) || 0,
      monthlyIncome: Number(monthlyPay) || 0,
      monthlyDebt: Number(monthlyDebt) || 0,
      tenureMonths: Number(tenure) || 12,
      documents,
      status: 'submitted' as const,
      stage: 'verification' as const,
    }
    await onSubmitToUnderwriter(appData)
    setIsSubmitting(false)
  }

  return (
    <div className="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 overflow-y-auto">
      <div className="relative w-full max-w-2xl bg-white rounded-3xl shadow-2xl overflow-hidden my-8 animate-scaleUp">
        
        {/* Top Header */}
        <div className="bg-[#0d2a1c] p-6 text-white relative">
          <div className="flex items-center justify-between">
            <div>
              <span className="text-[0.65rem] font-bold uppercase tracking-widest text-[#a4cc44]">SACCO CREDIT PIPELINE</span>
              <h2 className="text-xl font-serif font-bold mt-0.5">Loan Request Wizard</h2>
            </div>
            
            <div className="flex items-center gap-2">
              <button
                type="button"
                onClick={handleSaveDraftClick}
                disabled={isSavingDraft}
                className="inline-flex items-center gap-1.5 rounded-full border border-white/20 bg-white/10 px-3 py-1.5 text-xs font-semibold text-white hover:bg-white/20 transition-all cursor-pointer"
                title="Save progress as draft"
              >
                <Save className="size-3.5 text-[#a4cc44]" />
                {isSavingDraft ? 'Saving...' : 'Save Draft'}
              </button>

              {onClose && (
                <button
                  type="button"
                  onClick={onClose}
                  className="p-1.5 rounded-full hover:bg-white/10 text-white/70 hover:text-white transition-colors"
                >
                  <X className="size-5" />
                </button>
              )}
            </div>
          </div>

          {/* Stepper Bar (4 Steps) */}
          <div className="grid grid-cols-4 gap-2 mt-6">
            {[
              { num: 1, label: 'Classification' },
              { num: 2, label: 'Financials' },
              { num: 3, label: 'Documents' },
              { num: 4, label: 'Preview & Submit' },
            ].map((s) => (
              <div 
                key={s.num} 
                className="cursor-pointer"
                onClick={() => setStep(s.num)}
              >
                <div className={`h-1.5 rounded-full transition-all ${
                  step >= s.num ? 'bg-[#a4cc44]' : 'bg-white/20'
                }`} />
                <p className={`text-[0.65rem] font-bold mt-1.5 truncate ${
                  step === s.num ? 'text-[#a4cc44]' : 'text-white/50'
                }`}>
                  {s.num}. {s.label}
                </p>
              </div>
            ))}
          </div>
        </div>

        {/* Step Content */}
        <div className="p-6 md:p-8 space-y-6">
          
          {/* STEP 1: Classification & Multiplier */}
          {step === 1 && (
            <div className="space-y-6 animate-fadeIn">
              <div className="space-y-3">
                <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Applicant Classification</label>
                <div className="grid grid-cols-2 gap-4">
                  <button
                    type="button"
                    onClick={() => handleFieldChange('classification', 'individual')}
                    className={`p-4 rounded-2xl border-2 text-left transition-all ${
                      classification === 'individual'
                        ? 'border-[#103a27] bg-[#103a27]/5'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <p className="font-bold text-sm text-[#103a27]">BOSA Member</p>
                    <p className="text-xs text-gray-500 mt-1">Individual salary or savings anchored borrower.</p>
                  </button>

                  <button
                    type="button"
                    onClick={() => handleFieldChange('classification', 'cooperative')}
                    className={`p-4 rounded-2xl border-2 text-left transition-all ${
                      classification === 'cooperative'
                        ? 'border-[#103a27] bg-[#103a27]/5'
                        : 'border-gray-200 hover:border-gray-300'
                    }`}
                  >
                    <p className="font-bold text-sm text-[#103a27]">SME Growth Business</p>
                    <p className="text-xs text-gray-500 mt-1">Enterprise or Cooperative trading pipeline.</p>
                  </button>
                </div>
              </div>

              {/* Capital Multiplier */}
              <div className="space-y-2">
                <div className="flex justify-between items-center">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Capital Savings Multiplier</label>
                  <span className="font-mono text-sm font-bold text-[#103a27] bg-gray-100 px-2.5 py-0.5 rounded-full">{multiplier.toFixed(1)}x Multiplier</span>
                </div>
                <input
                  type="range"
                  min="2"
                  max="5"
                  step="0.25"
                  value={multiplier}
                  onChange={(e) => handleFieldChange('multiplier', parseFloat(e.target.value))}
                  className="w-full accent-[#103a27]"
                />
              </div>

              {/* Loan Purpose */}
              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Loan Purpose</label>
                <input
                  type="text"
                  placeholder="e.g. Working capital inventory purchase"
                  value={purpose}
                  onChange={(e) => handleFieldChange('purpose', e.target.value)}
                  className="w-full rounded-xl border border-gray-200 p-3 text-sm font-semibold text-gray-800 focus:outline-none focus:border-[#103a27]"
                />
              </div>

              {/* Principal */}
              <div className="space-y-2">
                <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Requested Principal (UGX)</label>
                <input
                  type="number"
                  placeholder="e.g. 15,000,000"
                  value={principal}
                  onChange={(e) => handleFieldChange('principal', e.target.value)}
                  className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                />
              </div>
            </div>
          )}

          {/* STEP 2: Financial Details */}
          {step === 2 && (
            <div className="space-y-6 animate-fadeIn">
              <div className="grid gap-6 sm:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Current Savings Balance (UGX)</label>
                  <input
                    type="number"
                    placeholder="e.g. 4,000,000"
                    value={savings}
                    onChange={(e) => handleFieldChange('savings', e.target.value)}
                    className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Monthly Revenue / Income (UGX)</label>
                  <input
                    type="number"
                    placeholder="e.g. 2,500,000"
                    value={monthlyPay}
                    onChange={(e) => handleFieldChange('monthlyPay', e.target.value)}
                    className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>
              </div>

              <div className="grid gap-6 sm:grid-cols-2">
                <div className="space-y-2">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Monthly Debt Deductions (UGX)</label>
                  <input
                    type="number"
                    placeholder="e.g. 500,000"
                    value={monthlyDebt}
                    onChange={(e) => handleFieldChange('monthlyDebt', e.target.value)}
                    className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-gray-800 font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>

                <div className="space-y-2">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Amortization Tenure (Months)</label>
                  <input
                    type="number"
                    placeholder="e.g. 12"
                    value={tenure}
                    onChange={(e) => handleFieldChange('tenure', e.target.value)}
                    className="w-full rounded-xl border border-gray-200 p-3 text-sm font-bold text-gray-800 font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>
              </div>

              {/* Live Guardrails Preview */}
              <div className="rounded-2xl bg-[#0d2a1c] p-4 text-white flex items-center justify-between">
                <div>
                  <p className="text-[0.65rem] font-bold uppercase text-[#a4cc44]">Savings Cap Bound</p>
                  <p className="text-xl font-bold font-mono">{formatUGX(calculatedCap)}</p>
                </div>
                <div className="text-right">
                  <p className="text-[0.65rem] font-bold uppercase text-[#a4cc44]">Estimated DTI Ratio</p>
                  <p className="text-xl font-bold font-mono">{estDTI}%</p>
                </div>
              </div>
            </div>
          )}

          {/* STEP 3: Compliance & Document Uploads */}
          {step === 3 && (
            <div className="space-y-6 animate-fadeIn">
              <div className="space-y-3">
                <div className="flex items-center justify-between">
                  <label className="text-xs font-bold uppercase tracking-wider text-gray-500">Required KYC & Compliance Documents</label>
                  <button
                    type="button"
                    onClick={() => handleOCRSimulate('salary')}
                    className="text-xs font-bold text-[#103a27] hover:underline flex items-center gap-1"
                  >
                    <Sparkles className="size-3 text-[#a4cc44]" />
                    Simulate OCR Auto-Upload
                  </button>
                </div>

                <div className="space-y-3">
                  {documents.map((doc) => {
                    const isVerified = doc.status === 'VERIFIED'
                    return (
                      <div key={doc.id} className="flex items-center justify-between p-4 rounded-2xl border border-gray-200 bg-gray-50/50 hover:bg-gray-50 transition-colors">
                        <div className="flex items-start gap-3">
                          <FileText className="size-5 text-[#103a27] mt-0.5" />
                          <div>
                            <p className="text-sm font-bold text-[#103a27]">{doc.label}</p>
                            <p className="text-xs text-gray-500 mt-0.5">{doc.hint}</p>
                            {doc.fileName && (
                              <p className="text-[0.65rem] font-mono text-emerald-700 mt-1 font-semibold">&bull; {doc.fileName}</p>
                            )}
                          </div>
                        </div>

                        <button
                          type="button"
                          onClick={() => toggleDocumentStatus(doc.id)}
                          className="shrink-0 text-xs font-bold"
                        >
                          {isVerified ? (
                            <span className="inline-flex items-center gap-1 rounded-full bg-emerald-100 px-3 py-1 text-emerald-800">
                              <CheckCircle2 className="size-3.5" />
                              Attached
                            </span>
                          ) : (
                            <span className="inline-flex items-center gap-1 rounded-full bg-[#103a27] text-white px-3 py-1 hover:bg-[#1a5235]">
                              <Upload className="size-3" />
                              Upload
                            </span>
                          )}
                        </button>
                      </div>
                    )
                  })}
                </div>
              </div>
            </div>
          )}

          {/* STEP 4: Application Preview Page */}
          {step === 4 && (
            <div className="space-y-6 animate-fadeIn">
              <div className="rounded-2xl border border-[#103a27]/20 bg-[#f4f5f4] p-5 space-y-4">
                <div className="flex items-center justify-between pb-3 border-b border-gray-200">
                  <div className="flex items-center gap-2">
                    <User className="size-4 text-[#103a27]" />
                    <h3 className="font-serif text-sm font-bold text-[#103a27]">Applicant Identity & Classification</h3>
                  </div>
                  <button 
                    type="button" 
                    onClick={() => setStep(1)}
                    className="text-xs font-bold text-[#103a27] flex items-center gap-1 hover:underline"
                  >
                    <Edit3 className="size-3" /> Edit
                  </button>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 text-xs">
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Borrower Name</span>
                    <span className="font-bold text-[#103a27]">{application.fullName || 'Amina K. Nakamya'}</span>
                  </div>
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">SACCO ID</span>
                    <span className="font-mono font-bold text-gray-700">{application.memberId || 'M-8842'}</span>
                  </div>
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Borrower Type</span>
                    <span className="font-bold text-emerald-800 bg-emerald-50 px-2 py-0.5 rounded-full inline-block mt-0.5">
                      {classification === 'individual' ? 'BOSA Member' : 'SME Growth'}
                    </span>
                  </div>
                </div>
              </div>

              {/* Loan Details Card */}
              <div className="rounded-2xl border border-[#103a27]/20 bg-white p-5 space-y-4 shadow-sm">
                <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                  <div className="flex items-center gap-2">
                    <Wallet className="size-4 text-[#103a27]" />
                    <h3 className="font-serif text-sm font-bold text-[#103a27]">Requested Facility & Terms</h3>
                  </div>
                  <button 
                    type="button" 
                    onClick={() => setStep(2)}
                    className="text-xs font-bold text-[#103a27] flex items-center gap-1 hover:underline"
                  >
                    <Edit3 className="size-3" /> Edit
                  </button>
                </div>

                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 text-xs">
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Principal</span>
                    <span className="font-mono font-bold text-base text-[#103a27]">{formatUGX(Number(principal) || 0)}</span>
                  </div>
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Multiplier</span>
                    <span className="font-bold text-gray-800">{multiplier.toFixed(1)}x</span>
                  </div>
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Tenure</span>
                    <span className="font-bold text-gray-800">{tenure || 12} Months</span>
                  </div>
                  <div>
                    <span className="text-gray-400 block text-[0.65rem] uppercase font-bold">Savings Base</span>
                    <span className="font-mono font-bold text-gray-800">{formatUGX(Number(savings) || 0)}</span>
                  </div>
                </div>

                <div className="pt-2 text-xs text-gray-600">
                  <strong>Purpose:</strong> {purpose || 'Working capital and retail inventory'}
                </div>
              </div>

              {/* Attached Documents Checklist */}
              <div className="rounded-2xl border border-gray-200 bg-white p-5 space-y-3">
                <div className="flex items-center justify-between">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-gray-500">Compliance Files ({documents.filter(d => d.status === 'VERIFIED').length}/{documents.length} Attached)</h4>
                  <button 
                    type="button" 
                    onClick={() => setStep(3)}
                    className="text-xs font-bold text-[#103a27] flex items-center gap-1 hover:underline"
                  >
                    <Edit3 className="size-3" /> Edit
                  </button>
                </div>

                <div className="grid grid-cols-2 gap-2 text-xs">
                  {documents.map((d) => (
                    <div key={d.id} className="flex items-center gap-2 p-2 rounded-lg bg-gray-50 border border-gray-100">
                      {d.status === 'VERIFIED' ? (
                        <CheckCircle2 className="size-4 text-emerald-600 shrink-0" />
                      ) : (
                        <Clock className="size-4 text-amber-500 shrink-0" />
                      )}
                      <span className="truncate font-medium text-gray-700">{d.label}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

        </div>

        {/* Footer Navigation */}
        <div className="border-t border-gray-200 px-6 py-4 bg-gray-50 flex items-center justify-between">
          {step > 1 ? (
            <button
              type="button"
              onClick={() => setStep(prev => prev - 1)}
              className="flex items-center gap-1.5 rounded-xl border border-gray-300 bg-white px-4 py-2 text-xs font-semibold text-gray-700 hover:bg-gray-100 transition-colors cursor-pointer"
            >
              <ChevronLeft className="size-4" />
              Back
            </button>
          ) : (
            <div />
          )}

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={handleSaveDraftClick}
              disabled={isSavingDraft}
              className="px-4 py-2 rounded-xl border border-gray-300 bg-white text-gray-700 text-xs font-semibold hover:bg-gray-100 transition-all"
            >
              {isSavingDraft ? 'Saving Draft...' : 'Save Draft'}
            </button>

            {step < 4 ? (
              <button
                type="button"
                onClick={() => setStep(prev => prev + 1)}
                className="flex items-center gap-1.5 rounded-xl bg-[#0d2a1c] hover:bg-[#153e2a] text-white px-5 py-2.5 text-xs font-bold shadow-md transition-all cursor-pointer"
              >
                Next
                <ChevronRight className="size-4" />
              </button>
            ) : (
              <button
                type="button"
                onClick={handleFinalSubmit}
                disabled={isSubmitting}
                className="flex items-center gap-2 rounded-xl bg-[#a4cc44] hover:bg-[#b5dc55] text-[#0d2a1c] px-6 py-2.5 text-sm font-bold shadow-md transition-all cursor-pointer"
              >
                <Send className="size-4" />
                {isSubmitting ? 'Submitting to Underwriter...' : 'Submit to Underwriting Desk'}
              </button>
            )}
          </div>
        </div>

      </div>
    </div>
  )
}
