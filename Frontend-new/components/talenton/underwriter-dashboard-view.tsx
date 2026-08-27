'use client'

import { useState } from 'react'
import {
  AlertTriangle,
  CheckCircle2,
  Plus,
  Send,
  ShieldAlert,
  ShieldCheck,
  Trash2,
  UserCheck,
  FileText,
  FileCheck,
  Sparkles,
  Award,
  Building,
  User,
  XCircle,
  Clock
} from 'lucide-react'
import {
  formatUGX,
  type Application,
  type Guarantor,
  type DocumentSlot,
} from '@/lib/talenton-data'
import { Card, CardBody } from '@/components/talenton/primitives'
import { updateUnderwritingOverride, verifyDocument } from '@/lib/api-service'

export function UnderwriterDashboardView({
  application,
  onUpdateApplication,
  onRouteToCommittee,
}: {
  application: Application
  onUpdateApplication: (updated: Partial<Application>) => void
  onRouteToCommittee: () => void
}) {
  const [classification, setClassification] = useState(application.applicantType || 'individual')
  const [multiplier, setMultiplier] = useState(application.multiplier || 3)
  const [tenure, setTenure] = useState(application.tenureMonths || 12)
  const [requestedCapital, setRequestedCapital] = useState(application.principal || 15_000_000)
  const [savingsBalance, setSavingsBalance] = useState(application.savingsBalance || 4_000_000)
  const [basicPay, setBasicPay] = useState(application.monthlyIncome || 2_500_000)
  const [monthlyDeductions, setMonthlyDeductions] = useState(application.monthlyDebt || 500_000)
  
  const [documents, setDocuments] = useState<DocumentSlot[]>(
    application.documents || [
      { id: 'id', label: 'National ID / NIN', hint: 'Mandatory KYC identification.', required: true, status: 'VERIFIED', fileName: 'national_id.pdf' },
      { id: 'payslip', label: 'Certified Payslip / Ledger', hint: 'Payroll data.', required: true, status: 'PENDING', fileName: 'payslip_aug2026.pdf' },
      { id: 'guarantor', label: 'Guarantor Consent Letter', hint: 'Social collateral.', required: true, status: 'VERIFIED', fileName: 'guarantor_consent.pdf' },
    ]
  )

  const [guarantors, setGuarantors] = useState<Guarantor[]>(application.guarantors || [
    { id: 'g1', name: 'Kato Joseph', memberId: 'M-1104', pledgedShares: 8_000_000, availableShares: 8_000_000 },
    { id: 'g2', name: 'Namatovu Sarah', memberId: 'M-2309', pledgedShares: 5_000_000, availableShares: 9_500_000 },
  ])

  // Qualitative Audit State
  const [crbCategory, setCrbCategory] = useState(application.crbCategory || 'Category B: Minor Delinquencies (< 30 Days)')
  const [crbScore, setCrbScore] = useState(application.crbScore || 685)
  const [characterAudit, setCharacterAudit] = useState(application.fieldAuditCharacter || 'KYC verified, market association references passed.')
  const [capacityAudit, setCapacityAudit] = useState(application.fieldAuditCapacity || 'OCR reconstructed revenue matches declared flows.')
  const [collateralAudit, setCollateralAudit] = useState(application.fieldAuditCollateral || 'Business stocks or social assets physically validated.')

  const [showAddGuarantorModal, setShowAddGuarantorModal] = useState(false)
  const [newGName, setNewGName] = useState('')
  const [newGMemberId, setNewGMemberId] = useState('')
  const [newGPledged, setNewGPledged] = useState('')
  const [isSigning, setIsSigning] = useState(false)

  // ----------------------------------------------------
  // Guardrail Check Engine Computations
  // ----------------------------------------------------
  const maxCap = savingsBalance * multiplier
  const estMonthlyPayment = tenure > 0 ? (requestedCapital / tenure) : 0
  const totalMonthlyCommitments = monthlyDeductions + estMonthlyPayment
  const dtiRatio = basicPay > 0 ? (totalMonthlyCommitments / basicPay) * 100 : 0
  const residualNetPay = basicPay - totalMonthlyCommitments

  const depositMultiplierPassed = requestedCapital <= maxCap
  const oneThirdPayPassed = basicPay > 0 && residualNetPay >= basicPay / 3
  const totalPledged = guarantors.reduce((acc, g) => acc + g.pledgedShares, 0)
  const requiredGuarantorCover = Math.max(0, requestedCapital - savingsBalance)
  const guarantorCoverPassed = totalPledged >= requiredGuarantorCover

  const overallPassed = depositMultiplierPassed && oneThirdPayPassed && guarantorCoverPassed

  // Verify Document Action
  async function handleVerifyDoc(slotId: string, status: 'VERIFIED' | 'REJECTED') {
    const updated = documents.map((d) => (d.id === slotId ? { ...d, status } : d))
    setDocuments(updated)
    onUpdateApplication({ documents: updated })
    await verifyDocument(application.reference, slotId, status)
  }

  function handleAddGuarantorSubmit() {
    if (!newGName || !newGPledged) return
    const newG: Guarantor = {
      id: `g-${Date.now()}`,
      name: newGName,
      memberId: newGMemberId || `M-${Math.floor(1000 + Math.random() * 9000)}`,
      pledgedShares: Number(newGPledged) || 0,
      availableShares: Number(newGPledged) || 0,
    }
    const updated = [...guarantors, newG]
    setGuarantors(updated)
    onUpdateApplication({ guarantors: updated })
    setNewGName('')
    setNewGMemberId('')
    setNewGPledged('')
    setShowAddGuarantorModal(false)
  }

  function handleRemoveGuarantor(id: string) {
    const updated = guarantors.filter((g) => g.id !== id)
    setGuarantors(updated)
    onUpdateApplication({ guarantors: updated })
  }

  async function handleSignAndRoute() {
    setIsSigning(true)
    const updatedFields: Partial<Application> = {
      applicantType: classification,
      multiplier,
      tenureMonths: tenure,
      principal: requestedCapital,
      savingsBalance,
      monthlyIncome: basicPay,
      monthlyDebt: monthlyDeductions,
      basicMonthlyPay: basicPay,
      monthlyDeductions,
      dtiNetRatio: dtiRatio,
      netTakeHome: residualNetPay,
      guardrailDepositMultiplierPassed: depositMultiplierPassed,
      guardrailOneThirdPayPassed: oneThirdPayPassed,
      guardrailGuarantorPassed: guarantorCoverPassed,
      verdict: overallPassed ? 'APPROVED' : 'DECLINED',
      guarantors,
      crbCategory,
      crbScore,
      fieldAuditCharacter: characterAudit,
      fieldAuditCapacity: capacityAudit,
      fieldAuditCollateral: collateralAudit,
      appraisalOfficer: 'Agaba Collins (Risk Division)',
      securitySignature: 'OTP Signed (Verified)',
      stage: 'committee',
      status: 'in_review',
      statusNote: 'Underwriting audit completed and digitally signed. Awaiting Committee Board Quorum vote.',
    }
    const underwritingResult = await updateUnderwritingOverride(application.reference, {
      applicantType: classification,
      multiplier,
      tenureMonths: tenure,
      requestedPrincipal: requestedCapital,
      savingsBalance,
      basicMonthlyPay: basicPay,
      monthlyDeductions,
      dtiRatio,
      netTakeHome: residualNetPay,
      verdict: overallPassed ? 'APPROVED' : 'DECLINED',
      guarantors,
    })
    onUpdateApplication({ ...updatedFields, ...(underwritingResult || {}) })
    setIsSigning(false)
    if (underwritingResult?.counterOfferStatus === 'PENDING') return
    onRouteToCommittee()
  }

  return (
    <div className="space-y-8 max-w-6xl mx-auto">
      
      {/* File Header */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-4 border-b border-gray-200">
        <div>
          <div className="flex items-center gap-3">
            <h2 className="text-2xl font-serif font-bold text-[#103a27]">{application.fullName}</h2>
            <span className="font-mono text-xs bg-[#0d2a1c] text-[#a4cc44] px-2.5 py-1 rounded-full font-bold">
              {application.reference}
            </span>
          </div>
          <p className="text-xs text-gray-500 mt-1">
            SACCO ID: <strong className="text-gray-700">{application.memberId}</strong> &bull; Facility: <strong className="text-gray-700">{application.purpose}</strong> &bull; Submitted: {application.submittedOn}
          </p>
        </div>

        <div className="flex items-center gap-2">
          <span className={`px-3 py-1 rounded-full text-xs font-bold ${
            overallPassed ? 'bg-emerald-100 text-emerald-800' : 'bg-rose-100 text-rose-800'
          }`}>
            Guardrail Engine: {overallPassed ? 'RECOMMENDED FOR APPROVAL' : 'GUARDRAIL BREACH / DECLINED'}
          </span>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-12">
        
        {/* LEFT COLUMN: Document Verification & Parameters Override */}
        <div className="lg:col-span-6 space-y-6">
          
          {/* Document Verification Card */}
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                <div className="flex items-center gap-2">
                  <FileCheck className="size-4 text-[#103a27]" />
                  <h3 className="font-serif text-sm font-bold text-[#103a27]">Compliance & Document Verification</h3>
                </div>
                <span className="text-xs text-gray-400 font-medium">Click to verify</span>
              </div>

              <div className="space-y-3">
                {documents.map((doc) => {
                  const isVerified = doc.status === 'VERIFIED'
                  return (
                    <div key={doc.id} className="flex items-center justify-between p-3.5 rounded-xl border border-gray-100 bg-[#f4f5f4]">
                      <div>
                        <p className="text-xs font-bold text-[#103a27]">{doc.label}</p>
                        <p className="text-[0.65rem] text-gray-500">{doc.fileName || 'Uploaded file'}</p>
                      </div>

                      <div className="flex items-center gap-1.5">
                        <button
                          type="button"
                          onClick={() => handleVerifyDoc(doc.id, isVerified ? 'REJECTED' : 'VERIFIED')}
                          className={`px-3 py-1 rounded-full text-xs font-bold transition-all ${
                            isVerified 
                              ? 'bg-emerald-600 text-white hover:bg-emerald-700' 
                              : 'bg-gray-200 text-gray-700 hover:bg-emerald-100 hover:text-emerald-800'
                          }`}
                        >
                          {isVerified ? 'Verified ✓' : 'Mark Verified'}
                        </button>
                      </div>
                    </div>
                  )
                })}
              </div>
            </CardBody>
          </Card>

          {/* Underwriter Parameter Overrides */}
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                <div className="flex items-center gap-2">
                  <ShieldCheck className="size-4 text-[#103a27]" />
                  <h3 className="font-serif text-sm font-bold text-[#103a27]">Live Policy Overrides</h3>
                </div>
                <span className="text-[0.65rem] font-bold uppercase text-amber-700 bg-amber-50 px-2 py-0.5 rounded-full">
                  Real-time Sync
                </span>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-1">
                  <label className="text-[0.65rem] font-bold uppercase text-gray-500">Approved Principal (UGX)</label>
                  <input
                    type="number"
                    value={requestedCapital}
                    onChange={(e) => {
                      const val = Number(e.target.value)
                      setRequestedCapital(val)
                      onUpdateApplication({ principal: val })
                    }}
                    className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-[0.65rem] font-bold uppercase text-gray-500">Multiplier ({multiplier.toFixed(1)}x)</label>
                  <input
                    type="range"
                    min="2"
                    max="5"
                    step="0.25"
                    value={multiplier}
                    onChange={(e) => {
                      const val = parseFloat(e.target.value)
                      setMultiplier(val)
                      onUpdateApplication({ multiplier: val })
                    }}
                    className="w-full accent-[#103a27] mt-3"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-[0.65rem] font-bold uppercase text-gray-500">Tenure (Months)</label>
                  <input
                    type="number"
                    value={tenure}
                    onChange={(e) => {
                      const val = Number(e.target.value)
                      setTenure(val)
                      onUpdateApplication({ tenureMonths: val })
                    }}
                    className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>

                <div className="space-y-1">
                  <label className="text-[0.65rem] font-bold uppercase text-gray-500">Savings Base (UGX)</label>
                  <input
                    type="number"
                    value={savingsBalance}
                    onChange={(e) => {
                      const val = Number(e.target.value)
                      setSavingsBalance(val)
                      onUpdateApplication({ savingsBalance: val })
                    }}
                    className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-bold text-gray-800 font-mono focus:outline-none focus:border-[#103a27]"
                  />
                </div>
              </div>
            </CardBody>
          </Card>

          {/* Guarantor Coverage Management */}
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                <div className="flex items-center gap-2">
                  <UserCheck className="size-4 text-[#103a27]" />
                  <h3 className="font-serif text-sm font-bold text-[#103a27]">Guarantor Coverage Audit</h3>
                </div>
                <button
                  type="button"
                  onClick={() => setShowAddGuarantorModal(true)}
                  className="flex items-center gap-1 rounded-full bg-[#103a27] text-white px-3 py-1 text-xs font-bold hover:bg-[#1a5235]"
                >
                  <Plus className="size-3" />
                  Add Guarantor
                </button>
              </div>

              <div className="space-y-2.5">
                {guarantors.map((g) => (
                  <div key={g.id} className="flex items-center justify-between p-3 rounded-xl border border-gray-100 bg-[#f4f5f4]">
                    <div>
                      <p className="text-xs font-bold text-[#103a27]">{g.name} <span className="text-gray-500 font-mono text-[0.65rem]">({g.memberId})</span></p>
                      <p className="text-[0.65rem] text-gray-500 mt-0.5">Pledged: <strong className="text-gray-800 font-mono">{formatUGX(g.pledgedShares)}</strong></p>
                    </div>
                    <button
                      type="button"
                      onClick={() => handleRemoveGuarantor(g.id)}
                      className="text-gray-400 hover:text-rose-600 p-1"
                    >
                      <Trash2 className="size-3.5" />
                    </button>
                  </div>
                ))}
              </div>

              <div className="pt-2 flex justify-between text-xs font-semibold">
                <span className="text-gray-500">Total Pledged Cover:</span>
                <span className={`font-mono font-bold ${guarantorCoverPassed ? 'text-emerald-700' : 'text-rose-600'}`}>
                  {formatUGX(totalPledged)} / {formatUGX(requiredGuarantorCover)} required
                </span>
              </div>
            </CardBody>
          </Card>
        </div>

        {/* RIGHT COLUMN: Guardrail Check Engine & Qualitative Audits */}
        <div className="lg:col-span-6 space-y-6">
          
          {/* Automated Guardrail Check Engine Card */}
          <div className="rounded-2xl bg-[#0d2a1c] p-6 text-white shadow-xl space-y-4">
            <div className="flex items-center justify-between pb-3 border-b border-white/10">
              <div className="flex items-center gap-2">
                <ShieldCheck className="size-5 text-[#a4cc44]" />
                <h3 className="font-serif text-sm font-bold text-white">Underwriting Guardrail Check Engine</h3>
              </div>
              <span className="text-[0.65rem] font-bold uppercase text-[#a4cc44] bg-white/10 px-2 py-0.5 rounded-full">
                Automated
              </span>
            </div>

            <div className="space-y-3">
              {/* Check 1: Multiplier Cap */}
              <div className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/10">
                <div>
                  <p className="text-xs font-bold text-white">Deposit Multiplier Bound</p>
                  <p className="text-[0.65rem] text-gray-300 mt-0.5">Cap: {formatUGX(maxCap)} ({multiplier}x savings)</p>
                </div>
                <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                  depositMultiplierPassed ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-300'
                }`}>
                  {depositMultiplierPassed ? 'PASSED ✓' : 'BREACH ✕'}
                </span>
              </div>

              {/* Check 2: 1/3 Statutory Net Pay */}
              <div className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/10">
                <div>
                  <p className="text-xs font-bold text-white">1/3 Statutory Net-Pay Check</p>
                  <p className="text-[0.65rem] text-gray-300 mt-0.5">Residual Take-Home: {formatUGX(residualNetPay)} (DTI: {dtiRatio.toFixed(1)}%)</p>
                </div>
                <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                  oneThirdPayPassed ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-300'
                }`}>
                  {oneThirdPayPassed ? 'PASSED ✓' : 'VIOLATION ✕'}
                </span>
              </div>

              {/* Check 3: Guarantor Coverage */}
              <div className="flex items-center justify-between p-3 rounded-xl bg-white/5 border border-white/10">
                <div>
                  <p className="text-xs font-bold text-white">Guarantor & Share Cover</p>
                  <p className="text-[0.65rem] text-gray-300 mt-0.5">Coverage ratio: {guarantorCoverPassed ? '100% Secured' : 'Uncollateralized Gap'}</p>
                </div>
                <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                  guarantorCoverPassed ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-300'
                }`}>
                  {guarantorCoverPassed ? 'PASSED ✓' : 'DEFICIT ✕'}
                </span>
              </div>
            </div>
          </div>

          {/* Credit Officer Qualitative Audits Card */}
          <Card className="border-none shadow-sm rounded-2xl bg-white">
            <CardBody className="p-6 space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-gray-100">
                <div className="flex items-center gap-2">
                  <Award className="size-4 text-[#103a27]" />
                  <h3 className="font-serif text-sm font-bold text-[#103a27]">Credit Officer Qualitative Audits</h3>
                </div>
                <span className="text-xs font-bold text-emerald-800 bg-emerald-50 px-2.5 py-0.5 rounded-full">
                  CRB Verified
                </span>
              </div>

              {/* CRB Record Section */}
              <div className="rounded-xl border border-gray-100 bg-[#f4f5f4] p-4 space-y-2">
                <div className="flex items-center justify-between">
                  <p className="text-xs font-bold text-[#103a27]">Credit Reference Bureau (CRB) Record</p>
                  <span className="font-mono text-xs font-bold text-amber-700 bg-amber-100 px-2 py-0.5 rounded">
                    Score: {crbScore}/900
                  </span>
                </div>
                <p className="text-xs font-semibold text-gray-700">
                  {crbCategory}
                </p>
                <p className="text-[0.65rem] text-gray-500">
                  No active default records. Historic slow repayment resolved within statutory grace period.
                </p>
              </div>

              {/* On-Site Field Audit Milestones */}
              <div className="space-y-2.5 pt-1">
                <p className="text-[0.65rem] font-bold uppercase tracking-wider text-gray-400">On-Site Field Audit Milestones</p>
                
                <div className="p-3 rounded-xl border border-gray-100 bg-white space-y-1">
                  <p className="text-xs font-bold text-gray-800 flex items-center gap-1.5">
                    <CheckCircle2 className="size-3.5 text-emerald-600" />
                    Character Assessment
                  </p>
                  <p className="text-[0.65rem] text-gray-500">{characterAudit}</p>
                </div>

                <div className="p-3 rounded-xl border border-gray-100 bg-white space-y-1">
                  <p className="text-xs font-bold text-gray-800 flex items-center gap-1.5">
                    <CheckCircle2 className="size-3.5 text-emerald-600" />
                    Capacity Validation
                  </p>
                  <p className="text-[0.65rem] text-gray-500">{capacityAudit}</p>
                </div>

                <div className="p-3 rounded-xl border border-gray-100 bg-white space-y-1">
                  <p className="text-xs font-bold text-gray-800 flex items-center gap-1.5">
                    <CheckCircle2 className="size-3.5 text-emerald-600" />
                    Collateral & Stocks
                  </p>
                  <p className="text-[0.65rem] text-gray-500">{collateralAudit}</p>
                </div>
              </div>

              {/* Digital Sign-off & Route Action */}
              <div className="pt-4 border-t border-gray-100 space-y-3">
                <div className="flex items-center justify-between text-xs">
                  <span className="text-gray-500">Appraisal Officer Signature:</span>
                  <span className="font-semibold text-emerald-800 flex items-center gap-1">
                    <CheckCircle2 className="size-3 text-emerald-600" /> OTP Verified (Agaba Collins)
                  </span>
                </div>

                <button
                  type="button"
                  onClick={handleSignAndRoute}
                  disabled={isSigning}
                  className="w-full flex items-center justify-center gap-2 rounded-full bg-[#103a27] py-3.5 text-xs font-bold text-white shadow-md hover:bg-[#1a5235] transition-all cursor-pointer"
                >
                  <Send className="size-3.5" />
                  {isSigning ? 'Signing & Routing...' : 'Digitally Sign & Route to Committee Board'}
                </button>
              </div>
            </CardBody>
          </Card>
        </div>

      </div>

      {/* Add Guarantor Modal */}
      {showAddGuarantorModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4">
          <div className="w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl space-y-4 animate-scaleUp">
            <h4 className="font-serif text-base font-bold text-[#103a27]">Add Guarantor Commitment</h4>
            
            <div className="space-y-3">
              <div>
                <label className="text-xs font-bold text-gray-700">Guarantor Name</label>
                <input
                  type="text"
                  placeholder="e.g. Kato Joseph"
                  value={newGName}
                  onChange={(e) => setNewGName(e.target.value)}
                  className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-semibold focus:outline-none focus:border-[#103a27]"
                />
              </div>

              <div>
                <label className="text-xs font-bold text-gray-700">Member ID</label>
                <input
                  type="text"
                  placeholder="e.g. M-1104"
                  value={newGMemberId}
                  onChange={(e) => setNewGMemberId(e.target.value)}
                  className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-semibold focus:outline-none focus:border-[#103a27]"
                />
              </div>

              <div>
                <label className="text-xs font-bold text-gray-700">Pledged Shares (UGX)</label>
                <input
                  type="number"
                  placeholder="e.g. 5000000"
                  value={newGPledged}
                  onChange={(e) => setNewGPledged(e.target.value)}
                  className="w-full rounded-xl border border-gray-200 p-2.5 text-xs font-bold text-[#103a27] font-mono focus:outline-none focus:border-[#103a27]"
                />
              </div>
            </div>

            <div className="flex justify-end gap-2 pt-2">
              <button
                type="button"
                onClick={() => setShowAddGuarantorModal(false)}
                className="rounded-full bg-gray-100 px-4 py-2 text-xs font-semibold text-gray-600 hover:bg-gray-200"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleAddGuarantorSubmit}
                className="rounded-full bg-[#103a27] px-5 py-2 text-xs font-bold text-white hover:bg-[#1a5235]"
              >
                Add Guarantor
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  )
}
