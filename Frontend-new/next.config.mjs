/**
 * The browser can only see environment variables that Next inlines at build time, and it inlines
 * exactly those prefixed NEXT_PUBLIC_. A value set under any other name — NEXT_API_URL, say — is
 * server-only, so client code reads undefined and silently falls back to localhost, which looks
 * identical to an unreachable API.
 *
 * The `env` block below is the documented escape hatch: whatever the host will let you name the
 * variable, it reaches the browser as NEXT_PUBLIC_API_URL.
 */
const apiUrl =
  process.env.NEXT_PUBLIC_API_URL ||
  process.env.NEXT_API_URL ||
  process.env.API_URL ||
  ''

if (!apiUrl) {
  console.warn(
    '[next.config] No API URL configured. Set NEXT_PUBLIC_API_URL (or NEXT_API_URL) or the app ' +
      'will fall back to http://localhost:5195 and every server-backed panel will report the API unreachable.'
  )
} else {
  console.log(`[next.config] API base URL: ${apiUrl}`)
}

/** @type {import('next').NextConfig} */
const nextConfig = {
  // Lets a second build (a smoke test, a preview) write somewhere other than .next, so it does
  // not fight with a dev server already running from this directory.
  distDir: process.env.NEXT_DIST_DIR || '.next',
  env: {
    NEXT_PUBLIC_API_URL: apiUrl,
  },
  typescript: {
    ignoreBuildErrors: true,
  },
  images: {
    unoptimized: true,
  },
}

export default nextConfig
