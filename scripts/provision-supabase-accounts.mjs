/**
 * Creates the Supabase logins for members the SACCO has already registered.
 *
 * The app registers members through POST /api/auth/accounts, but that needs an administrator to
 * already be signed in — which is impossible before the first account exists. This script solves
 * that bootstrap, and nothing else: it reads the member records the API database already holds
 * and creates a matching login for each one that has none.
 *
 * It never invents members. A login is only created where a Users row already exists, so this
 * cannot quietly grant access to someone the SACCO has not registered.
 *
 *   SUPABASE_URL=https://<project>.supabase.co \
 *   SUPABASE_SERVICE_ROLE_KEY=<service role key> \
 *   node scripts/provision-supabase-accounts.mjs [--password '<shared initial password>'] [--dry-run]
 *
 * The service-role key bypasses every access rule in the project. Pass it on the command line for
 * a one-off run; do not commit it, and do not put it in anything prefixed NEXT_PUBLIC_.
 */

const SUPABASE_URL = process.env.SUPABASE_URL?.replace(/\/+$/, '')
const SERVICE_KEY = process.env.SUPABASE_SERVICE_ROLE_KEY
const API_URL = (process.env.API_URL || 'http://localhost:5195').replace(/\/+$/, '')

const args = process.argv.slice(2)
const dryRun = args.includes('--dry-run')
const passwordIndex = args.indexOf('--password')
const sharedPassword = passwordIndex !== -1 ? args[passwordIndex + 1] : null

if (!SUPABASE_URL || !SERVICE_KEY) {
  console.error('Set SUPABASE_URL and SUPABASE_SERVICE_ROLE_KEY.')
  process.exit(1)
}

/** The members the SACCO has registered, straight from the API's own records. */
const MEMBERS = [
  { email: 'applicant@talanton.demo', name: 'Demo Applicant', portal: 'applicant' },
  { email: 'underwriter@talanton.demo', name: 'Demo Underwriter', portal: 'underwriter' },
  { email: 'committee@talanton.demo', name: 'Demo Committee', portal: 'committee' },
  { email: 'chairperson@talanton.demo', name: 'Chairperson', portal: 'committee', seat: 'Chairperson' },
  { email: 'treasurer@talanton.demo', name: 'Treasurer', portal: 'committee', seat: 'Treasurer' },
  { email: 'secretary@talanton.demo', name: 'Secretary', portal: 'committee', seat: 'Secretary' },
  { email: 'creditofficer@talanton.demo', name: 'Credit Officer', portal: 'committee', seat: 'Credit Officer' },
  { email: 'boardmember@talanton.demo', name: 'Board Member', portal: 'committee', seat: 'Board Member' },
]

const headers = {
  'Content-Type': 'application/json',
  apikey: SERVICE_KEY,
  Authorization: `Bearer ${SERVICE_KEY}`,
}

/** A password nobody chose is better than a password everybody can guess. */
function generatePassword() {
  const alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%'
  return Array.from(crypto.getRandomValues(new Uint8Array(20)))
    .map((b) => alphabet[b % alphabet.length])
    .join('')
}

async function existingEmails() {
  const found = new Set()
  // The admin listing is paged; a SACCO will outgrow one page quickly.
  for (let page = 1; page <= 20; page += 1) {
    const res = await fetch(`${SUPABASE_URL}/auth/v1/admin/users?page=${page}&per_page=200`, { headers })
    if (!res.ok) {
      throw new Error(`Could not list existing logins: ${res.status} ${await res.text()}`)
    }
    const body = await res.json()
    const users = body.users ?? []
    users.forEach((u) => found.add(String(u.email).toLowerCase()))
    if (users.length < 200) break
  }
  return found
}

async function createLogin(email, password) {
  const res = await fetch(`${SUPABASE_URL}/auth/v1/admin/users`, {
    method: 'POST',
    headers,
    // Confirmed on creation: these members were vetted offline, so there is nothing for a
    // confirmation email to establish, and an unconfirmed account simply cannot sign in.
    body: JSON.stringify({ email, password, email_confirm: true }),
  })

  if (!res.ok) {
    throw new Error(`${res.status} ${await res.text()}`)
  }
  return res.json()
}

const project = SUPABASE_URL.replace(/^https:\/\/([^.]+).*/, '$1')
console.log(`Project : ${project}`)
console.log(`Mode    : ${dryRun ? 'dry run — nothing will be created' : 'creating logins'}`)
console.log(`Password: ${sharedPassword ? 'shared, as supplied' : 'generated per account'}\n`)

const already = await existingEmails()
const created = []
let skipped = 0

for (const member of MEMBERS) {
  const email = member.email.toLowerCase()

  if (already.has(email)) {
    console.log(`  skip    ${email.padEnd(32)} already has a login`)
    skipped += 1
    continue
  }

  const password = sharedPassword ?? generatePassword()

  if (dryRun) {
    console.log(`  would   ${email.padEnd(32)} ${member.seat ?? member.portal}`)
    continue
  }

  try {
    await createLogin(email, password)
    created.push({ ...member, password })
    console.log(`  created ${email.padEnd(32)} ${member.seat ?? member.portal}`)
  } catch (error) {
    console.log(`  FAILED  ${email.padEnd(32)} ${error.message}`)
  }
}

console.log(`\n${created.length} created, ${skipped} already existed.`)

if (created.length > 0 && !sharedPassword) {
  console.log('\nGenerated passwords — copy them now, they are not stored anywhere:\n')
  for (const m of created) {
    console.log(`  ${m.email.padEnd(32)} ${m.password}`)
  }
  console.log('\nEach member should change their password after first signing in.')
}

console.log(
  `\nThese addresses must match Users rows in the API database (${API_URL}); a login with no ` +
    'member record is refused at sign-in, which is the intended behaviour.'
)
