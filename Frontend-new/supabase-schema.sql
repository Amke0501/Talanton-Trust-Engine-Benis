-- ==============================================================================
-- TALANTON TRUST ENGINE — SUPABASE POSTGRESQL SCHEMA & INITIAL SEED
-- ==============================================================================

-- Enable UUID Extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- 1. PROFILES & SACCO MEMBERS TABLE
CREATE TABLE IF NOT EXISTS public.profiles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email TEXT UNIQUE NOT NULL,
    full_name TEXT NOT NULL,
    member_id TEXT UNIQUE NOT NULL,
    phone TEXT,
    address TEXT,
    employer_or_business TEXT,
    monthly_income NUMERIC(15, 2) DEFAULT 0,
    role TEXT NOT NULL CHECK (role IN ('applicant', 'underwriter', 'committee')),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 2. LOAN APPLICATIONS TABLE
CREATE TABLE IF NOT EXISTS public.loan_applications (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    reference TEXT UNIQUE NOT NULL,
    applicant_id UUID REFERENCES public.profiles(id) ON DELETE SET NULL,
    applicant_name TEXT NOT NULL,
    member_id TEXT NOT NULL,
    phone TEXT,
    email TEXT,
    applicant_type TEXT NOT NULL CHECK (applicant_type IN ('individual', 'cooperative')),
    principal NUMERIC(15, 2) NOT NULL,
    purpose TEXT NOT NULL,
    tenure_months INTEGER NOT NULL DEFAULT 12,
    savings_balance NUMERIC(15, 2) NOT NULL DEFAULT 0,
    monthly_income NUMERIC(15, 2) NOT NULL DEFAULT 0,
    monthly_debt NUMERIC(15, 2) NOT NULL DEFAULT 0,
    multiplier NUMERIC(5, 2) NOT NULL DEFAULT 3.0,
    
    -- Status & Lifecycle
    status TEXT NOT NULL DEFAULT 'draft' CHECK (status IN ('draft', 'submitted', 'in_review', 'approved', 'declined', 'disbursed')),
    stage TEXT NOT NULL DEFAULT 'draft' CHECK (stage IN ('draft', 'verification', 'underwriting', 'committee', 'disbursed')),
    submitted_on TIMESTAMPTZ,
    status_note TEXT,
    
    -- Underwriter & Guardrail Engine Fields
    basic_monthly_pay NUMERIC(15, 2),
    monthly_deductions NUMERIC(15, 2),
    dti_net_ratio NUMERIC(5, 2),
    net_take_home NUMERIC(15, 2),
    guardrail_multiplier_passed BOOLEAN DEFAULT FALSE,
    guardrail_one_third_passed BOOLEAN DEFAULT FALSE,
    guardrail_guarantor_passed BOOLEAN DEFAULT TRUE,
    verdict TEXT DEFAULT 'PENDING' CHECK (verdict IN ('PENDING', 'APPROVED', 'DECLINED')),
    appraisal_officer TEXT,
    security_signature TEXT,
    
    -- Qualitative Audits
    crb_category TEXT DEFAULT 'Category B: Minor Delinquencies (< 30 Days)',
    crb_score INTEGER DEFAULT 685,
    field_audit_character TEXT DEFAULT 'KYC verified, market association references passed.',
    field_audit_capacity TEXT DEFAULT 'OCR reconstructed revenue matches declared flows.',
    field_audit_collateral TEXT DEFAULT 'Business stocks or social assets physically validated.',
    
    -- Disbursement
    disbursed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- 3. APPLICATION DOCUMENTS TABLE
CREATE TABLE IF NOT EXISTS public.application_documents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    application_id UUID REFERENCES public.loan_applications(id) ON DELETE CASCADE,
    slot_id TEXT NOT NULL,
    label TEXT NOT NULL,
    hint TEXT,
    required BOOLEAN DEFAULT TRUE,
    file_name TEXT,
    file_url TEXT,
    status TEXT NOT NULL DEFAULT 'PENDING' CHECK (status IN ('PENDING', 'VERIFIED', 'REJECTED', 'MISSING')),
    verified_at TIMESTAMPTZ,
    verified_by TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 4. GUARANTORS TABLE
CREATE TABLE IF NOT EXISTS public.guarantors (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    application_id UUID REFERENCES public.loan_applications(id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    member_id TEXT NOT NULL,
    pledged_shares NUMERIC(15, 2) NOT NULL,
    available_shares NUMERIC(15, 2) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 5. COMMITTEE VOTES TABLE
CREATE TABLE IF NOT EXISTS public.committee_votes (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    application_id UUID REFERENCES public.loan_applications(id) ON DELETE CASCADE,
    member_name TEXT NOT NULL,
    member_role TEXT NOT NULL,
    vote TEXT CHECK (vote IN ('APPROVE', 'REJECT', 'ABSTAIN')),
    comments TEXT,
    voted_at TIMESTAMPTZ DEFAULT NOW()
);

-- 6. CREDIT PASSPORT REGISTRY
CREATE TABLE IF NOT EXISTS public.credit_passports (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    member_id TEXT UNIQUE NOT NULL,
    classification TEXT NOT NULL,
    tier TEXT NOT NULL CHECK (tier IN ('PLATINUM', 'GOLD', 'SILVER')),
    trust_score INTEGER NOT NULL,
    on_time_rate_pct INTEGER NOT NULL,
    loans_completed INTEGER NOT NULL DEFAULT 0,
    total_repaid NUMERIC(15, 2) NOT NULL DEFAULT 0,
    current_limit NUMERIC(15, 2) NOT NULL,
    last_loan_date TEXT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 7. PORTFOLIO LOANS & DISBURSEMENTS
CREATE TABLE IF NOT EXISTS public.portfolio_loans (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    reference TEXT UNIQUE NOT NULL,
    borrower_name TEXT NOT NULL,
    borrower_meta TEXT,
    type TEXT NOT NULL,
    principal NUMERIC(15, 2) NOT NULL,
    status TEXT NOT NULL DEFAULT 'PENDING' CHECK (status IN ('PENDING', 'APPROVED', 'REPAYING', 'COMPLETED', 'REJECTED')),
    repayment_progress TEXT,
    due_date TEXT,
    arrears NUMERIC(15, 2) DEFAULT 0,
    is_locked BOOLEAN DEFAULT FALSE,
    disbursed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- 8. LEDGER ACCOUNTS FOR LIQUID CASH TRACKING
CREATE TABLE IF NOT EXISTS public.ledger_accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    account_type TEXT UNIQUE NOT NULL CHECK (account_type IN ('Cash_Vault', 'Bank_Current', 'Mobile_Money_Float')),
    current_balance NUMERIC(15, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- ==============================================================================
-- INITIAL SEED DATA
-- ==============================================================================

-- Seed Profiles
INSERT INTO public.profiles (email, full_name, member_id, phone, address, employer_or_business, monthly_income, role)
VALUES 
    ('applicant@talanton.io', 'Amina K. Nakamya', 'M-8842', '+256 701 445 889', 'Plot 14 Jinja Road, Kampala', 'Grace Retail & General Supplies', 2500000, 'applicant'),
    ('underwriter@talanton.io', 'Agaba Collins', 'U-0104', '+256 772 119 440', 'Talanton Risk Headquarters', 'Risk & Appraisal Division', 4500000, 'underwriter'),
    ('committee@talanton.io', 'Dr. Ochieng Chairperson', 'C-0001', '+256 782 550 120', 'SACCO Board Secretariat', 'Executive Committee', 6000000, 'committee')
ON CONFLICT (email) DO NOTHING;

-- Seed Applications
INSERT INTO public.loan_applications (
    reference, applicant_name, member_id, phone, email, applicant_type, principal, purpose, tenure_months,
    savings_balance, monthly_income, monthly_debt, multiplier, status, stage, submitted_on, status_note,
    basic_monthly_pay, monthly_deductions, dti_net_ratio, net_take_home,
    guardrail_multiplier_passed, guardrail_one_third_passed, guardrail_guarantor_passed, verdict,
    appraisal_officer, security_signature, crb_category, crb_score
) VALUES 
(
    'LA-2026-0941A', 'Nakamya Grace', 'M-8842', '+256 701 445 889', 'applicant@talanton.io', 'individual',
    15000000, 'Expand retail inventory', 12, 4000000, 2500000, 500000, 3.0,
    'in_review', 'committee', NOW() - INTERVAL '3 days', 'Underwriting verified. Awaiting Committee Quorum.',
    2500000, 500000, 28.5, 1250000, TRUE, TRUE, TRUE, 'APPROVED',
    'Agaba Collins (Risk Division)', 'OTP Signed (Verified)', 'Category B: Minor Delinquencies (< 30 Days)', 710
),
(
    'LA-2026-0938B', 'Ssemakula Enterprises Ltd', 'SME-0412', '+256 752 900 112', 'ssemakula@agro.co.ug', 'cooperative',
    42000000, 'Agricultural Processing Equipment', 24, 12000000, 7500000, 1800000, 3.5,
    'in_review', 'committee', NOW() - INTERVAL '2 days', 'Underwriting verified. Awaiting Committee Quorum.',
    7500000, 1800000, 34.0, 3950000, TRUE, TRUE, TRUE, 'APPROVED',
    'Agaba Collins (Risk Division)', 'OTP Signed (Verified)', 'Category A: Clean Credit Record', 820
),
(
    'LA-2026-0871E', 'Mukasa Agro Supplies', 'SME-0871', '+256 703 118 992', 'mukasa@agro.ug', 'cooperative',
    28000000, 'Working Capital & Inventory', 18, 9000000, 5200000, 1100000, 3.0,
    'approved', 'disbursed', NOW() - INTERVAL '10 days', 'Quorum passed. Funds released to member.',
    5200000, 1100000, 22.0, 3100000, TRUE, TRUE, TRUE, 'APPROVED',
    'Agaba Collins (Risk Division)', 'OTP Signed (Verified)', 'Category A: Clean Credit Record', 780
)
ON CONFLICT (reference) DO NOTHING;

-- Seed Credit Passport Members
INSERT INTO public.credit_passports (name, member_id, classification, tier, trust_score, on_time_rate_pct, loans_completed, total_repaid, current_limit, last_loan_date)
VALUES
    ('Namatovu Sarah', 'M-2309', 'BOSA', 'PLATINUM', 92, 100, 4, 18500000, 25000000, 'Dec 2025'),
    ('Ssemakula Agro Ltd', 'SME-0412', 'SME', 'PLATINUM', 88, 97, 3, 76000000, 90000000, 'Nov 2025'),
    ('Kato Joseph', 'M-1104', 'BOSA', 'GOLD', 84, 95, 2, 9200000, 15000000, 'Oct 2025'),
    ('Auma Florence', 'M-4511', 'BOSA', 'GOLD', 76, 89, 3, 11400000, 12000000, 'Jan 2026'),
    ('Mukasa Peter', 'M-9022', 'BOSA', 'SILVER', 71, 92, 1, 3500000, 6000000, 'Sep 2025'),
    ('Kiiza Wholesale Co.', 'SME-0755', 'SME', 'GOLD', 83, 94, 2, 44000000, 60000000, 'Dec 2025')
ON CONFLICT (member_id) DO NOTHING;

-- Seed Ledger Accounts
INSERT INTO public.ledger_accounts (name, account_type, current_balance)
VALUES
    ('SACCO Main Cash Vault', 'Cash_Vault', 30000000.00),
    ('Commercial Bank Account', 'Bank_Current', 85000000.00),
    ('Mobile Money Float Account', 'Mobile_Money_Float', 35000000.00)
ON CONFLICT (account_type) DO NOTHING;

-- Enable RLS
ALTER TABLE public.profiles ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.loan_applications ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.application_documents ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.guarantors ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.committee_votes ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.credit_passports ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.portfolio_loans ENABLE ROW LEVEL SECURITY;
ALTER TABLE public.ledger_accounts ENABLE ROW LEVEL SECURITY;

-- Permissive public policies for SACCO demo
CREATE POLICY "Public read profiles" ON public.profiles FOR SELECT USING (true);
CREATE POLICY "Public write profiles" ON public.profiles FOR ALL USING (true);

CREATE POLICY "Public read loan_applications" ON public.loan_applications FOR SELECT USING (true);
CREATE POLICY "Public write loan_applications" ON public.loan_applications FOR ALL USING (true);

CREATE POLICY "Public read application_documents" ON public.application_documents FOR SELECT USING (true);
CREATE POLICY "Public write application_documents" ON public.application_documents FOR ALL USING (true);

CREATE POLICY "Public read guarantors" ON public.guarantors FOR SELECT USING (true);
CREATE POLICY "Public write guarantors" ON public.guarantors FOR ALL USING (true);

CREATE POLICY "Public read committee_votes" ON public.committee_votes FOR SELECT USING (true);
CREATE POLICY "Public write committee_votes" ON public.committee_votes FOR ALL USING (true);

CREATE POLICY "Public read credit_passports" ON public.credit_passports FOR SELECT USING (true);
CREATE POLICY "Public write credit_passports" ON public.credit_passports FOR ALL USING (true);

CREATE POLICY "Public read portfolio_loans" ON public.portfolio_loans FOR SELECT USING (true);
CREATE POLICY "Public write portfolio_loans" ON public.portfolio_loans FOR ALL USING (true);

CREATE POLICY "Public read ledger_accounts" ON public.ledger_accounts FOR SELECT USING (true);
CREATE POLICY "Public write ledger_accounts" ON public.ledger_accounts FOR ALL USING (true);

-- ==============================================================================
-- 8. STORAGE BUCKET FOR COMPLIANCE & KYC DOCUMENTS (S3-Compatible)
-- ==============================================================================
INSERT INTO storage.buckets (id, name, public) 
VALUES ('documents', 'documents', true)
ON CONFLICT (id) DO NOTHING;

CREATE POLICY "Public Document Access" ON storage.objects FOR SELECT USING (bucket_id = 'documents');
CREATE POLICY "Public Document Upload" ON storage.objects FOR INSERT WITH CHECK (bucket_id = 'documents');
CREATE POLICY "Public Document Update" ON storage.objects FOR UPDATE USING (bucket_id = 'documents');

