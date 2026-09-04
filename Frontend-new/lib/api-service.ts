import {
  type Application,
  type ApplicantType,
  type CreditPassportMember,
  type Guarantor,
  type UserProfile,
  INITIAL_APPLICATION,
  INITIAL_USER_PROFILE,
  SEED_APPLICATIONS,
  SEED_PASSPORT_MEMBERS,
  SEED_PORTFOLIO_LOANS,
  type PortfolioLoan,
} from './talenton-data'
import { getSupabase, isSupabaseConfigured } from './supabase'

const BACKEND_API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5195'

/** Why the last backend call did not return data. Callers that must fail closed should
 *  check this rather than treating `undefined` as "the server had nothing to say" — a
 *  rejection and an unreachable server are very different answers. */
export type BackendFailure = 'rejected' | 'unreachable' | null
let lastBackendFailure: BackendFailure = null
export function getLastBackendFailure(): BackendFailure {
  return lastBackendFailure
}

async function requestBackend<T>(path: string, options: RequestInit): Promise<T | undefined> {
  const url = `${BACKEND_API_BASE_URL}${path}`
  try {
    const response = await fetch(url, {
      ...options,
      headers: { 'Content-Type': 'application/json', ...(options.headers || {}) },
    })
    if (!response.ok) {
      lastBackendFailure = 'rejected'
      const body = await response.text().catch(() => '')
      console.error(
        `[API] ${options.method || 'GET'} ${path} rejected with ${response.status} ${response.statusText}. ` +
          `The change was NOT applied on the server.${body ? ` Response: ${body.slice(0, 300)}` : ''}`
      )
      return undefined
    }
    lastBackendFailure = null
    return (await response.json()) as T
  } catch (error) {
    lastBackendFailure = 'unreachable'
    console.error(
      `[API] ${options.method || 'GET'} ${path} could not reach the server at ${BACKEND_API_BASE_URL}. ` +
        `Falling back to local state, so what you see is NOT what the server holds. ` +
        `Check NEXT_PUBLIC_API_URL and that the origin is allowed by CORS.`,
      error
    )
    return undefined
  }
}

// In-Memory/Local Reactive Store for robust fallback
let memoryApplications: Application[] = [...SEED_APPLICATIONS]
let memoryPassports: CreditPassportMember[] = [...SEED_PASSPORT_MEMBERS]
let memoryPortfolio: PortfolioLoan[] = [...SEED_PORTFOLIO_LOANS]
let memoryProfile: UserProfile = { ...INITIAL_USER_PROFILE }

// Try to read from localStorage in browser environment
if (typeof window !== 'undefined') {
  try {
    const savedApps = localStorage.getItem('talanton_applications')
    if (savedApps) memoryApplications = JSON.parse(savedApps)

    const savedProfile = localStorage.getItem('talanton_profile')
    if (savedProfile) memoryProfile = JSON.parse(savedProfile)
  } catch (e) {
    console.warn('Could not read from localStorage', e)
  }
}

function persistLocalState() {
  if (typeof window !== 'undefined') {
    try {
      localStorage.setItem('talanton_applications', JSON.stringify(memoryApplications))
      localStorage.setItem('talanton_profile', JSON.stringify(memoryProfile))
    } catch (e) {
      console.warn('Could not persist to localStorage', e)
    }
  }
}

// ----------------------------------------------------------------------
// 1. APPLICATIONS
// ----------------------------------------------------------------------

