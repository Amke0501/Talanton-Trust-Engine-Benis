import { NextResponse } from 'next/server'
import {
  AUTH_COOKIE_NAME,
  ROLE_COOKIE_NAME,
  USER_EMAIL_COOKIE_NAME,
} from '@/lib/role-access'

export async function GET(request: Request) {
  const response = NextResponse.redirect(new URL('/', request.url))
  response.cookies.set(AUTH_COOKIE_NAME, '', { path: '/', maxAge: 0 })
  response.cookies.set(ROLE_COOKIE_NAME, '', { path: '/', maxAge: 0 })
  response.cookies.set(USER_EMAIL_COOKIE_NAME, '', { path: '/', maxAge: 0 })
  return response
}
