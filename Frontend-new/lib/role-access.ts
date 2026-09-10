import type { RoleType } from '@/lib/talenton-data'

export const ROLE_COOKIE_NAME = 'talanton_role'
export const AUTH_COOKIE_NAME = 'talanton_auth'
export const USER_EMAIL_COOKIE_NAME = 'talanton_email'

const VALID_ROLES: RoleType[] = ['applicant', 'underwriter', 'committee']

/**
 * A committee member's seat on the board. This is a second dimension alongside the portal role:
 * everyone below signs into the `committee` portal, but the seat decides what they may do there.
 *
 * The seat drives two of the founder's rules that previously had no one to apply to — whose vote
 * counts toward quorum (Chairperson and Treasurer must both approve a big loan, and a Chairperson
 * REJECT is an absolute veto), and who may release funds (Treasurer for small loans, Chairperson
 * plus Secretary for big ones). Without seats there was no way to be "the wrong person".
 *
 * These strings are the contract with the backend: they must match the MemberRole values compared
 * in QuorumEvaluationService and DisbursementAuthorizationService.
 */
export const COMMITTEE_SEATS = [
  'Chairperson',
  'Treasurer',
  'Secretary',
  'Credit Officer',
  'Board Member',
] as const

export type CommitteeSeat = (typeof COMMITTEE_SEATS)[number]

export const SEAT_COOKIE_NAME = 'talanton_seat'

/** The seat assumed when a committee session has none recorded. */
export const DEFAULT_COMMITTEE_SEAT: CommitteeSeat = 'Credit Officer'

export function normalizeSeat(value: string | null | undefined): CommitteeSeat | null {
  if (!value) return null
  const decoded = decodeURIComponent(value).trim().toLowerCase()
  return COMMITTEE_SEATS.find((seat) => seat.toLowerCase() === decoded) ?? null
}

/** Reads the signed-in committee seat from the browser's cookies. */
export function readSeatFromCookie(): CommitteeSeat {
  if (typeof document === 'undefined') return DEFAULT_COMMITTEE_SEAT
  const match = document.cookie.split('; ').find((c) => c.startsWith(`${SEAT_COOKIE_NAME}=`))
  return normalizeSeat(match?.split('=')[1]) ?? DEFAULT_COMMITTEE_SEAT
}

/**
 * How long a signed-in session lasts, in seconds.
 *
 * The session cookies were written with no lifetime and no Secure flag at all, which made them
 * session cookies the browser was free to drop — and over HTTPS, a SameSite cookie without
 * Secure is rejected outright by some browsers. Either way the next request arrived with no
 * auth cookie, the middleware saw an unauthenticated request and bounced the user to the
 * landing page: QA saw this as "the app logs you out when you refresh the page".
 *
 * Writing an explicit Max-Age, and Secure whenever the page is served over HTTPS, makes the
 * session survive a reload — and a closed tab — rather than depending on browser policy.
 */
export const SESSION_MAX_AGE_SECONDS = 60 * 60 * 8

/** Writes one session cookie so it actually survives a page reload. */
export function writeSessionCookie(name: string, value: string): void {
  if (typeof document === 'undefined') return
  const secure = typeof location !== 'undefined' && location.protocol === 'https:' ? '; secure' : ''
  document.cookie = `${name}=${value}; path=/; max-age=${SESSION_MAX_AGE_SECONDS}; samesite=lax${secure}`
}

/** Reads one cookie written by {@link writeSessionCookie}. */
export function readCookie(name: string): string | null {
  if (typeof document === 'undefined') return null
  const match = document.cookie.split('; ').find((c) => c.startsWith(`${name}=`))
  return match ? decodeURIComponent(match.slice(name.length + 1)) : null
}

export function normalizeRole(value: string | null | undefined): RoleType | null {
  if (!value) return null
  const lowered = value.toLowerCase()
  return VALID_ROLES.includes(lowered as RoleType) ? (lowered as RoleType) : null
}

export function resolveRole(options: {
  cookieRole?: string | null
  queryRole?: string | null
  fallbackRole?: RoleType
}): RoleType {
  const fallbackRole = options.fallbackRole ?? 'applicant'
  return (
    normalizeRole(options.queryRole) ??
    normalizeRole(options.cookieRole) ??
    fallbackRole
  )
}