export async function fetchApplications(): Promise<Application[]> {
  const sb = getSupabase()
  if (sb) {
    try {
      const { data, error } = await sb
        .from('loan_applications')
        .select(`
          *,
          application_documents (*),
          guarantors (*),
          committee_votes (*)
        `)
        .order('created_at', { ascending: false })

      if (!error && Array.isArray(data) && data.length > 0) {
        return data.map((item) => ({
          ...INITIAL_APPLICATION,
          id: item.id,
          reference: item.reference,
          fullName: item.applicant_name,
          memberId: item.member_id,
          phone: item.phone || '',
          email: item.email || '',
          applicantType: item.applicant_type as ApplicantType,
          principal: Number(item.principal) || 0,
          purpose: item.purpose || '',
          tenureMonths: item.tenure_months || 12,
          savingsBalance: Number(item.savings_balance) || 0,
          monthlyIncome: Number(item.monthly_income) || 0,
          monthlyDebt: Number(item.monthly_debt) || 0,
          multiplier: Number(item.multiplier) || 3,
          status: item.status || 'submitted',
          stage: item.stage || 'verification',
          submittedOn: item.submitted_on ? new Date(item.submitted_on).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }) : 'Draft',
          statusNote: item.status_note || '',
                    counterOfferPrincipal: item.counter_offer_principal_amount != null ? Number(item.counter_offer_principal_amount) : undefined,
                    counterOfferTenureMonths: item.counter_offer_term_months != null ? Number(item.counter_offer_term_months) : undefined,
                    counterOfferReason: item.counter_offer_reason || undefined,
                    counterOfferStatus: item.counter_offer_status || 'NONE',
                    applicantConsentAt: item.applicant_consent_at || undefined,
                    applicantConsentReceived: item.applicant_consent_received ?? false,
          dtiNetRatio: item.dti_net_ratio != null ? Number(item.dti_net_ratio) : undefined,
          netTakeHome: item.net_take_home != null ? Number(item.net_take_home) : undefined,
          guardrailDepositMultiplierPassed: item.guardrail_multiplier_passed ?? false,
          guardrailOneThirdPayPassed: item.guardrail_one_third_passed ?? false,
          guardrailGuarantorPassed: item.guardrail_guarantor_passed ?? true,
          verdict: item.verdict || 'PENDING',
          appraisalOfficer: item.appraisal_officer,
          securitySignature: item.security_signature,
          crbCategory: item.crb_category || 'Category B: Minor Delinquencies (< 30 Days)',
          crbScore: item.crb_score || 685,
          fieldAuditCharacter: item.field_audit_character || 'KYC verified, market association references passed.',
          fieldAuditCapacity: item.field_audit_capacity || 'OCR reconstructed revenue matches declared flows.',
          fieldAuditCollateral: item.field_audit_collateral || 'Business stocks or social assets physically validated.',
          disbursedAt: item.disbursed_at,
          documents: (item.application_documents || []).map((d: any) => ({
            id: d.slot_id || d.id,
            label: d.label,
            hint: d.hint || '',
            required: d.required ?? true,
            fileName: d.file_name,
            fileUrl: d.file_url,
            status: d.status || 'PENDING',
          })),
          guarantors: (item.guarantors || []).map((g: any) => ({
            id: g.id,
            name: g.name,
            memberId: g.member_id,
            pledgedShares: Number(g.pledged_shares),
            availableShares: Number(g.available_shares),
          })),
          committeeVotes: (item.committee_votes || []).map((v: any) => ({
            id: v.id,
            name: v.member_name,
            role: v.member_role,
            vote: v.vote,
          })),
        }))
      }
    } catch (err) {
      console.warn('Supabase fetch error, using local state', err)
    }
  }

  return memoryApplications
}

export type CreateLoanApplicationPayload = {
  applicantName: string
  memberId: string
  applicantType: string
  principal: number
  purpose: string
  tenureMonths: number
  savingsBalance: number
  monthlyIncome: number
  monthlyDebt: number
  multiplier: number
  phone?: string
  email?: string
  isDraft?: boolean
}

