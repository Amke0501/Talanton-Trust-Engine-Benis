/**
 * Creates the Supabase logins for already-registered members, using only the project's public key.
 *
 * This is the bootstrap route for a project with email auto-confirm switched on: an ordinary
 * sign-up produces an immediately usable account, so no service-role key is needed. It is the
 * weaker of the two routes and is documented as such — while auto-confirm is on, anyone who finds
 * the project URL can create themselves an account.
 *
 * What stops that mattering is the membership check on the API: a login with no matching Users
 * row is refused at sign-in. A stranger can create a Supabase account and still get nowhere. Turn
 * auto-confirm back off once the accounts below exist, and use the admin route
 * (provision-supabase-accounts.mjs) for members added later.
 *
 *   SUPABASE_URL=https://<project>.supabase.co \
 *   SUPABASE_ANON_KEY=<publishable key> \
 *   node scripts/provision-accounts-public-key.mjs [--password 'Demo123!']
 */

const SUPABASE_URL = process.env.SUPABASE_URL?.replace(/\/+$/, '')
const ANON_KEY = process.env.SUPABASE_ANON_KEY

const args = process.argv.slice(2)
const passwordIndex = args.indexOf('--password')
const PASSWORD = passwordIndex !== -1 ? args[passwordIndex + 1] : 'Demo123!'

if (!SUPABASE_URL || !ANON_KEY) {
  console.error('Set SUPABASE_URL and SUPABASE_ANON_KEY.')
  process.exit(1)
}

const MEMBERS = [
  'applicant@talanton.demo',
  'underwriter@talanton.demo',
  'committee@talanton.demo',
  'chairperson@talanton.demo',
  'treasurer@talanton.demo',
  'secretary@talanton.demo',
  'creditofficer@talanton.demo',
  'boardmember@talanton.demo',
]

const headers = { 'Content-Type': 'application/json', apikey: ANON_KEY }

async function signUp(email) {
  const res = await fetch(`${SUPABASE_URL}/auth/v1/signup`, {
    method: 'POST',
    headers,
    body: JSON.stringify({ email, password: PASSWORD }),
  })
  return { ok: res.ok, status: res.status, body: await res.json().catch(() => ({})) }
}

/**
 * The only proof that matters. Sign-up can report success for an account that cannot actually be
 * used — an unconfirmed one, or an address that already existed — so each account is verified by
 * signing into it.
 */
async function canSignIn(email) {
  const res = await fetch(`${SUPABASE_URL}/auth/v1/token?grant_type=password`, {
    method: 'POST',
    headers,
    body: JSON.stringify({ email, password: PASSWORD }),
  })
  if (!res.ok) {
    const body = await res.json().catch(() => ({}))
    return { ok: false, reason: body.error_description || body.msg || `${res.status}` }
  }
  return { ok: true }
}

const settings = await fetch(`${SUPABASE_URL}/auth/v1/settings`, { headers }).then((r) => r.json())
if (!settings.mailer_autoconfirm) {
  console.error(
    'Email auto-confirm is still off, so any account created here would be unusable.\n' +
      'Supabase dashboard → Authentication → Sign In / Providers → Email → turn "Confirm email" OFF.'
  )
  process.exit(1)
}

console.log(`Project : ${SUPABASE_URL.replace(/^https:\/\/([^.]+).*/, '$1')}`)
console.log(`Password: ${PASSWORD}\n`)

let usable = 0
for (const email of MEMBERS) {
  const created = await signUp(email)
  const verified = await canSignIn(email)

  if (verified.ok) {
    usable += 1
    console.log(`  ok      ${email.padEnd(32)} ${created.ok ? 'created' : 'already existed'}`)
  } else {
    console.log(`  FAILED  ${email.padEnd(32)} ${verified.reason}`)
  }
}

console.log(`\n${usable}/${MEMBERS.length} accounts can sign in.`)
console.log(
  '\nTurn "Confirm email" back ON in the dashboard now that these exist. Members added later ' +
    'should be registered through the app, which creates their login as a confirmed account.'
)
