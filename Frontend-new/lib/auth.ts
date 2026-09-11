'use client'

import { getSupabase, isSupabaseConfigured } from '@/lib/supabase'
import {
  AUTH_COOKIE_NAME,
  ROLE_COOKIE_NAME,
  SEAT_COOKIE_NAME,
  USER_EMAIL_COOKIE_NAME,
  writeSessionCookie,
  type CommitteeSeat,
} from '@/lib/role-access'
import type { RoleType } from '@/lib/talenton-data'

const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5195'

/** Where the local-development token lives when there is no Supabase project to sign in to. */
const DEV_TOKEN_KEY = 'talanton_dev_token'

/**
 * Who the signed-in person is, according to the server.
 *
 * Every field here is an answer, not a request. The portal and the committee seat used to be
 * chosen on the login screen — the seat being the thing that decides whose approval counts toward
 * quorum and who may release money.
 */
export interface Identity {
  email: string
  fullName: string
  portalRole: RoleType
  committeeSeat: CommitteeSeat | null
  memberId: string | null
}

export type SignInResult =
  | { ok: true; identity: Identity }
  | { ok: false; reason: string }

/**
 * The access token for the current session, or null.
 *
 * Read fresh on every call rather than cached: Supabase rotates these roughly hourly and refreshes
 * them in the background, so a token held in a variable goes stale and every request starts
 * failing with no obvious cause.
 */
export async function getAccessToken(): Promise<string | null> {
  const supabase = getSupabase()
  if (supabase) {
    const { data } = await supabase.auth.getSession()
    return data.session?.access_token ?? null
  }

  if (typeof window === 'undefined') return null
  return window.localStorage.getItem(DEV_TOKEN_KEY)
}

/**
 * Signs in and asks the server who this is.
 *
 * Two steps, and the order matters. Supabase answers "is this really the holder of this address?"
 * The SACCO's own records answer "and what may they do?" — a valid login belonging to nobody on
 * the membership roll is refused, because authenticating is not the same as being a member.
 */
export async function signIn(email: string, password: string): Promise<SignInResult> {
  const trimmed = email.trim()

  if (!trimmed || !password) {
    return { ok: false, reason: 'Enter your email address and password.' }
  }

  let token: string | null = null

  if (isSupabaseConfigured()) {
    const supabase = getSupabase()!
    const { data, error } = await supabase.auth.signInWithPassword({
      email: trimmed,
      password,
    })

    if (error) {
      // Never distinguish "no such account" from "wrong password": that difference tells an
      // attacker which addresses are worth attacking.
      return {
        ok: false,
        reason:
          error.message.toLowerCase().includes('confirm')
            ? 'This account has not been confirmed yet. Ask an administrator to activate it.'
            : 'That email address and password do not match.',
      }
    }

    token = data.session?.access_token ?? null
  } else {
    // No Supabase project configured. Only the local development API will issue a token, and it
    // only does so when it is running on a throwaway database.
    token = await requestDevToken(trimmed)
    if (!token) {
      return {
        ok: false,
        reason:
          'Sign-in is not configured. Set NEXT_PUBLIC_SUPABASE_URL and ' +
          'NEXT_PUBLIC_SUPABASE_ANON_KEY, or run the API with USE_INMEMORY_DB=true for local work.',
      }
    }
    if (typeof window !== 'undefined') {
      window.localStorage.setItem(DEV_TOKEN_KEY, token)
    }
  }

  if (!token) {
    return { ok: false, reason: 'Signed in, but no session was returned. Please try again.' }
  }

  const identity = await fetchIdentity(token)
  if (!identity.ok) {
    await signOut()
    return identity
  }

  rememberSession(identity.identity)
  return identity
}

/** Asks the API who the bearer of this token is. */
export async function fetchIdentity(token: string): Promise<SignInResult> {
  try {
    const response = await fetch(`${API_BASE}/api/auth/me`, {
      headers: { Authorization: `Bearer ${token}` },
    })

    if (response.status === 403) {
      const body = await response.json().catch(() => null)
      return {
        ok: false,
        reason:
          body?.message ??
          'This account is not registered with the SACCO. Ask an administrator to register you.',
      }
    }

    if (!response.ok) {
      return { ok: false, reason: `The server could not confirm your account (${response.status}).` }
    }

    const body = await response.json()
    return {
      ok: true,
      identity: {
        email: body.email,
        fullName: body.fullName,
        portalRole: body.portalRole as RoleType,
        committeeSeat: (body.committeeSeat as CommitteeSeat) ?? null,
        memberId: body.memberId ?? null,
      },
    }
  } catch {
    return {
      ok: false,
      reason: 'Could not reach the server to confirm your account. Check your connection and try again.',
    }
  }
}

/**
 * Mirrors just enough into cookies for the route guard to work.
 *
 * The token itself is deliberately not among them — it stays in the Supabase client's own storage,
 * which refreshes it. These cookies say only "somebody is signed in, and this is their portal",
 * which is all the middleware needs to send them to the right place. Nothing is trusted from
 * them: the API re-derives everything from the token on every request.
 */
function rememberSession(identity: Identity): void {
  writeSessionCookie(AUTH_COOKIE_NAME, '1')
  writeSessionCookie(ROLE_COOKIE_NAME, identity.portalRole)
  writeSessionCookie(USER_EMAIL_COOKIE_NAME, encodeURIComponent(identity.email))
  if (identity.committeeSeat) {
    writeSessionCookie(SEAT_COOKIE_NAME, encodeURIComponent(identity.committeeSeat))
  }
}

export async function signOut(): Promise<void> {
  const supabase = getSupabase()
  if (supabase) {
    await supabase.auth.signOut().catch(() => undefined)
  }

  if (typeof window !== 'undefined') {
    window.localStorage.removeItem(DEV_TOKEN_KEY)
  }

  for (const name of [AUTH_COOKIE_NAME, ROLE_COOKIE_NAME, USER_EMAIL_COOKIE_NAME, SEAT_COOKIE_NAME]) {
    document.cookie = `${name}=; path=/; max-age=0; samesite=lax`
  }
}

/** The signed-in identity, re-confirmed with the server. Null when nobody is signed in. */
export async function currentIdentity(): Promise<Identity | null> {
  const token = await getAccessToken()
  if (!token) return null

  const result = await fetchIdentity(token)
  return result.ok ? result.identity : null
}

async function requestDevToken(email: string): Promise<string | null> {
  try {
    const response = await fetch(`${API_BASE}/api/auth/dev-token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email }),
    })
    if (!response.ok) return null
    const body = await response.json()
    return body.accessToken ?? null
  } catch {
    return null
  }
}