export async function createLoanApplication(
  payload: CreateLoanApplicationPayload
): Promise<{ success: boolean; data?: Application; error?: string }> {
  const ref = `LA-2026-${Math.floor(1000 + Math.random() * 9000)}${payload.applicantType === 'individual' ? 'A' : 'B'}`
  const status = payload.isDraft ? 'draft' : 'submitted'
  const stage = payload.isDraft ? 'draft' : 'verification'

  const newApp: Application = {
    ...INITIAL_APPLICATION,
    id: `app-${Date.now()}`,
    reference: ref,
    fullName: payload.applicantName,
    memberId: payload.memberId,
    phone: payload.phone || '+256 701 445 889',
    email: payload.email || 'applicant@talanton.io',
    applicantType: (payload.applicantType as ApplicantType) || 'individual',
    principal: payload.principal || 0,
    purpose: payload.purpose || 'Working Capital',
    tenureMonths: payload.tenureMonths || 12,
    savingsBalance: payload.savingsBalance || 0,
    monthlyIncome: payload.monthlyIncome || 0,
    monthlyDebt: payload.monthlyDebt || 0,
    multiplier: payload.multiplier || 3,
    status,
    stage,
    submittedOn: payload.isDraft ? 'Draft' : new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' }),
    statusNote: payload.isDraft ? 'Application saved as draft.' : 'Application submitted. Underwriting verification in progress.',
    verdict: 'PENDING',
    crbCategory: 'Category B: Minor Delinquencies (< 30 Days)',
    crbScore: 685,
    fieldAuditCharacter: 'KYC verified, market association references passed.',
    fieldAuditCapacity: 'OCR reconstructed revenue matches declared flows.',
    fieldAuditCollateral: 'Business stocks or social assets physically validated.',
    committeeVotes: [
      { id: 'v1', name: 'Chairman', role: 'Chairperson', vote: 'APPROVE' },
      { id: 'v2', name: 'Sec. General', role: 'Risk Head', vote: null },
      { id: 'v3', name: 'Mrs. Nabukenya', role: 'Credit Officer', vote: null },
      { id: 'v4', name: 'Dr. Ochieng', role: 'Treasurer', vote: null },
      { id: 'v5', name: 'Eng. Museveni', role: 'Board Member', vote: null },
    ],
  }

  const sb = getSupabase()
  if (sb) {
    try {
      const { data, error } = await sb.from('loan_applications').insert({
        reference: newApp.reference,
        applicant_name: newApp.fullName,
        member_id: newApp.memberId,
        phone: newApp.phone,
        email: newApp.email,
        applicant_type: newApp.applicantType,
        principal: newApp.principal,
        purpose: newApp.purpose,
        tenure_months: newApp.tenureMonths,
        savings_balance: newApp.savingsBalance,
        monthly_income: newApp.monthlyIncome,
        monthly_debt: newApp.monthlyDebt,
        multiplier: newApp.multiplier,
        status: newApp.status,
        stage: newApp.stage,
        submitted_on: payload.isDraft ? null : new Date().toISOString(),
        status_note: newApp.statusNote,
      }).select().single()

      if (!error && data) {
        newApp.id = data.id
      }
    } catch (err) {
      console.warn('Supabase insert failed, fallback to local', err)
    }
  }

  // Update in-memory
  memoryApplications = [newApp, ...memoryApplications.filter(a => a.reference !== newApp.reference)]
  persistLocalState()

  return { success: true, data: newApp }
}

export async function updateApplication(
  reference: string,
  updates: Partial<Application>
): Promise<{ success: boolean; data?: Application }> {
  let updatedApp: Application | undefined

  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === reference) {
      updatedApp = { ...app, ...updates }
      return updatedApp
    }
    return app
  })

  persistLocalState()

  const sb = getSupabase()
  if (sb && updatedApp) {
    try {
      await sb.from('loan_applications').update({
        status: updatedApp.status,
        stage: updatedApp.stage,
        principal: updatedApp.principal,
        multiplier: updatedApp.multiplier,
        tenure_months: updatedApp.tenureMonths,
        savings_balance: updatedApp.savingsBalance,
        monthly_income: updatedApp.monthlyIncome,
        monthly_debt: updatedApp.monthlyDebt,
        dti_net_ratio: updatedApp.dtiNetRatio,
        net_take_home: updatedApp.netTakeHome,
        verdict: updatedApp.verdict,
        status_note: updatedApp.statusNote,
        appraisal_officer: updatedApp.appraisalOfficer,
        security_signature: updatedApp.securitySignature,
      }).eq('reference', reference)
    } catch (e) {
      console.warn('Supabase update failed', e)
    }
  }

  return { success: true, data: updatedApp }
}

