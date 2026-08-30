// Talanton — shared types and data structures for SACCO Credit Pipeline

export type ApplicantType = 'individual' | 'cooperative'

export type RoleType = 'applicant' | 'underwriter' | 'committee'

export type StageKey =
  | 'draft'
  | 'verification'
  | 'underwriting'
  | 'committee'
  | 'disbursed'

export const STAGES: { key: StageKey; label: string; caption: string }[] = [
  { key: 'draft', label: '1. Draft', caption: 'Preparing request' },
  { key: 'verification', label: '2. Verification', caption: 'Documents checked' },
  { key: 'underwriting', label: '3. Underwriting Audit', caption: 'Risk officer review' },
  { key: 'committee', label: '4. Committee Vote', caption: 'Board authorization' },
  { key: 'disbursed', label: '5. Disbursed', caption: 'Funds released' },
]

export type ApplicationStatus =
  | 'draft'
  | 'submitted'
  | 'in_review'
  | 'approved'
  | 'declined'
  | 'disbursed'
  | 'counter_offer_pending'

export interface DocumentSlot {
  id: string
  label: string
  hint: string
  required: boolean
  fileName?: string
  status?: 'VERIFIED' | 'PENDING' | 'MISSING' | 'REJECTED'
}

export interface Guarantor {
  id: string
  name: string
  memberId: string
  pledgedShares: number
  availableShares?: number
}

export interface ApplicationDraft {
  // profile
  applicantType: ApplicantType
  fullName: string
  memberId: string
  phone: string
  email: string
  savingsBalance: number
  // loan
  principal: number
  purpose: string
  tenureMonths: number
  monthlyIncome: number
  monthlyDebt: number
  multiplier: number
  // supporting
  documents: DocumentSlot[]
  guarantors: Guarantor[]
}

export interface BoardMemberVote {
  id: string
  name: string
  role: string
  vote: 'APPROVE' | 'REJECT' | 'ABSTAIN' | null
}

export interface PortfolioLoan {
  reference: string
  borrowerName: string
  borrowerMeta: string
  type: 'BOSA' | 'SME'
  principal: number
  repaymentProgress?: string
  dueDate?: string
  arrears?: number
  status: 'PENDING' | 'REPAYING' | 'APPROVED' | 'COMPLETED' | 'REJECTED'
  actionLabel?: string
  isLocked?: boolean
}

export interface CreditPassportMember {
  id: string
  name: string
  memberId: string
  classification: string // BOSA | SME
  tier: 'PLATINUM' | 'GOLD' | 'SILVER'
  trustScore: number
  onTimeRatePct: number
  loansCompleted: number
  totalRepaid: number
  currentLimit: number
  lastLoanDate: string
}

export interface UserProfile {
  id: string
  fullName: string
  memberId: string
  email: string
  phone: string
  address: string
  employerOrBusiness: string
  monthlyIncome: number
  role: RoleType
}

export const INITIAL_USER_PROFILE: UserProfile = {
  id: 'usr-001',
  fullName: 'Amina K. Nakamya',
  memberId: 'M-8842',
  email: 'applicant@talanton.io',
  phone: '+256 701 445 889',
  address: 'Plot 14 Jinja Road, Kampala, Uganda',
  employerOrBusiness: 'Grace Retail & General Supplies Ltd',
  monthlyIncome: 2_500_000,
  role: 'applicant',
}

export interface Application extends ApplicationDraft {
  id: string
  reference: string
  status: ApplicationStatus
  stage: StageKey
  submittedOn: string
  statusNote: string
  // Underwriter & Guardrails Data
  basicMonthlyPay?: number
  monthlyDeductions?: number
  dtiNetRatio?: number
  netTakeHome?: number
  guardrailDepositMultiplierPassed?: boolean
  guardrailOneThirdPayPassed?: boolean
  guardrailGuarantorPassed?: boolean
  verdict?: 'APPROVED' | 'DECLINED' | 'PENDING'
  counterOfferPrincipal?: number
  counterOfferTenureMonths?: number
  counterOfferReason?: string
  counterOfferStatus?: 'NONE' | 'PENDING' | 'ACCEPTED' | 'DECLINED'
  applicantConsentAt?: string
  applicantConsentReceived?: boolean
  appraisalOfficer?: string
  securitySignature?: string
  committeeVotes?: BoardMemberVote[]
  
  // Qualitative Audits
  crbCategory?: string
  crbScore?: number
  fieldAuditCharacter?: string
  fieldAuditCapacity?: string
  fieldAuditCollateral?: string
  disbursedAt?: string
}

export const CLASSIFICATION_LABEL: Record<ApplicantType, string> = {
  individual: 'BOSA Member (Savings Anchor)',
  cooperative: 'Cooperative / SME',
}

