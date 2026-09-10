'use client'

import { useCallback, useEffect, useRef, useState } from 'react'
import { Bell, CheckCheck } from 'lucide-react'
import {
  fetchNotifications,
  markNotificationsRead,
  type AppNotification,
} from '@/lib/api-service'
import type { RoleType } from '@/lib/talenton-data'

/**
 * The alerts for whoever is signed in.
 *
 * QA found no notification feature of any kind, so a revised offer waited until somebody thought
 * to reopen the file. The server raises an alert at each point a file changes hands; this is
 * where they are read.
 *
 * Email and SMS are not wired: the deployment has no provider or credentials for either. Every
 * alert is stored with the audience it is addressed to, so adding a channel later means draining
 * that table rather than re-instrumenting the workflow.
 */
const POLL_INTERVAL_MS = 30_000

function relativeTime(iso: string): string {
  const then = new Date(iso).getTime()
  if (Number.isNaN(then)) return ''
  const seconds = Math.max(0, Math.round((Date.now() - then) / 1000))
  if (seconds < 60) return 'just now'
  const minutes = Math.round(seconds / 60)
  if (minutes < 60) return `${minutes}m ago`
  const hours = Math.round(minutes / 60)
  if (hours < 24) return `${hours}h ago`
  return `${Math.round(hours / 24)}d ago`
}

export function NotificationBell({
  role,
  audienceKey,
  onOpenReference,
}: {
  role: RoleType
  /** A membership number or seat, when the alerts should be narrowed to one person. */
  audienceKey?: string
  onOpenReference?: (reference: string) => void
}) {
  const [items, setItems] = useState<AppNotification[]>([])
  const [unread, setUnread] = useState(0)
  const [open, setOpen] = useState(false)
  const [state, setState] = useState<'loading' | 'ready' | 'unavailable'>('loading')
  const panelRef = useRef<HTMLDivElement | null>(null)

  const load = useCallback(async () => {
    const feed = await fetchNotifications(role, audienceKey)
    if (!feed) {
      setState('unavailable')
      return
    }
    setItems(feed.notifications)
    setUnread(feed.unreadCount)
    setState('ready')
  }, [role, audienceKey])

  useEffect(() => {
    load()
    const timer = setInterval(load, POLL_INTERVAL_MS)
    return () => clearInterval(timer)
  }, [load])

  // Clicking anywhere else closes the panel, so it does not sit over the page a user has
  // moved on from.
  useEffect(() => {
    if (!open) return
    function onDocumentClick(event: MouseEvent) {
      if (panelRef.current && !panelRef.current.contains(event.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', onDocumentClick)
    return () => document.removeEventListener('mousedown', onDocumentClick)
  }, [open])

  async function markAllRead() {
    const unreadIds = items.filter((n) => !n.isRead).map((n) => n.id)
    if (unreadIds.length === 0) return
    setItems((prev) => prev.map((n) => ({ ...n, isRead: true })))
    setUnread(0)
    const ok = await markNotificationsRead(role, unreadIds)
    if (!ok) load()
  }

  return (
    <div ref={panelRef} className="relative">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        aria-label={unread > 0 ? `Notifications, ${unread} unread` : 'Notifications'}
        aria-expanded={open}
        className="relative flex size-9 items-center justify-center rounded-full border border-white/15 bg-white/5 text-white transition-colors hover:bg-white/15"
      >
        <Bell className="size-4" />
        {unread > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex min-w-[1.1rem] items-center justify-center rounded-full bg-[#a4cc44] px-1 text-[0.6rem] font-bold text-[#0d2a1c]">
            {unread > 9 ? '9+' : unread}
          </span>
        )}
      </button>

      {open && (
        <div className="absolute right-0 z-50 mt-2 w-[22rem] max-w-[calc(100vw-2rem)] overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-2xl">
          <div className="flex items-center justify-between border-b border-gray-100 px-4 py-3">
            <p className="text-xs font-bold uppercase tracking-widest text-[#103a27]">Notifications</p>
            {unread > 0 && (
              <button
                type="button"
                onClick={markAllRead}
                className="flex items-center gap-1 text-[0.65rem] font-semibold text-gray-500 hover:text-[#103a27]"
              >
                <CheckCheck className="size-3" />
                Mark all read
              </button>
            )}
          </div>

          <div className="max-h-[24rem] overflow-y-auto">
            {state === 'loading' && (
              <p className="px-4 py-6 text-center text-xs text-gray-500">Checking for updates…</p>
            )}

            {state === 'unavailable' && (
              <p className="px-4 py-6 text-center text-xs text-amber-700">
                Alerts could not be loaded &mdash; the server did not respond.
              </p>
            )}

            {state === 'ready' && items.length === 0 && (
              <p className="px-4 py-6 text-center text-xs text-gray-500">
                Nothing yet. You will be told here when a file moves.
              </p>
            )}

            {items.map((n) => (
              <button
                key={n.id}
                type="button"
                onClick={() => {
                  if (n.reference && onOpenReference) {
                    onOpenReference(n.reference)
                    setOpen(false)
                  }
                }}
                className={`block w-full border-b border-gray-50 px-4 py-3 text-left last:border-none ${
                  n.isRead ? 'bg-white' : 'bg-[#f6faef]'
                } ${n.reference && onOpenReference ? 'cursor-pointer hover:bg-gray-50' : 'cursor-default'}`}
              >
                <div className="flex items-start justify-between gap-3">
                  <p className="text-xs font-bold text-[#103a27]">{n.title}</p>
                  <span className="shrink-0 text-[0.6rem] text-gray-400">{relativeTime(n.createdAt)}</span>
                </div>
                <p className="mt-1 text-[0.7rem] leading-relaxed text-gray-600">{n.body}</p>
                {n.reference && (
                  <span className="mt-1.5 inline-block rounded bg-gray-100 px-1.5 py-0.5 font-mono text-[0.6rem] font-bold text-gray-600">
                    {n.reference}
                  </span>
                )}
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  )
}
