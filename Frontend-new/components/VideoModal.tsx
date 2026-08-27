'use client'

import { X } from 'lucide-react'
import { ReactNode, useState, useEffect, useCallback } from 'react'
import { createPortal } from 'react-dom'

export function VideoModal({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false)
  const [mounted, setMounted] = useState(false)

  useEffect(() => { setMounted(true) }, [])

  const close = useCallback(() => setOpen(false), [])

  // Close on Escape
  useEffect(() => {
    if (!open) return
    const handler = (e: KeyboardEvent) => { if (e.key === 'Escape') close() }
    window.addEventListener('keydown', handler)
    return () => window.removeEventListener('keydown', handler)
  }, [open, close])

  // Prevent body scroll while open
  useEffect(() => {
    document.body.style.overflow = open ? 'hidden' : ''
    return () => { document.body.style.overflow = '' }
  }, [open])

  const modal = open && mounted ? createPortal(
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
    >
      {/* Backdrop — page stays visible, just dimmed */}
      <div
        className="absolute inset-0 bg-black/60 backdrop-blur-sm"
        onClick={close}
        aria-hidden="true"
      />

      {/* Video card */}
      <div className="relative z-10 w-full max-w-3xl rounded-2xl overflow-hidden shadow-2xl ring-1 ring-white/10">
        {/* Close button */}
        <button
          onClick={close}
          className="absolute top-3 right-3 z-20 rounded-full bg-black/50 p-2 text-white hover:bg-black/70 transition-colors"
          aria-label="Close video"
        >
          <X className="size-5" strokeWidth={2.5} />
        </button>

        {/* 16:9 iframe */}
        <div className="relative w-full bg-black" style={{ paddingTop: '56.25%' }}>
          <iframe
            src="https://drive.google.com/file/d/1VyQRmi8N6Xr5MjWok3icoeRO7d2jbR1m/preview"
            className="absolute inset-0 w-full h-full"
            allow="autoplay"
            allowFullScreen
          />
        </div>
      </div>
    </div>,
    document.body
  ) : null

  return (
    <>
      {/* Trigger — clone child and attach onClick */}
      <span onClick={() => setOpen(true)} style={{ display: 'contents' }}>
        {children}
      </span>
      {modal}
    </>
  )
}

