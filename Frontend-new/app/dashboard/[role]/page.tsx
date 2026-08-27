import { redirect } from 'next/navigation'
import { DashboardRolePage } from '@/components/talenton/dashboard-role-page'
import { normalizeRole } from '@/lib/role-access'

export default async function RoleDashboardPage({
  params,
}: {
  params: Promise<{ role: string }>
}) {
  const resolved = await params
  const role = normalizeRole(resolved.role)

  if (!role) {
    redirect('/dashboard/applicant')
  }

  return <DashboardRolePage role={role} />
}