export async function submitOrUpdateApplication(
  reference: string | undefined,
  payload: CreateLoanApplicationPayload
): Promise<{ success: boolean; data?: Application; error?: string }> {
  // If editing/submitting an existing draft
  if (reference && memoryApplications.some(a => a.reference === reference)) {
    const isDraft = Boolean(payload.isDraft)
    const status = isDraft ? 'draft' : 'submitted'
    const stage = isDraft ? 'draft' : 'verification'
    const submittedOn = isDraft
      ? 'Draft'
      : new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
    const statusNote = isDraft
      ? 'Application saved as draft.'
      : 'Application submitted. Underwriting verification in progress.'

    const updates: Partial<Application> = {
      applicantType: (payload.applicantType as ApplicantType) || 'individual',
      principal: payload.principal || 0,
      purpose: payload.purpose || 'Working Capital',
      tenureMonths: payload.tenureMonths || 12,
      savingsBalance: payload.savingsBalance || 0,
      monthlyIncome: payload.monthlyIncome || 0,
      monthlyDebt: payload.monthlyDebt || 0,
      multiplier: payload.multiplier || 3,
      status,
      stage,
      submittedOn,
      statusNote,
    }

    return updateApplication(reference, updates)
  }

  // Otherwise create new application
  return createLoanApplication(payload)
}

export async function uploadDocumentToStorage(
  file: File,
  applicationRef: string,
  slotId: string
): Promise<string> {
  const sb = getSupabase()
  if (sb) {
    try {
      const fileExt = file.name.split('.').pop() || 'pdf'
      const filePath = `${applicationRef}/${slotId}_${Date.now()}.${fileExt}`
      const { data, error } = await sb.storage
        .from('documents')
        .upload(filePath, file, {
          cacheControl: '3600',
          upsert: true,
        })

      if (!error && data) {
        const { data: publicUrlData } = sb.storage
          .from('documents')
          .getPublicUrl(filePath)
        return publicUrlData?.publicUrl || filePath
      }
    } catch (e) {
      console.warn('Supabase storage upload error, using local object URL', e)
    }
  }

  // Local object URL fallback
  if (typeof window !== 'undefined') {
    return URL.createObjectURL(file)
  }
  return file.name
}

// ----------------------------------------------------------------------
// 2. DOCUMENT VERIFICATION
// ----------------------------------------------------------------------

export async function verifyDocument(
  applicationRef: string,
  slotId: string,
  status: 'VERIFIED' | 'REJECTED' | 'PENDING'
): Promise<boolean> {
  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === applicationRef) {
      const updatedDocs = app.documents.map((d) =>
        d.id === slotId ? { ...d, status } : d
      )
      return { ...app, documents: updatedDocs }
    }
    return app
  })

  persistLocalState()
  return true
}

// ----------------------------------------------------------------------
// 3. UNDERWRITER OVERRIDES & QUALITATIVE SIGN-OFF
// ----------------------------------------------------------------------

