import { createFileRoute } from '@tanstack/react-router'
import { Shield, User, Lock, AlertTriangle } from 'lucide-react'
import { authApi } from '@/features/admin/auth/adminAuthApi'
import { adminAuthStoreAction } from '@/stores/adminAuthStore'
import { useState } from 'react'
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

type TabType = 'profile' | 'security' | 'danger-zone'

function AccountPage() {
  const [activeTab, setActiveTab] = useState<TabType>('profile')
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

  const tabs = [
    {
      id: 'profile' as TabType,
      label: 'Profile',
      icon: User,
      description: 'Manage your personal information',
    },
    {
      id: 'security' as TabType,
      label: 'Security',
      icon: Lock,
      description: 'Two-factor authentication & security settings',
    },
    {
      id: 'danger-zone' as TabType,
      label: 'Danger Zone',
      icon: AlertTriangle,
      description: 'Account deletion and logout',
    },
  ]

  return (
    <div className="container max-w-6xl mx-auto px-4 py-8">
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

      {/* Tabs Navigation */}
      <div className="border-b border-border mb-6">
        <div className="flex gap-1">
          {tabs.map((tab) => {
            const Icon = tab.icon
            const isActive = activeTab === tab.id
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`
                  relative px-4 py-3 flex items-center gap-2 text-sm font-medium transition-colors
                  ${
                  isActive
                    ? 'text-primary border-b-2 border-primary'
                    : 'text-muted-foreground hover:text-foreground'
                }
                `}
              >
                <Icon className="h-4 w-4" />
                {tab.label}
              </button>
            )
          })}
        </div>
      </div>

      {/* Tab Content */}
      <div className="grid gap-6">
        {activeTab === 'profile' && (
          <div className="space-y-6">
            <ProfileSection userInfo={user} />
          </div>
        )}

        {activeTab === 'security' && (
          <div className="space-y-6">
            <TwoFactorSection />
            <SecurityStatusSection userInfo={user} />
          </div>
        )}

        {activeTab === 'danger-zone' && (
          <div className="space-y-6">
            <DangerZoneSection onLogout={handleLogout} />
          </div>
        )}
      </div>
    </div>
  )
}