import { Shield, User } from 'lucide-react'
import { Label } from '@/components/ui/label'
import { AdminUserInfoResponse } from '@/features/admin/auth/adminAuthTypes.ts'

export function ProfileSection({
  userInfo,
}: {
  userInfo: AdminUserInfoResponse | undefined
}) {
  if (!userInfo) {
    return (
      <section className="bg-card border border-border rounded-lg p-6">
        <div className="flex items-center gap-2 mb-4">
          <User className="h-5 w-5 text-primary" />
          <h2 className="text-lg font-semibold">Profile Information</h2>
        </div>
        <p className="text-sm text-muted-foreground">
          No user information available
        </p>
      </section>
    )
  }

  const fullName =
    [userInfo.firstName, userInfo.lastName].filter(Boolean).join(' ') || 'N/A'

  return (
    <section className="bg-card border border-border rounded-lg p-6">
      <div className="flex items-center gap-2 mb-4">
        <User className="h-5 w-5 text-primary" />
        <h2 className="text-lg font-semibold">Profile Information</h2>
      </div>

      <div className="space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              Email
            </Label>
            <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm font-mono">
              {userInfo.email}
            </div>
          </div>

          <div>
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              User ID
            </Label>
            <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm font-mono">
              {userInfo.userId}
            </div>
          </div>

          <div>
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              Full Name
            </Label>
            <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm">
              {fullName}
            </div>
          </div>

          <div>
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              Member Since
            </Label>
            <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm">
              {new Date(userInfo.createdAt).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric',
              })}
            </div>
          </div>
        </div>

        <div>
          <Label className="text-xs text-muted-foreground uppercase tracking-wide">
            Roles
          </Label>
          <div className="mt-1.5 flex flex-wrap gap-2">
            {userInfo.roles.map((role) => (
              <div
                key={role}
                className="inline-flex items-center gap-2 px-3 py-1.5 bg-primary/10 border border-primary/30 rounded text-sm font-medium text-primary"
              >
                <Shield className="h-3.5 w-3.5" />
                {role}
              </div>
            ))}
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 pt-2">
          <div className="flex items-center gap-2">
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              Email Verified
            </Label>
            <span
              className={`text-xs font-medium ${userInfo.emailConfirmed ? 'text-green-600' : 'text-orange-600'}`}
            >
              {userInfo.emailConfirmed ? '✓ Verified' : '✗ Not Verified'}
            </span>
          </div>

          <div className="flex items-center gap-2">
            <Label className="text-xs text-muted-foreground uppercase tracking-wide">
              2FA Status
            </Label>
            <span
              className={`text-xs font-medium ${userInfo.twoFactorEnabled ? 'text-green-600' : 'text-gray-600'}`}
            >
              {userInfo.twoFactorEnabled ? '✓ Enabled' : '✗ Disabled'}
            </span>
          </div>
        </div>
      </div>
    </section>
  )
}