export function makeDocumentSlots(type: ApplicantType): DocumentSlot[] {
  const shared: DocumentSlot[] = [
    {
      id: 'id',
      label: 'National ID / NIN',
      hint: 'Mandatory KYC identification.',
      required: true,
      status: 'VERIFIED',
      fileName: 'national_id_front_back.pdf',
    },
    {
      id: 'guarantor',
      label: 'Signed Guarantor Consent Letter',
      hint: 'Social collateral audit confirmation.',
      required: true,
      status: 'VERIFIED',
      fileName: 'guarantor_consent_signed.pdf',
    },
    {
      id: 'tax',
      label: 'Tax Clearance (URA / KRA)',
      hint: 'Local statutory filing certificate.',
      required: false,
      status: 'PENDING',
    },
  ]

  if (type === 'individual') {
    return [
      shared[0],
      {
        id: 'payslip',
        label: 'Certified Payslip / Business Ledger',
        hint: 'Reconstructed cashbook / payroll data.',
        required: true,
        status: 'VERIFIED',
        fileName: 'payslip_q2_2026.pdf',
      },
      shared[1],
      shared[2],
    ]
  }

  return [
    shared[0],
    {
      id: 'registration',
      label: 'Business Registration Certificate',
      hint: 'Incorporation & Tax Pin documentation.',
      required: true,
      status: 'VERIFIED',
      fileName: 'coop_registration_cert.pdf',
    },
    {
      id: 'ledger',
      label: 'Business Ledger / Bank Statements',
      hint: 'Last 6 months audited bank flows.',
      required: true,
      status: 'VERIFIED',
      fileName: 'bank_statement_6m.pdf',
    },
    shared[1],
    shared[2],
  ]
}

export function emptyDraft(): ApplicationDraft {
  return {
    applicantType: 'individual',
    fullName: 'Nakamya Grace',
    memberId: 'M-8842',
    phone: '+256 701 445 889',
    email: 'nakamya.grace@example.com',
    savingsBalance: 4_000_000,
    principal: 15_000_000,
    purpose: 'Expand retail inventory',
    tenureMonths: 12,
    monthlyIncome: 2_500_000,
    monthlyDebt: 500_000,
    multiplier: 3,
    documents: makeDocumentSlots('individual'),
    guarantors: [
      { id: 'g1', name: 'Kato Joseph', memberId: 'M-1104', pledgedShares: 8_000_000, availableShares: 8_000_000 },
      { id: 'g2', name: 'Namatovu Sarah', memberId: 'M-2309', pledgedShares: 5_000_000, availableShares: 9_500_000 },
    ],
  }
}

export const INITIAL_APPLICATION: Application = {
  ...emptyDraft(),
  id: 'app-0941a',
  reference: 'LA-2026-0941A',
  status: 'in_review',
  stage: 'verification',
  submittedOn: 'Aug 04, 2026',
  statusNote: 'Current Phase: Documents Uploaded & Auditing',
  basicMonthlyPay: 2_500_000,
  monthlyDeductions: 500_000,
  dtiNetRatio: 82.0,
  netTakeHome: 450_000,
  guardrailDepositMultiplierPassed: false,
  guardrailOneThirdPayPassed: false,
  guardrailGuarantorPassed: true,
  verdict: 'DECLINED',
  appraisalOfficer: 'Agaba Collins (Risk Division)',
  securitySignature: 'OTP Signed (Verified)',
  committeeVotes: [
    { id: 'v1', name: 'Chairman', role: 'Chairperson', vote: 'APPROVE' },
    { id: 'v2', name: 'Sec. General', role: 'Risk Head', vote: 'APPROVE' },
    { id: 'v3', name: 'Mrs. Nabukenya', role: 'Credit Officer', vote: 'APPROVE' },
    { id: 'v4', name: 'Dr. Ochieng', role: 'Treasurer', vote: 'ABSTAIN' },
    { id: 'v5', name: 'Eng. Museveni', role: 'Board Member', vote: 'ABSTAIN' },
  ],
}

