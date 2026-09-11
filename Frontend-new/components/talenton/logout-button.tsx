'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { LogOut } from 'lucide-react'
import { signOut } from '@/lib/auth'

/**
 * Ends the session properly.
 *
 * Clearing the cookies is not enough on its own: the access token lives in the Supabase client's
 * own storage, so a "logout" that only dropped cookies left a working token behind — the screens
 * looked signed out while the API would still have honoured the next request.
 *
 * It is also a button rather than a link. Next pre-fetches links in the background, and the
 * logout address used to end the session merely by being requested, which signed people out just
 * for having a dashboard on screen.
 */
export function LogoutButton({ className }: { className?: string }) {
  const router = useRouter()
  const [busy, setBusy] = useState(false)

  async function handleLogout() {
    setBusy(true)
    await signOut()
    router.replace('/')
    router.refresh()
  }

  return (
    <button
      type="button"
      onClick={handleLogout}
      disabled={busy}
      className={
        className ??
        'flex w-full items-center gap-2 rounded-lg px-3 py-2 text-xs font-medium text-white/50 transition-colors hover:bg-white/8 hover:text-white/80 disabled:opacity-50'
      }
    >
      <LogOut className="size-3.5" />
      {busy ? 'Signing out…' : 'Logout'}
    </button>
  )
}