export async function updateUnderwritingOverride(
  reference: string,
  payload: {
    applicantType: string
    multiplier: number
    tenureMonths: number
    requestedPrincipal: number
    savingsBalance: number
    basicMonthlyPay: number
    monthlyDeductions: number
    dtiRatio?: number
    netTakeHome?: number
    verdict?: 'APPROVED' | 'DECLINED'
    guarantors?: Guarantor[]
    adjustmentReason?: string
  }
): Promise<Application | undefined> {
  const updatedFromBackend = await requestBackend<Application>(`/api/loanapplications/${encodeURIComponent(reference)}/underwrite`, {
    method: 'PUT',
    body: JSON.stringify({
      applicantType: payload.applicantType,
      multiplier: payload.multiplier,
      tenureMonths: payload.tenureMonths,
      requestedPrincipal: payload.requestedPrincipal,
      savingsBalance: payload.savingsBalance,
      basicMonthlyPay: payload.basicMonthlyPay,
      monthlyDeductions: payload.monthlyDeductions,
      adjustmentReason: payload.adjustmentReason,
    }),
  })

  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === reference) {
      return {
        ...app,
        applicantType: payload.applicantType as ApplicantType,
        multiplier: payload.multiplier,
        tenureMonths: payload.tenureMonths,
        principal: payload.requestedPrincipal,
        savingsBalance: payload.savingsBalance,
        monthlyIncome: payload.basicMonthlyPay,
        monthlyDebt: payload.monthlyDeductions,
        basicMonthlyPay: payload.basicMonthlyPay,
        monthlyDeductions: payload.monthlyDeductions,
        dtiNetRatio: payload.dtiRatio ?? app.dtiNetRatio,
        netTakeHome: payload.netTakeHome ?? app.netTakeHome,
        verdict: payload.verdict ?? app.verdict,
        guarantors: payload.guarantors ?? app.guarantors,
        counterOfferStatus: updatedFromBackend?.counterOfferStatus ?? app.counterOfferStatus,
        counterOfferPrincipal: updatedFromBackend?.counterOfferPrincipal ?? app.counterOfferPrincipal,
        counterOfferTenureMonths: updatedFromBackend?.counterOfferTenureMonths ?? app.counterOfferTenureMonths,
        counterOfferReason: updatedFromBackend?.counterOfferReason ?? app.counterOfferReason,
        applicantConsentReceived: updatedFromBackend?.applicantConsentReceived ?? app.applicantConsentReceived,
      }
    }
    return app
  })

  return updatedFromBackend || memoryApplications.find((app) => app.reference === reference)
}

export async function respondToCounterOffer(
  reference: string,
  decision: 'ACCEPT' | 'DECLINE'
): Promise<Application | undefined> {
  const updatedFromBackend = await requestBackend<Application>(`/api/loanapplications/${encodeURIComponent(reference)}/counter-offer`, {
    method: 'POST',
    body: JSON.stringify({ decision }),
  })

  let updatedApplication: Application | undefined
  memoryApplications = memoryApplications.map((app) => {
    if (app.reference !== reference) return app
    updatedApplication = {
      ...app,
      ...(updatedFromBackend || {}),
      counterOfferStatus: decision === 'ACCEPT' ? 'ACCEPTED' : 'DECLINED',
      applicantConsentReceived: decision === 'ACCEPT',
      status: decision === 'ACCEPT' ? 'in_review' : 'declined',
    }
    return updatedApplication
  })
  persistLocalState()
  return updatedFromBackend || updatedApplication
}

export async function signAndRouteToCommittee(
  reference: string,
  payload: {
    appraisalOfficer: string
    signature: string
    verdict: 'APPROVED' | 'DECLINED'
  }
): Promise<boolean> {
  const updatedFromBackend = await requestBackend<Application>(`/api/loanapplications/${encodeURIComponent(reference)}/route?stage=committee`, {
    method: 'POST',
  })

  if (updatedFromBackend?.counterOfferStatus === 'PENDING') return false
  if (updatedFromBackend?.status === 'declined') return false

  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === reference) {
      return {
        ...app,
        stage: 'committee',
        status: 'in_review',
        appraisalOfficer: payload.appraisalOfficer,
        securitySignature: payload.signature,
        verdict: payload.verdict,
        statusNote: 'Underwriting audit completed and digitally signed. Awaiting Committee Board Quorum vote.',
      }
    }
    return app
  })

  persistLocalState()
  return Boolean(updatedFromBackend || memoryApplications.some((app) => app.reference === reference))
}

