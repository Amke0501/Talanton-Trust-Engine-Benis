import type { RoleType } from '@/lib/talenton-data'

export const ROLE_COOKIE_NAME = 'talanton_role'
export const AUTH_COOKIE_NAME = 'talanton_auth'
export const USER_EMAIL_COOKIE_NAME = 'talanton_email'

const VALID_ROLES: RoleType[] = ['applicant', 'underwriter', 'committee']

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