export const SEED_PORTFOLIO_LOANS: PortfolioLoan[] = [
  {
    reference: 'LA-2026-0941A',
    borrowerName: 'Nakamya Grace (M-8842)',
    borrowerMeta: 'by Agaba Collins',
    type: 'BOSA',
    principal: 15_000_000,
    status: 'PENDING',
    actionLabel: 'Pending Vote',
  },
  {
    reference: 'LA-2026-0938B',
    borrowerName: 'Ssemakula Enterprises Ltd',
    borrowerMeta: 'by Nabirye Faith',
    type: 'SME',
    principal: 42_000_000,
    status: 'PENDING',
    actionLabel: 'Pending Vote',
  },
  {
    reference: 'LA-2026-0912C',
    borrowerName: 'Kato Joseph (M-1104)',
    borrowerMeta: 'Direct BOSA Loan',
    type: 'BOSA',
    principal: 8_000_000,
    repaymentProgress: '4/10 paid',
    dueDate: 'Due: Feb 05, 2026',
    status: 'REPAYING',
    isLocked: true,
  },
  {
    reference: 'LA-2026-0899D',
    borrowerName: 'Auma Florence (M-4511)',
    borrowerMeta: 'Direct BOSA Loan',
    type: 'BOSA',
    principal: 6_500_000,
    repaymentProgress: '6/8 paid',
    dueDate: 'Due: Feb 12, 2026',
    arrears: 320_000,
    status: 'REPAYING',
    isLocked: true,
  },
  {
    reference: 'LA-2026-0871E',
    borrowerName: 'Mukasa Agro Supplies',
    borrowerMeta: 'SME Pipeline',
    type: 'SME',
    principal: 28_000_000,
    status: 'APPROVED',
    actionLabel: 'Disburse',
  },
  {
    reference: 'LA-2025-0842F',
    borrowerName: 'Namatovu Sarah (M-2309)',
    borrowerMeta: 'Direct BOSA Loan',
    type: 'BOSA',
    principal: 4_000_000,
    repaymentProgress: '6/6 paid',
    status: 'COMPLETED',
    isLocked: true,
  },
  {
    reference: 'LA-2025-0803G',
    borrowerName: 'Okello Trading Co.',
    borrowerMeta: 'SME Pipeline',
    type: 'SME',
    principal: 55_000_000,
    status: 'REJECTED',
    isLocked: true,
  },
]

export const SEED_PASSPORT_MEMBERS: CreditPassportMember[] = [
  {
    id: 'cp1',
    name: 'Namatovu Sarah',
    memberId: 'M-2309',
    classification: 'BOSA',
    tier: 'PLATINUM',
    trustScore: 92,
    onTimeRatePct: 100,
    loansCompleted: 4,
    totalRepaid: 18_500_000,
    currentLimit: 25_000_000,
    lastLoanDate: 'Dec 2025',
  },
  {
    id: 'cp2',
    name: 'Ssemakula Agro Ltd',
    memberId: 'SME-0412',
    classification: 'SME',
    tier: 'PLATINUM',
    trustScore: 88,
    onTimeRatePct: 97,
    loansCompleted: 3,
    totalRepaid: 76_000_000,
    currentLimit: 90_000_000,
    lastLoanDate: 'Nov 2025',
  },
  {
    id: 'cp3',
    name: 'Kato Joseph',
    memberId: 'M-1104',
    classification: 'BOSA',
    tier: 'GOLD',
    trustScore: 84,
    onTimeRatePct: 95,
    loansCompleted: 2,
    totalRepaid: 9_200_000,
    currentLimit: 15_000_000,
    lastLoanDate: 'Oct 2025',
  },
  {
    id: 'cp4',
    name: 'Auma Florence',
    memberId: 'M-4511',
    classification: 'BOSA',
    tier: 'GOLD',
    trustScore: 76,
    onTimeRatePct: 89,
    loansCompleted: 3,
    totalRepaid: 11_400_000,
    currentLimit: 12_000_000,
    lastLoanDate: 'Jan 2026',
  },
  {
    id: 'cp5',
    name: 'Mukasa Peter',
    memberId: 'M-9022',
    classification: 'BOSA',
    tier: 'SILVER',
    trustScore: 71,
    onTimeRatePct: 92,
    loansCompleted: 1,
    totalRepaid: 3_500_000,
    currentLimit: 6_000_000,
    lastLoanDate: 'Sep 2025',
  },
  {
    id: 'cp6',
    name: 'Kiiza Wholesale Co.',
    memberId: 'SME-0755',
    classification: 'SME',
    tier: 'GOLD',
    trustScore: 83,
    onTimeRatePct: 94,
    loansCompleted: 2,
    totalRepaid: 44_000_000,
    currentLimit: 60_000_000,
    lastLoanDate: 'Dec 2025',
  },
]

export const SEED_APPLICATIONS: Application[] = []

export function formatUGX(value: number): string {
  return new Intl.NumberFormat('en-UG', {
    style: 'currency',
    currency: 'UGX',
    maximumFractionDigits: 0,
  }).format(value || 0)
}

export function stageIndex(stage: StageKey): number {
  return STAGES.findIndex((s) => s.key === stage)
}

export function savingsCap(savings: number, multiplier: number): number {
  return Math.round(savings * multiplier)
}

export const STATUS_META: Record<
  ApplicationStatus,
  { label: string; tone: 'muted' | 'warning' | 'primary' | 'destructive' | 'success' }
> = {
  draft: { label: 'Draft', tone: 'muted' },
  submitted: { label: 'Submitted', tone: 'warning' },
  in_review: { label: 'In review', tone: 'warning' },
  counter_offer_pending: { label: 'Counter-offer', tone: 'warning' },
  approved: { label: 'Approved', tone: 'success' },
  declined: { label: 'Declined', tone: 'destructive' },
  disbursed: { label: 'Disbursed', tone: 'success' },
}
