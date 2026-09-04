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
