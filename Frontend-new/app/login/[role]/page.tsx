import { redirect } from 'next/navigation'
import { RoleLoginPage } from '@/components/talenton/role-login-page'
import { normalizeRole } from '@/lib/role-access'

export default async function LoginByRolePage({
  params,
}: {
  params: Promise<{ role: string }>
}) {
  const resolved = await params
  const role = normalizeRole(resolved.role)

  if (!role) {
    redirect('/')
  }

  return <RoleLoginPage role={role} />
}