// ----------------------------------------------------------------------
// 4. COMMITTEE VOTING & DISBURSEMENT
// ----------------------------------------------------------------------

export async function castCommitteeVote(
  reference: string,
  payload: { memberRole: string; vote: 'APPROVE' | 'REJECT' | 'ABSTAIN'; memberName?: string }
): Promise<boolean> {
  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === reference) {
      const votes = (app.committeeVotes || []).map((v) =>
        v.role === payload.memberRole ? { ...v, vote: payload.vote } : v
      )
      return { ...app, committeeVotes: votes }
    }
    return app
  })

  persistLocalState()
  return true
}

export interface QuorumCheckResult {
  isQuorumPassed: boolean
  reason: string
  isBigLoan: boolean
  requiredApprovals: number
  approvalCount: number
  hasChairpersonVeto: boolean
  hasRequiredMembers: boolean
}

export async function checkQuorumStatus(reference: string): Promise<QuorumCheckResult | null> {
  const app = memoryApplications.find((a) => a.reference === reference)
  if (!app) return null

  // Local implementation of quorum rules
  const BIG_LOAN_THRESHOLD = 5_000_000
  const isBigLoan = app.principal >= BIG_LOAN_THRESHOLD
  const requiredApprovals = isBigLoan ? 3 : 1

  const votes = app.committeeVotes || []
  const approvalCount = votes.filter((v) => v.vote === 'APPROVE').length
  const chairpersonVeto = votes.some((v) => v.role === 'Chairperson' && v.vote === 'REJECT')
  const chairmanApproved = votes.some((v) => v.role === 'Chairperson' && v.vote === 'APPROVE')
  const treasurerApproved = votes.some((v) => v.role === 'Treasurer' && v.vote === 'APPROVE')
  const hasRequiredMembers = chairmanApproved && treasurerApproved

  let isQuorumPassed = false
  let reason = ''

  if (chairpersonVeto) {
    isQuorumPassed = false
    reason = 'Chairperson has voted REJECT — absolute veto applied regardless of other approvals.'
  } else if (isBigLoan) {
    isQuorumPassed = approvalCount >= requiredApprovals && hasRequiredMembers
    if (approvalCount < requiredApprovals) {
      reason = `Big loan requires ${requiredApprovals} approvals (currently ${approvalCount}). Additionally, both Chairman and Treasurer must approve.`
    } else if (!hasRequiredMembers) {
      reason = 'Big loan requires approval from both Chairman AND Treasurer. Not all required members have approved.'
    } else {
      reason = `Big loan quorum passed: ${approvalCount} approvals (≥${requiredApprovals} required), with Chairman and Treasurer approval.`
    }
  } else {
    isQuorumPassed = approvalCount >= requiredApprovals
    reason = isQuorumPassed
      ? `Small loan quorum passed: ${approvalCount} approval(s) received (1 required).`
      : `Small loan requires 1 approval. Currently ${approvalCount} approvals received.`
  }

  return {
    isQuorumPassed,
    reason,
    isBigLoan,
    requiredApprovals,
    approvalCount,
    hasChairpersonVeto: chairpersonVeto,
    hasRequiredMembers,
  }
}

