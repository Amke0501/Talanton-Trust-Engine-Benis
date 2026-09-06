'use client'

import Image from 'next/image'

/**
 * The Talanton brand lockup, in one place.
 *
 * The wordmark was previously typed out in six components and paired with a generic scales icon
 * in the header, so there was no single thing to change when the real logo arrives. Drop the
 * artwork at `public/talanton-logo.svg` and set LOGO_SRC below; every surface picks it up.
 *
 * Until then this renders the wordmark treatment the app already used, so nothing regresses and
 * no placeholder mark is passed off as the brand.
 */
const LOGO_SRC: string | null = null

export function BrandMark({
  tone = 'dark',
  size = 'md',
  caption,
}: {
  /** 'dark' for light backgrounds, 'light' for the deep-green sidebars. */
  tone?: 'dark' | 'light'
  size?: 'sm' | 'md' | 'lg'
  /** Optional line beneath the wordmark, e.g. "SACCO Credit Pipeline". */
  caption?: string
}) {
  const wordSize = size === 'lg' ? 'text-3xl' : size === 'sm' ? 'text-lg' : 'text-2xl'
  const logoPx = size === 'lg' ? 40 : size === 'sm' ? 24 : 32
  const wordColor = tone === 'light' ? 'text-white' : 'text-[#103a27]'
  const captionColor = tone === 'light' ? 'text-white/60' : 'text-gray-500'

  return (
    <div className="flex items-center gap-2.5">
      {LOGO_SRC && (
        <Image
          src={LOGO_SRC}
          alt=""
          width={logoPx}
          height={logoPx}
          className="shrink-0"
          priority
        />
      )}
      <div className="leading-none">
        <span className={`block font-serif font-bold tracking-tight ${wordSize} ${wordColor}`}>
          Talanton<span className="text-[#a4cc44]">.</span>
        </span>
        {caption && (
          <span className={`mt-1 block text-[0.65rem] font-medium uppercase tracking-widest ${captionColor}`}>
            {caption}
          </span>
        )}
      </div>
    </div>
  )
}
