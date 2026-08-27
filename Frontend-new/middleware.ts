import { NextResponse, type NextRequest } from 'next/server'
import {
  AUTH_COOKIE_NAME,
  ROLE_COOKIE_NAME,
  normalizeRole,
  resolveRole,
} from '@/lib/role-access'

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl
  if (!pathname.startsWith('/dashboard')) {
    return NextResponse.next()
  }

  const pathRole = normalizeRole(pathname.split('/')[2])
  const isAuthenticated = request.cookies.get(AUTH_COOKIE_NAME)?.value === '1'

  if (!isAuthenticated) {
    const url = request.nextUrl.clone()
    url.pathname = '/'
    url.search = ''
    return NextResponse.redirect(url)
  }

  const cookieRole = request.cookies.get(ROLE_COOKIE_NAME)?.value ?? null
  const effectiveRole = resolveRole({ cookieRole })

  const redirectToRole = (role: string) => {
    const url = request.nextUrl.clone()
    url.pathname = `/dashboard/${role}`
    url.search = ''
    const response = NextResponse.redirect(url)
    response.cookies.set(ROLE_COOKIE_NAME, role, {
      path: '/',
      sameSite: 'lax',
    })
    return response
  }

  if (pathname === '/dashboard' || pathname === '/dashboard/') {
    return redirectToRole(effectiveRole)
  }

  if (!pathRole) {
    return redirectToRole(effectiveRole)
  }

  if (pathRole !== effectiveRole) {
    return redirectToRole(effectiveRole)
  }

  const response = NextResponse.next()
  if (cookieRole !== effectiveRole) {
    response.cookies.set(ROLE_COOKIE_NAME, effectiveRole, {
      path: '/',
      sameSite: 'lax',
    })
  }
  return response
}

export const config = {
  matcher: ['/dashboard/:path*'],
}