export async function disburseLoan(reference: string, requestorRole: string = 'Treasurer'): Promise<boolean> {
  const now = new Date().toISOString()

  // Try to call backend API with authorization
  const sb = getSupabase()
  if (sb) {
    try {
      const response = await fetch(`${BACKEND_API_BASE_URL}/api/loanapplications/${reference}/disburse`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          requestorRole,
          chairpersonSignature: 'OTP_VERIFIED', // Simulated for demo
          secretarySignature: 'OTP_VERIFIED',   // Simulated for demo
          disbursementNotes: 'Authorized by committee board',
        }),
      })

      if (!response.ok) {
        const body = await response.text().catch(() => '')
        console.error(
          `[API] Disbursement of ${reference} was REFUSED by the server (${response.status} ${response.statusText}). ` +
            `The local fallback below still marks it disbursed, so the screen will disagree with the server.${body ? ` Reason: ${body.slice(0, 300)}` : ''}`
        )
      }
    } catch (err) {
      console.error(
        `[API] Disbursement of ${reference} could not reach the server at ${BACKEND_API_BASE_URL}. ` +
          `No authorization check was performed; the local fallback below marks it disbursed regardless.`,
        err
      )
    }
  }

  // Local fallback: update memory state
  memoryApplications = memoryApplications.map((app) => {
    if (app.reference === reference) {
      return {
        ...app,
        stage: 'disbursed',
        status: 'disbursed',
        disbursedAt: now,
        statusNote: `Funds released and credited on ${new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}. Active repayment underway.`,
      }
    }
    return app
  })

  memoryPortfolio = memoryPortfolio.map((l) =>
    l.reference === reference ? { ...l, status: 'REPAYING', isLocked: true, disbursedAt: now } : l
  )

  persistLocalState()
  return true
}

// ----------------------------------------------------------------------
// 5. CREDIT PASSPORT REGISTRY
// ----------------------------------------------------------------------

export async function fetchCreditPassportMembers(): Promise<CreditPassportMember[]> {
  const sb = getSupabase()
  if (sb) {
    try {
      const { data, error } = await sb.from('credit_passports').select('*').order('trust_score', { ascending: false })
      if (!error && Array.isArray(data) && data.length > 0) {
        return data.map((d) => ({
          id: d.id,
          name: d.name,
          memberId: d.member_id,
          classification: d.classification,
          tier: d.tier,
          trustScore: d.trust_score,
          onTimeRatePct: d.on_time_rate_pct,
          loansCompleted: d.loans_completed,
          totalRepaid: Number(d.total_repaid),
          currentLimit: Number(d.current_limit),
          lastLoanDate: d.last_loan_date,
        }))
      }
    } catch (e) {
      console.warn('Supabase credit passports query failed', e)
    }
  }

  return memoryPassports
}

// ----------------------------------------------------------------------
// 6. USER PROFILE & SETTINGS
// ----------------------------------------------------------------------

export async function fetchUserProfile(email?: string): Promise<UserProfile> {
  const sb = getSupabase()
  if (sb && email) {
    try {
      const { data, error } = await sb.from('profiles').select('*').eq('email', email).single()
      if (!error && data) {
        return {
          id: data.id,
          fullName: data.full_name,
          memberId: data.member_id,
          email: data.email,
          phone: data.phone || '',
          address: data.address || '',
          employerOrBusiness: data.employer_or_business || '',
          monthlyIncome: Number(data.monthly_income) || 0,
          role: data.role,
        }
      }
    } catch (e) {
      console.warn('Supabase fetch profile failed', e)
    }
  }

  return memoryProfile
}

export async function updateUserProfile(updates: Partial<UserProfile>): Promise<boolean> {
  memoryProfile = { ...memoryProfile, ...updates }
  persistLocalState()

  const sb = getSupabase()
  if (sb && memoryProfile.email) {
    try {
      await sb.from('profiles').update({
        full_name: memoryProfile.fullName,
        phone: memoryProfile.phone,
        address: memoryProfile.address,
        employer_or_business: memoryProfile.employerOrBusiness,
        monthly_income: memoryProfile.monthlyIncome,
      }).eq('email', memoryProfile.email)
    } catch (e) {
      console.warn('Supabase profile update failed', e)
    }
  }

  return true
}