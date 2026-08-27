'use client'

import Link from 'next/link'
import { LoginModal } from '@/components/LoginModal'
import { VideoModal } from '@/components/VideoModal'
import { ArrowRight, Play } from 'lucide-react'

export default function RootPage() {
  return (
    <div className="min-h-screen bg-[#eaf4e5] text-[#103a27] font-sans flex flex-col">

      {/* Navbar — sits on the fallback color, above the hero bg */}
      <nav className="anim-nav flex items-center justify-between px-8 py-5 w-full z-10 bg-[#eaf4e5]">
        <div className="text-3xl font-serif font-bold text-[#103a27]">Talanton.</div>
        <div className="hidden md:flex items-center gap-7 text-sm font-medium rounded-full bg-[#103a27]/6 px-7 py-2.5">
          <Link href="#" className="hover:text-[#103a27]/70">Solution</Link>
          <span className="text-[#103a27]/25">•</span>
          <Link href="#" className="hover:text-[#103a27]/70">For SACCOs</Link>
          <span className="text-[#103a27]/25">•</span>
          <Link href="#" className="hover:text-[#103a27]/70">Partnerships</Link>
          <span className="text-[#103a27]/25">•</span>
          <Link href="#" className="hover:text-[#103a27]/70">Resources</Link>
          <span className="text-[#103a27]/25">•</span>
          <Link href="#" className="hover:text-[#103a27]/70">Contact Us</Link>
        </div>
        <div className="flex items-center gap-5 text-base font-medium">
          <LoginModal>
            Log in
          </LoginModal>
          <button className="rounded-full bg-[#103a27] text-white px-5 py-2.5 text-sm font-semibold hover:bg-[#124a31] transition-colors flex items-center gap-2 cursor-pointer">
            Sign up
            <span className="bg-white text-[#103a27] rounded-full p-1"><ArrowRight className="size-3" strokeWidth={2.5} /></span>
          </button>
        </div>
      </nav>

      {/* Hero — background image starts HERE, below the navbar */}
      <main className="landing-bg anim-hero-bg flex-1 flex items-center w-full overflow-hidden">

        {/* Left spacer — lets the bg image person show through (~45% of width) */}
        <div className="hidden md:block w-[45%] flex-shrink-0 self-stretch" />

        {/* Text — right side, full readable width, vertically centered */}
        <div className="flex-1 flex items-center px-10 lg:px-16 xl:px-20 py-16">
          <div className="max-w-2xl">
            <h1 className="anim-heading font-serif text-5xl md:text-6xl xl:text-7xl font-bold leading-[1.08] tracking-tight text-[#103a27]">
              A better way to offer credit access to your members
            </h1>
            <p className="anim-sub mt-6 text-[#2a5040]/80 text-base md:text-lg max-w-lg">
              Talanton offers secure credit access to SACCO members with the lowest-risk and lowest cost to cooperatives in the market.
            </p>
            <div className="anim-buttons mt-10 flex flex-wrap items-center gap-4">
              <LoginModal className="rounded-full bg-[#103a27] text-white px-7 py-3.5 text-base font-semibold hover:bg-[#124a31] transition-colors flex items-center gap-3 cursor-pointer">
                Get started
                <span className="bg-white text-[#103a27] rounded-full p-1.5"><ArrowRight className="size-4" strokeWidth={2.5} /></span>
              </LoginModal>
              <VideoModal>
                <button className="rounded-full bg-white/60 border border-[#103a27]/15 text-[#103a27] px-7 py-3.5 text-base font-semibold hover:bg-white/90 transition-colors flex items-center gap-3 cursor-pointer">
                  See how it works
                  <span className="bg-[#103a27] text-white rounded-full p-1.5"><Play className="size-4 fill-white" /></span>
                </button>
              </VideoModal>
            </div>
          </div>
        </div>
      </main>

    </div>
  )
}

