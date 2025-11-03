import { createFileRoute } from '@tanstack/react-router'
import { Shield } from 'lucide-react'
import { authApi } from '@/features/admin/auth/adminAuthApi'
import { adminAuthStoreAction } from '@/stores/adminAuthStore'

import { ProfileSection } from '@/features/account/component/ProfileSection'
import { TwoFactorSection } from '@/features/account/component/TwoFactorSection'
import { SecurityStatusSection } from '@/features/account/component/SecurityStatusSection'
import { DangerZoneSection } from '@/features/account/component/DangerZoneSection'
import { AccountSkeleton } from '@/features/account/component/AccountSkeleton'
import { useAuthUser } from '@/features/admin/auth/useAdminAuth.ts'
import { ApiErrorsMessage } from '@/components/ApiErrorsMessage.tsx'

export const Route = createFileRoute('/_authenticated/account')({
  component: AccountPage,
})

function AccountPage() {
  const { data: user, isLoading, error } = useAuthUser()
  const apiErrors = error?.response?.data?.error

  const handleLogout = async () => {
    try {
      await authApi.logout()
      adminAuthStoreAction.reset()
      window.location.href = '/auth/login'
    } catch (error) {
      console.error('Logout failed:', error)
    }
  }

  if (isLoading) return <AccountSkeleton />
  if (error) {
    return (
      <div className="p-8 text-center text-destructive">
        <ApiErrorsMessage apiErrors={apiErrors} />
      </div>
    )
  }
  return (
    <div className="container max-w-5xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <Shield className="h-8 w-8 text-primary" />
          <div>
            <h1 className="text-2xl font-bold tracking-tight">
              Account Settings
            </h1>
            <p className="text-sm text-muted-foreground">
              Manage your security and profile settings
            </p>
          </div>
        </div>
      </div>

      <div className="grid gap-6">
        <ProfileSection userInfo={user} />
        <TwoFactorSection userInfo={user} />
        <SecurityStatusSection userInfo={user} />
        <DangerZoneSection onLogout={handleLogout} />
      </div>
    </div>
  )
}
