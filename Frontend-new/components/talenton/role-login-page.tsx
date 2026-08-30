'use client'

import Link from 'next/link'
import Image from 'next/image'
import { useRouter } from 'next/navigation'
import { FormEvent, useMemo, useState } from 'react'
import { Eye, EyeOff, ArrowLeft } from 'lucide-react'
import {
  AUTH_COOKIE_NAME,
  ROLE_COOKIE_NAME,
  USER_EMAIL_COOKIE_NAME,
} from '@/lib/role-access'
import type { RoleType } from '@/lib/talenton-data'

/* ── per-role image config ───────────────────────────────────────── */
const ROLE_META: Record<RoleType, { title: string; image: string | null; imageAlt: string }> = {
  applicant: {
    title: 'Applicant Portal',
    image: '/applicant_login.jpg',
    imageAlt: 'Applicant at a business counter',
  },
  underwriter: {
    title: 'Underwriter Portal',
    image: null,
    imageAlt: 'Underwriter clipboard and documents',
  },
  committee: {
    title: 'Committee Portal',
    image: null,
    imageAlt: '',
  },
}

function AnimatedAbstractGraphic({ role }: { role: RoleType }) {
  if (role === 'applicant') return null;

  return (
    <div className="absolute inset-0 bg-[#0d2a1c] overflow-hidden flex items-center justify-center">
      {/* Glow effects */}
      <div className="absolute top-1/4 -left-1/4 w-96 h-96 bg-[#a4cc44]/20 rounded-full blur-[100px] animate-pulse" />
      <div className="absolute bottom-1/4 -right-1/4 w-[30rem] h-[30rem] bg-[#124a31]/80 rounded-full blur-[120px] animate-pulse" style={{ animationDelay: '1s' }} />

      <svg width="100%" height="100%" xmlns="http://www.w3.org/2000/svg" className="absolute inset-0 z-0 opacity-40">
        <defs>
          <pattern id="grid" width="40" height="40" patternUnits="userSpaceOnUse">
            <path d="M 40 0 L 0 0 0 40" fill="none" stroke="#a4cc44" strokeWidth="0.5" strokeOpacity="0.3" />
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#grid)" />
      </svg>

      {role === 'underwriter' ? (
        <div className="relative z-10 flex flex-col items-center justify-center w-full h-full">
          
          {/* Lines + Icon nodes — all in one SVG so lines connect exactly to icon centers */}
          <svg className="absolute inset-0 w-full h-full z-30 pointer-events-none" viewBox="0 0 100 100" preserveAspectRatio="xMidYMid slice">
            {/* Lines from center outward to each icon */}
            <g stroke="#a4cc44" strokeWidth="0.3" opacity="0.6">
              <line x1="50" y1="45" x2="22" y2="15" />
              <line x1="50" y1="45" x2="50" y2="12" />
              <line x1="50" y1="45" x2="78" y2="15" />
              <line x1="50" y1="50" x2="12" y2="45" />
              <line x1="50" y1="50" x2="88" y2="45" />
              <line x1="50" y1="55" x2="18" y2="78" />
              <line x1="50" y1="55" x2="82" y2="78" />
              {/* Cross-links between adjacent icons */}
              <line x1="22" y1="15" x2="50" y2="12" opacity="0.3" />
              <line x1="50" y1="12" x2="78" y2="15" opacity="0.3" />
              <line x1="12" y1="45" x2="18" y2="78" opacity="0.3" />
              <line x1="88" y1="45" x2="82" y2="78" opacity="0.3" />
            </g>

            {/* Icon node circles at line endpoints */}
            {/* Top-left: Shield */}
            <circle cx="22" cy="15" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <path d="M21 15.5l0.8 0.8 1.6-1.6" fill="none" stroke="#a4cc44" strokeWidth="0.4" />
            <path d="M22 12.5a3.5 3.5 0 01-2.5 1 3.5 3.5 0 01.1 1.5c0 1.6 1.1 3 2.4 3.4 1.3-.4 2.4-1.7 2.4-3.4a3.5 3.5 0 01.1-1.5 3.5 3.5 0 01-2.5-1z" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            
            {/* Top-center: Clipboard check */}
            <circle cx="50" cy="12" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <rect x="48.5" y="10.5" width="3" height="3.5" rx="0.3" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            <line x1="49.3" y1="12.5" x2="50.7" y2="12.5" stroke="#a4cc44" strokeWidth="0.2" />
            <line x1="49.3" y1="13.2" x2="50.7" y2="13.2" stroke="#a4cc44" strokeWidth="0.2" />

            {/* Top-right: Search */}
            <circle cx="78" cy="15" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <circle cx="77.5" cy="14.5" r="1.2" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            <line x1="78.4" y1="15.4" x2="79.3" y2="16.3" stroke="#a4cc44" strokeWidth="0.3" />

            {/* Mid-left: Person */}
            <circle cx="12" cy="45" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <circle cx="12" cy="43.8" r="0.8" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            <path d="M10.2 46.5a1.8 1.8 0 013.6 0" fill="none" stroke="#a4cc44" strokeWidth="0.3" />

            {/* Mid-right: Chart */}
            <circle cx="88" cy="45" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <line x1="86.8" y1="46.5" x2="86.8" y2="44.5" stroke="#a4cc44" strokeWidth="0.4" />
            <line x1="88" y1="46.5" x2="88" y2="43.5" stroke="#a4cc44" strokeWidth="0.4" />
            <line x1="89.2" y1="46.5" x2="89.2" y2="44" stroke="#a4cc44" strokeWidth="0.4" />

            {/* Bottom-left: Document */}
            <circle cx="18" cy="78" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <rect x="16.8" y="76.2" width="2.4" height="3.2" rx="0.3" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            <line x1="17.3" y1="77.5" x2="18.7" y2="77.5" stroke="#a4cc44" strokeWidth="0.2" />
            <line x1="17.3" y1="78.2" x2="18.7" y2="78.2" stroke="#a4cc44" strokeWidth="0.2" />

            {/* Bottom-right: Edit */}
            <circle cx="82" cy="78" r="3.2" fill="#0d2a1c" stroke="#a4cc44" strokeWidth="0.4" opacity="0.9" />
            <path d="M81 79.5l1.5-1.5 0.8 0.8-1.5 1.5z" fill="none" stroke="#a4cc44" strokeWidth="0.3" />
            <line x1="82.5" y1="78" x2="83.3" y2="77.2" stroke="#a4cc44" strokeWidth="0.3" />
          </svg>

          {/* Clipboard Image — fills most of the panel, in front of lines */}
          <div className="relative z-40">
            <div className="relative">
              <div className="absolute inset-0 bg-[#a4cc44]/8 rounded-full blur-[100px] scale-150" />
              <img
                src="/underwriter.png"
                alt="Underwriter clipboard and documents"
                className="relative w-[42rem] xl:w-[48rem] 2xl:w-[54rem] max-w-[100%] mx-auto"
                style={{ filter: 'drop-shadow(0 30px 60px rgba(0,0,0,0.5))' }}
              />
            </div>
          </div>

          {/* Badge */}
          <div className="relative z-30 mt-4 border border-[#a4cc44]/30 bg-[#0d2a1c]/70 backdrop-blur-md rounded-full px-6 py-2.5 flex items-center gap-2" style={{ animation: 'rl-fadeUp 0.8s cubic-bezier(0.22,1,0.36,1) 0.5s both' }}>
            <svg className="w-4 h-4 text-[#a4cc44]" fill="none" stroke="currentColor" viewBox="0 0 24 24" strokeWidth="2"><path d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z" /></svg>
            <p className="text-[0.7rem] font-bold uppercase tracking-widest text-[#a4cc44]">Trust Audit Engine</p>
          </div>
        </div>
      ) : (
        /* Committee graphic: City Skyline Shadow */
        <div className="relative z-10 w-full h-full flex items-end justify-center overflow-hidden">
          
          {/* Subtle glowing moon/sun behind city */}
          <div className="absolute top-[20%] right-[30%] w-40 h-40 bg-[#a4cc44] rounded-full blur-[60px] opacity-20 animate-pulse" style={{ animationDuration: '6s' }} />

          {/* Network Lines */}
          <svg className="absolute inset-0 w-full h-full z-10 pointer-events-none" viewBox="0 0 1000 800" preserveAspectRatio="xMidYMid slice">
            <g stroke="#a4cc44" strokeWidth="1.5" opacity="0.3" className="animate-[pulse_5s_ease-in-out_infinite]">
              <line x1="200" y1="200" x2="450" y2="350" />
              <line x1="450" y1="350" x2="700" y2="150" />
              <line x1="700" y1="150" x2="850" y2="400" />
              <line x1="150" y1="500" x2="450" y2="350" />
              <line x1="450" y1="350" x2="600" y2="600" />
              <line x1="600" y1="600" x2="850" y2="400" />
              <line x1="100" y1="300" x2="200" y2="200" />
              <line x1="850" y1="400" x2="950" y2="250" />
              <circle cx="200" cy="200" r="4" fill="#a4cc44" />
              <circle cx="450" cy="350" r="6" fill="#a4cc44" />
              <circle cx="700" cy="150" r="4" fill="#a4cc44" />
              <circle cx="850" cy="400" r="5" fill="#a4cc44" />
              <circle cx="150" cy="500" r="3" fill="#a4cc44" />
              <circle cx="600" cy="600" r="4" fill="#a4cc44" />
              <circle cx="100" cy="300" r="2" fill="#a4cc44" />
              <circle cx="950" cy="250" r="3" fill="#a4cc44" />
            </g>
          </svg>

          {/* City skyline SVG - layered shadows */}
          <svg viewBox="0 0 1000 400" className="absolute bottom-0 w-full min-w-[150%] h-[60%] sm:h-[70%] text-[#103a27] z-20 preserveAspectRatio='none'">
            {/* Background layer - lighter shadow */}
            <path d="M0 400 V 280 H 60 V 220 H 120 V 160 H 180 V 250 H 240 V 140 H 300 V 200 H 380 V 100 H 460 V 220 H 540 V 160 H 620 V 260 H 700 V 130 H 780 V 240 H 860 V 170 H 940 V 290 H 1000 V 400 Z" fill="currentColor" opacity="0.6" />
            
            {/* Midground layer */}
            <path d="M0 400 V 320 H 40 V 260 H 90 V 200 H 140 V 290 H 200 V 180 H 260 V 250 H 340 V 130 H 420 V 280 H 500 V 200 H 580 V 300 H 660 V 170 H 740 V 280 H 820 V 210 H 900 V 330 H 1000 V 400 Z" fill="#0c2518" opacity="0.8" />

            {/* Foreground layer - darkest shadow */}
            <path d="M0 400 V 350 H 80 V 300 H 150 V 240 H 220 V 320 H 300 V 210 H 380 V 290 H 480 V 180 H 560 V 330 H 640 V 250 H 720 V 340 H 800 V 220 H 880 V 310 H 960 V 260 H 1000 V 400 Z" fill="#081c12" />
            
            {/* Little glowing windows on some buildings */}
            <g fill="#a4cc44" opacity="0.4" className="animate-pulse">
              <rect x="320" y="240" width="4" height="6" />
              <rect x="330" y="240" width="4" height="6" />
              <rect x="320" y="255" width="4" height="6" />
              <rect x="330" y="255" width="4" height="6" />
              
              <rect x="500" y="220" width="6" height="4" />
              <rect x="500" y="230" width="6" height="4" />
              
              <rect x="760" y="250" width="4" height="8" />
              <rect x="770" y="250" width="4" height="8" />
            </g>
          </svg>

          <div className="absolute inset-0 flex flex-col items-center justify-center text-center z-30 pb-20">
             <div className="border border-[#a4cc44]/30 bg-[#0d2a1c]/80 backdrop-blur-md rounded-xl px-5 py-2.5" style={{ animation: 'rl-fadeUp 0.8s cubic-bezier(0.22,1,0.36,1) 0.3s both' }}>
               <p className="text-[0.65rem] font-bold uppercase tracking-widest text-[#a4cc44]">Urban Credit Network</p>
             </div>
          </div>
        </div>
      )}
    </div>
  )
}

