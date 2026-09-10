import { NextResponse } from 'next/server'
import {
  AUTH_COOKIE_NAME,
  ROLE_COOKIE_NAME,
  SEAT_COOKIE_NAME,
  USER_EMAIL_COOKIE_NAME,
} from '@/lib/role-access'

/**
 * Ends the session. POST only, and that is the whole point.
 *
 * This used to be a GET, reached from a `<Link href="/logout">` in every sidebar. Next prefetches
 * links, so simply rendering a dashboard fired a background GET at this handler, which dutifully
 * cleared the session cookies. The page the user was looking at kept working — it was already
 * rendered — but the cookies were gone, so the next navigation or refresh hit the middleware
 * with no auth cookie and was bounced to the landing page.
 *
 * That is QA's "the app logs the user out when the page is refreshed", and it was intermittent
 * precisely because prefetching is: it depends on what scrolled into view and what was hovered.
 *
 * A GET must not change state. Logging out is a state change, so it takes a POST, which nothing
 * prefetches. The sidebars submit a form here.
 */
function clearedSession(target: URL) {
  // 303, not the default 307: a 307 preserves the method, so the browser would re-POST to the
  // landing page instead of fetching it.
  const response = NextResponse.redirect(target, 303)
  for (const name of [AUTH_COOKIE_NAME, ROLE_COOKIE_NAME, USER_EMAIL_COOKIE_NAME, SEAT_COOKIE_NAME]) {
    response.cookies.set(name, '', { path: '/', maxAge: 0 })
  }
  return response
}

export async function POST(request: Request) {
  return clearedSession(new URL('/', request.url))
}

/**
 * Someone who types or bookmarks /logout still expects to be signed out, so this clears the
 * session too — but only for a real top-level navigation. A prefetch identifies itself, and is
 * answered with a redirect that touches nothing.
 */
export async function GET(request: Request) {
  const headers = request.headers
  const isPrefetch =
    headers.get('next-router-prefetch') === '1' ||
    headers.get('purpose') === 'prefetch' ||
    headers.get('x-purpose') === 'prefetch' ||
    headers.get('sec-purpose')?.includes('prefetch') === true

  const target = new URL('/', request.url)
  return isPrefetch ? NextResponse.redirect(target, 303) : clearedSession(target)
}
