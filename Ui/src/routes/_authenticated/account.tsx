import { createFileRoute } from '@tanstack/react-router'
import { useState, useEffect } from 'react'
import {
  Shield,
  User,
  Key,
  Lock,
  Smartphone,
  Activity,
  LogOut,
  AlertTriangle,
  CheckCircle2,
  XCircle,
  QrCode,
  Copy,
  Check
} from 'lucide-react'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'
import { adminAuthStoreAction } from '@/stores/adminAuthStore.ts'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Skeleton } from '@/components/ui/skeleton'

export const Route = createFileRoute('/_authenticated/account')({
  component: AccountPage,
})

function AccountPage() {
  const [userInfo, setUserInfo] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [show2FASetup, setShow2FASetup] = useState(false)
  const [twoFAData, setTwoFAData] = useState<any>(null)
  const [verificationCode, setVerificationCode] = useState('')
  const [disableCode, setDisableCode] = useState('')
  const [showDisable2FA, setShowDisable2FA] = useState(false)
  const [copiedSecret, setCopiedSecret] = useState(false)
  const [actionLoading, setActionLoading] = useState(false)

  useEffect(() => {
    loadUserInfo()
  }, [])

  const loadUserInfo = async () => {
    try {
      setLoading(true)
      const response = await authApi.me()
      setUserInfo(response.data)
    } catch (error) {
      console.error('Failed to load user info:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleEnable2FA = async () => {
    try {
      setActionLoading(true)
      const response = await authApi.enable2fa()
      setTwoFAData(response.data)
      setShow2FASetup(true)
    } catch (error) {
      console.error('Failed to enable 2FA:', error)
    } finally {
      setActionLoading(false)
    }
  }

  const handleConfirm2FA = async () => {
    if (!verificationCode || verificationCode.length !== 6) return

    try {
      setActionLoading(true)
      await authApi.confirm2fa({ code: verificationCode })
      setShow2FASetup(false)
      setTwoFAData(null)
      setVerificationCode('')
      await loadUserInfo()
    } catch (error) {
      console.error('Failed to confirm 2FA:', error)
    } finally {
      setActionLoading(false)
    }
  }

  const handleDisable2FA = async () => {
    if (!disableCode || disableCode.length !== 6) return

    try {
      setActionLoading(true)
      await authApi.disable2fa({ code: disableCode })
      setShowDisable2FA(false)
      setDisableCode('')
      await loadUserInfo()
    } catch (error) {
      console.error('Failed to disable 2FA:', error)
    } finally {
      setActionLoading(false)
    }
  }

  const handleLogout = async () => {
    try {
      await authApi.logout()
      adminAuthStoreAction.reset()
      window.location.href = '/auth/login'
    } catch (error) {
      console.error('Logout failed:', error)
    }
  }

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text)
    setCopiedSecret(true)
    setTimeout(() => setCopiedSecret(false), 2000)
  }

  if (loading) {
    return <AccountSkeleton />
  }

  return (
    <div className="container max-w-5xl mx-auto px-4 py-8">
      {/* Header */}
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <div className="relative">
            <Shield className="h-8 w-8 text-primary" />
            <div className="absolute inset-0 bg-primary/20 blur-lg rounded-full"></div>
          </div>
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Account Settings</h1>
            <p className="text-sm text-muted-foreground">Manage your security and profile settings</p>
          </div>
        </div>
      </div>

      <div className="grid gap-6">
        {/* Profile Information */}
        <section className="bg-card border border-border rounded-lg p-6">
          <div className="flex items-center gap-2 mb-4">
            <User className="h-5 w-5 text-primary" />
            <h2 className="text-lg font-semibold">Profile Information</h2>
          </div>

          <div className="space-y-4">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <Label className="text-xs text-muted-foreground uppercase tracking-wide">Username</Label>
                <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm font-mono">
                  {userInfo?.username || 'N/A'}
                </div>
              </div>

              <div>
                <Label className="text-xs text-muted-foreground uppercase tracking-wide">User ID</Label>
                <div className="mt-1.5 px-3 py-2 bg-muted/50 border border-border rounded text-sm font-mono">
                  {userInfo?.id || 'N/A'}
                </div>
              </div>
            </div>

            <div>
              <Label className="text-xs text-muted-foreground uppercase tracking-wide">Role</Label>
              <div className="mt-1.5 inline-flex items-center gap-2 px-3 py-1.5 bg-primary/10 border border-primary/30 rounded text-sm font-medium text-primary">
                <Shield className="h-3.5 w-3.5" />
                Administrator
              </div>
            </div>
          </div>
        </section>

        {/* Two-Factor Authentication */}
        <section className="bg-card border border-border rounded-lg p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <Smartphone className="h-5 w-5 text-primary" />
              <h2 className="text-lg font-semibold">Two-Factor Authentication</h2>
            </div>

            <div className="flex items-center gap-2">
              {userInfo?.is2FaEnabled ? (
                <div className="flex items-center gap-1.5 px-2 py-1 bg-security-success/10 border border-security-success/30 rounded text-xs">
                  <CheckCircle2 className="h-3 w-3 text-security-success" />
                  <span className="text-security-success font-medium">Enabled</span>
                </div>
              ) : (
                <div className="flex items-center gap-1.5 px-2 py-1 bg-security-warning/10 border border-security-warning/30 rounded text-xs">
                  <AlertTriangle className="h-3 w-3 text-security-warning" />
                  <span className="text-security-warning font-medium">Disabled</span>
                </div>
              )}
            </div>
          </div>

          <p className="text-sm text-muted-foreground mb-4">
            Add an extra layer of security to your account by enabling two-factor authentication.
          </p>

          {!userInfo?.is2FaEnabled && !show2FASetup && (
            <Button
              onClick={handleEnable2FA}
              disabled={actionLoading}
              className="bg-primary hover:bg-primary/90 text-primary-foreground"
            >
              <Lock className="h-4 w-4 mr-2" />
              Enable 2FA
            </Button>
          )}

          {userInfo?.is2FaEnabled && !showDisable2FA && (
            <Button
              onClick={() => setShowDisable2FA(true)}
              variant="outline"
              className="border-destructive/50 text-destructive hover:bg-destructive/10"
            >
              <XCircle className="h-4 w-4 mr-2" />
              Disable 2FA
            </Button>
          )}

          {/* 2FA Setup Flow */}
          {show2FASetup && twoFAData && (
            <div className="mt-4 p-4 bg-muted/30 border border-primary/30 rounded-lg space-y-4">
              <div className="flex items-start gap-3">
                <div className="shrink-0 p-2 bg-primary/10 rounded">
                  <QrCode className="h-5 w-5 text-primary" />
                </div>
                <div className="flex-1">
                  <h3 className="font-semibold text-sm mb-1">Scan QR Code</h3>
                  <p className="text-xs text-muted-foreground mb-3">
                    Scan this QR code with your authenticator app (Google Authenticator, Authy, etc.)
                  </p>

                  <div className="bg-white p-3 rounded inline-block">
                    <img
                      src={twoFAData.qrCode}
                      alt="2FA QR Code"
                      className="w-48 h-48"
                    />
                  </div>

                  <div className="mt-3">
                    <Label className="text-xs text-muted-foreground uppercase tracking-wide">Or enter this key manually</Label>
                    <div className="mt-1.5 flex items-center gap-2">
                      <code className="flex-1 px-3 py-2 bg-muted border border-border rounded text-xs font-mono break-all">
                        {twoFAData.secret}
                      </code>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => copyToClipboard(twoFAData.secret)}
                        className="shrink-0"
                      >
                        {copiedSecret ? (
                          <Check className="h-4 w-4 text-security-success" />
                        ) : (
                          <Copy className="h-4 w-4" />
                        )}
                      </Button>
                    </div>
                  </div>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="verify-code" className="text-sm">Enter verification code</Label>
                <Input
                  id="verify-code"
                  type="text"
                  placeholder="000000"
                  value={verificationCode}
                  onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                  className="font-mono text-center text-lg tracking-widest"
                  maxLength={6}
                />
              </div>

              <div className="flex gap-2">
                <Button
                  onClick={handleConfirm2FA}
                  disabled={verificationCode.length !== 6 || actionLoading}
                  className="bg-primary hover:bg-primary/90 text-primary-foreground"
                >
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  Confirm & Enable
                </Button>
                <Button
                  onClick={() => {
                    setShow2FASetup(false)
                    setTwoFAData(null)
                    setVerificationCode('')
                  }}
                  variant="outline"
                  disabled={actionLoading}
                >
                  Cancel
                </Button>
              </div>
            </div>
          )}

          {/* Disable 2FA Flow */}
          {showDisable2FA && (
            <div className="mt-4 p-4 bg-destructive/5 border border-destructive/30 rounded-lg space-y-4">
              <div className="flex items-start gap-3">
                <AlertTriangle className="h-5 w-5 text-destructive shrink-0 mt-0.5" />
                <div className="flex-1">
                  <h3 className="font-semibold text-sm mb-1">Disable Two-Factor Authentication</h3>
                  <p className="text-xs text-muted-foreground mb-3">
                    Enter your current 2FA code to disable two-factor authentication. This will make your account less secure.
                  </p>
                </div>
              </div>

              <div className="space-y-2">
                <Label htmlFor="disable-code" className="text-sm">Verification code</Label>
                <Input
                  id="disable-code"
                  type="text"
                  placeholder="000000"
                  value={disableCode}
                  onChange={(e) => setDisableCode(e.target.value.replace(/\D/g, '').slice(0, 6))}
                  className="font-mono text-center text-lg tracking-widest"
                  maxLength={6}
                />
              </div>

              <div className="flex gap-2">
                <Button
                  onClick={handleDisable2FA}
                  disabled={disableCode.length !== 6 || actionLoading}
                  variant="outline"
                  className="border-destructive/50 text-destructive hover:bg-destructive/10"
                >
                  <XCircle className="h-4 w-4 mr-2" />
                  Disable 2FA
                </Button>
                <Button
                  onClick={() => {
                    setShowDisable2FA(false)
                    setDisableCode('')
                  }}
                  variant="outline"
                  disabled={actionLoading}
                >
                  Cancel
                </Button>
              </div>
            </div>
          )}
        </section>

        {/* Security Information */}
        <section className="bg-card border border-border rounded-lg p-6">
          <div className="flex items-center gap-2 mb-4">
            <Activity className="h-5 w-5 text-primary" />
            <h2 className="text-lg font-semibold">Security Status</h2>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="p-3 bg-muted/30 border border-border rounded">
              <div className="flex items-center gap-2 mb-1">
                <Key className="h-4 w-4 text-muted-foreground" />
                <span className="text-xs text-muted-foreground uppercase tracking-wide">Authentication</span>
              </div>
              <div className="text-sm font-medium">Token-based Auth</div>
            </div>

            <div className="p-3 bg-muted/30 border border-border rounded">
              <div className="flex items-center gap-2 mb-1">
                <Shield className="h-4 w-4 text-muted-foreground" />
                <span className="text-xs text-muted-foreground uppercase tracking-wide">2FA Status</span>
              </div>
              <div className="text-sm font-medium">
                {userInfo?.is2FaEnabled ? 'Protected' : 'Not Protected'}
              </div>
            </div>
          </div>
        </section>

        {/* Danger Zone */}
        <section className="bg-card border border-destructive/30 rounded-lg p-6">
          <div className="flex items-center gap-2 mb-4">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <h2 className="text-lg font-semibold text-destructive">Danger Zone</h2>
          </div>

          <div className="space-y-3">
            <p className="text-sm text-muted-foreground">
              Logout from your account and clear all session data.
            </p>

            <Button
              onClick={handleLogout}
              variant="outline"
              className="border-destructive/50 text-destructive hover:bg-destructive/10"
            >
              <LogOut className="h-4 w-4 mr-2" />
              Logout
            </Button>
          </div>
        </section>
      </div>
    </div>
  )
}

function AccountSkeleton() {
  return (
    <div className="container max-w-5xl mx-auto px-4 py-8">
      <div className="mb-8">
        <div className="flex items-center gap-3 mb-2">
          <Shield className="h-8 w-8 text-primary/50 animate-pulse" />
          <div>
            <Skeleton className="h-7 w-48 mb-2" />
            <Skeleton className="h-4 w-64" />
          </div>
        </div>
      </div>

      <div className="grid gap-6">
        {[1, 2, 3, 4].map((i) => (
          <div key={i} className="bg-card border border-border rounded-lg p-6">
            <Skeleton className="h-6 w-48 mb-4" />
            <div className="space-y-3">
              <Skeleton className="h-10 w-full" />
              <Skeleton className="h-10 w-full" />
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}