export function RoleLoginPage({ role }: { role: RoleType }) {
  const router = useRouter()
  const [email, setEmail]       = useState('')
  const [password, setPassword] = useState('')
  const [showPw, setShowPw]     = useState(false)
  const [error, setError]       = useState('')
  const [loading, setLoading]   = useState(false)

  const meta = useMemo(() => ROLE_META[role], [role])

  async function handleSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault()
    setError('')
    setLoading(true)
    const resolvedEmail = email || `${role}@talenton.com`
    document.cookie = `${AUTH_COOKIE_NAME}=1; path=/; samesite=lax`
    document.cookie = `${ROLE_COOKIE_NAME}=${role}; path=/; samesite=lax`
    document.cookie = `${USER_EMAIL_COOKIE_NAME}=${encodeURIComponent(resolvedEmail)}; path=/; samesite=lax`
    router.replace(`/dashboard/${role}`)
  }

  return (
    <>
      {/* ── Keyframe animations ─────────────────────────────────── */}
      <style>{`
        @keyframes rl-imgSlide {
          from { opacity: 0; transform: translateX(30px) scale(1.04); }
          to   { opacity: 1; transform: translateX(0)    scale(1); }
        }
        @keyframes rl-fadeUp {
          from { opacity: 0; transform: translateY(24px); }
          to   { opacity: 1; transform: translateY(0); }
        }
        @keyframes rl-fadeIn {
          from { opacity: 0; }
          to   { opacity: 1; }
        }
        @keyframes rl-clipFloat {
          0%, 100% { transform: translateY(0) rotate(0deg); }
          50%      { transform: translateY(-12px) rotate(1deg); }
        }
        .rl-img-panel  { animation: rl-imgSlide 0.9s cubic-bezier(0.22,1,0.36,1) both; }
        .rl-badge      { animation: rl-fadeUp   0.55s cubic-bezier(0.22,1,0.36,1) 0.15s both; }
        .rl-title      { animation: rl-fadeUp   0.6s  cubic-bezier(0.22,1,0.36,1) 0.25s both; }
        .rl-sub        { animation: rl-fadeUp   0.6s  cubic-bezier(0.22,1,0.36,1) 0.35s both; }
        .rl-field1     { animation: rl-fadeUp   0.6s  cubic-bezier(0.22,1,0.36,1) 0.42s both; }
        .rl-field2     { animation: rl-fadeUp   0.6s  cubic-bezier(0.22,1,0.36,1) 0.52s both; }
        .rl-btn        { animation: rl-fadeUp   0.6s  cubic-bezier(0.22,1,0.36,1) 0.62s both; }
        /* Mobile irregular shape */
        .rl-mobile-img {
          clip-path: ellipse(90% 72% at 50% 38%);
        }
      `}</style>

      <main className="min-h-screen bg-[#eaf4e5] text-[#103a27] flex flex-col lg:flex-row-reverse">

        {/* ══ LEFT — image/graphic panel (desktop only) ════════════════════ */}
        <div className="rl-img-panel hidden lg:block lg:w-[48%] xl:w-[52%] relative overflow-hidden">
          {meta.image ? (
            <Image
              src={meta.image}
              alt={meta.imageAlt}
              fill
              priority
              className="object-cover object-center"
            />
          ) : (
            <AnimatedAbstractGraphic role={role} />
          )}
        </div>

        {/* ══ RIGHT — form panel ═══════════════════════════════════ */}
        <div className="flex-1 flex flex-col min-h-screen lg:min-h-0">

          {/* Header: back button + logo */}
          <header className="flex items-center justify-between px-6 pt-8 pb-4 lg:px-12 lg:pt-10">
            <Link
              href="/"
              className="flex items-center justify-center rounded-full bg-[#103a27]/8 hover:bg-[#103a27]/15 p-3 transition-colors"
              aria-label="Back"
            >
              <ArrowLeft className="size-5" strokeWidth={2.5} />
            </Link>
            <div className="text-2xl font-serif font-bold text-[#103a27]">Talanton.</div>
          </header>

          {/* Mobile decorative image/graphic */}
          <div className="lg:hidden mx-6 mt-2 mb-6 h-52 relative overflow-hidden rounded-3xl">
            {meta.image ? (
              <Image
                src={meta.image}
                alt={meta.imageAlt}
                fill
                priority
                className="rl-mobile-img object-cover object-center"
              />
            ) : (
              <AnimatedAbstractGraphic role={role} />
            )}
            {/* soft overlay so form is still the focus */}
            <div className="absolute inset-0 bg-[#eaf4e5]/30" />
          </div>

          {/* Form container */}
          <div className="flex-1 flex items-center justify-center px-6 py-8 lg:px-16 xl:px-24">
            <div className="w-full max-w-md">

              {/* Portal badge */}
              <div className="rl-badge mb-3">
                <span className="inline-block text-xs font-semibold uppercase tracking-widest text-[#103a27]/60 bg-[#103a27]/8 px-3 py-1 rounded-full">
                  {meta.title}
                </span>
              </div>

              {/* Heading */}
              <h1 className="rl-title font-serif text-5xl md:text-6xl font-bold tracking-tight text-[#103a27] leading-tight">
                Welcome back
              </h1>
              <p className="rl-sub mt-3 text-base text-[#2a5040]/70">
                Sign in securely to access your workspace.
              </p>

              <form className="mt-10 space-y-6" onSubmit={handleSubmit}>

                {/* Email */}
                <label className="rl-field1 block space-y-2">
                  <span className="text-xs font-semibold uppercase tracking-wider text-[#2a5040]/80">Email</span>
                  <input
                    value={email}
                    onChange={e => setEmail(e.target.value)}
                    type="email"
                    required
                    autoComplete="email"
                    placeholder="Enter your email"
                    className="w-full rounded-2xl border border-[#103a27]/12 bg-white/80 px-5 py-4 text-base outline-none transition focus:border-[#103a27] focus:bg-white focus:ring-1 focus:ring-[#103a27] placeholder:text-[#103a27]/35"
                  />
                </label>

                {/* Password */}
                <label className="rl-field2 block space-y-2">
                  <div className="flex items-center justify-between">
                    <span className="text-xs font-semibold uppercase tracking-wider text-[#2a5040]/80">Password</span>
                    <Link href="#" className="text-xs font-semibold text-[#103a27]/70 hover:text-[#103a27] hover:underline transition-colors">
                      Forgot password?
                    </Link>
                  </div>
                  <div className="relative">
                    <input
                      value={password}
                      onChange={e => setPassword(e.target.value)}
                      type={showPw ? 'text' : 'password'}
                      required
                      autoComplete="current-password"
                      placeholder="Enter your password"
                      className="w-full rounded-2xl border border-[#103a27]/12 bg-white/80 px-5 py-4 pr-12 text-base outline-none transition focus:border-[#103a27] focus:bg-white focus:ring-1 focus:ring-[#103a27] placeholder:text-[#103a27]/35"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPw(v => !v)}
                      className="absolute right-4 top-1/2 -translate-y-1/2 text-[#103a27]/40 hover:text-[#103a27]/70 transition-colors"
                      aria-label={showPw ? 'Hide password' : 'Show password'}
                    >
                      {showPw ? <EyeOff className="size-5" /> : <Eye className="size-5" />}
                    </button>
                  </div>
                </label>

                {/* Error */}
                {error && (
                  <p className="rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700">
                    {error}
                  </p>
                )}

                {/* Submit */}
                <button
                  type="submit"
                  disabled={loading}
                  className="rl-btn w-full rounded-[2.5rem] bg-[#103a27] px-8 py-5 text-lg font-semibold text-white transition hover:bg-[#124a31] active:scale-[0.98] disabled:opacity-60 cursor-pointer shadow-md mt-2"
                >
                  {loading ? 'Signing in…' : 'Sign in'}
                </button>
              </form>
            </div>
          </div>
        </div>
      </main>
    </>
  )
}

