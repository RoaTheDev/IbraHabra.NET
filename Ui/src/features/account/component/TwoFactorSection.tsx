import { useState } from 'react'
import {
  AlertTriangle,
  Check,
  CheckCircle2,
  Copy,
  Download,
  Lock,
  RefreshCw,
  Shield,
  Smartphone,
  XCircle,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  useAuthUser,
  useConfirm2Fa,
  useDisable2Fa,
  useEnable2Fa,
  useRegenerateRecoveryCodes,
} from '@/features/admin/auth/useAdminAuth'
import { toast } from 'sonner'
import { QRCodeSVG } from 'qrcode.react'
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from '@/components/ui/input-otp.tsx'

export function TwoFactorSection() {
  const [show2FASetup, setShow2FASetup] = useState(false)
  const [verificationCode, setVerificationCode] = useState('')
  const [disablePassword, setDisablePassword] = useState('')
  const [showDisable2FA, setShowDisable2FA] = useState(false)
  const [copiedSecret, setCopiedSecret] = useState(false)

  const [showRegenerateRecoveryCodes, setShowRegenerateRecoveryCodes] =
    useState(false)
  const [regeneratePassword, setRegeneratePassword] = useState('')

  const { data: userInfo, refetch } = useAuthUser()

  const enable2FaMutation = useEnable2Fa()
  const confirm2FaMutation = useConfirm2Fa()
  const disable2FaMutation = useDisable2Fa()
  const regenerateRecoveryCodesMutation = useRegenerateRecoveryCodes()

  const handleEnable2FA = async () => {
    enable2FaMutation.mutate(undefined, {
      onSuccess: () => {
        setShow2FASetup(true)
      },
      onError: (error) => {
        const errorMessage =
          error.response?.data?.error?.[0]?.message || 'Failed to enable 2FA'
        toast.error(errorMessage)
      },
    })
  }

  const handleConfirm2FA = async () => {
    if (verificationCode.length !== 6) {
      toast.error('Please enter a 6-digit code')
      return
    }

    confirm2FaMutation.mutate(
      { code: verificationCode },
      {
        onSuccess: (response) => {
          toast.success(response.data.message || '2FA enabled successfully!')
          setShow2FASetup(false)
          setVerificationCode('')
          refetch()
        },
        onError: (error) => {
          const errorMessage =
            error.response?.data?.error?.[0]?.message ||
            'Invalid verification code'
          toast.error(errorMessage)
        },
      },
    )
  }

  const handleDisable2FA = async () => {
    if (!disablePassword) {
      toast.error('Please enter your password')
      return
    }

    disable2FaMutation.mutate(
      { password: disablePassword },
      {
        onSuccess: (response) => {
          toast.success(response.data.message || '2FA disabled successfully')
          setShowDisable2FA(false)
          setDisablePassword('')
          refetch()
        },
        onError: (error) => {
          const errorMessage =
            error.response?.data?.error?.[0]?.message || 'Failed to disable 2FA'
          toast.error(errorMessage)
        },
      },
    )
  }

  const handleRegenerateRecoveryCodes = async () => {
    if (!regeneratePassword) {
      toast.error('Please enter your password')
      return
    }

    regenerateRecoveryCodesMutation.mutate(
      { password: regeneratePassword },
      {
        onSuccess: (response) => {
          toast.success(
            response.data.message || 'New recovery codes generated!',
          )
          setRegeneratePassword('')
          // Keep the modal open to show the new codes
        },
        onError: (error) => {
          const errorMessage =
            error.response?.data?.error?.[0]?.message ||
            'Failed to generate recovery codes'
          toast.error(errorMessage)
        },
      },
    )
  }

  const handleCancelDisable = () => {
    setShowDisable2FA(false)
    setDisablePassword('')
  }

  const handleCancelRegenerate = () => {
    setShowRegenerateRecoveryCodes(false)
    setRegeneratePassword('')
    regenerateRecoveryCodesMutation.reset()
  }

  const copyToClipboard = async (text: string) => {
    await navigator.clipboard.writeText(text)
    setCopiedSecret(true)
    setTimeout(() => setCopiedSecret(false), 2000)
  }

  const downloadRecoveryCodes = (codes: string[]) => {
    const codesText = codes.join('\n')
    const blob = new Blob([codesText], { type: 'text/plain' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = 'recovery-codes.txt'
    a.click()
    URL.revokeObjectURL(url)
  }

  const isLoading =
    enable2FaMutation.isPending ||
    confirm2FaMutation.isPending ||
    disable2FaMutation.isPending ||
    regenerateRecoveryCodesMutation.isPending

  return (
    <section className="bg-card border border-border rounded-lg p-6">
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center gap-2">
          <Smartphone className="h-5 w-5 text-primary" />
          <h2 className="text-lg font-semibold">Two-Factor Authentication</h2>
        </div>

        {userInfo?.twoFactorEnabled ? (
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

      <p className="text-sm text-muted-foreground mb-4">
        Add an extra layer of security to your account by enabling two-factor
        authentication.
      </p>

      {/* Enable / Disable / Regenerate Buttons */}
      {!userInfo?.twoFactorEnabled && !show2FASetup && (
        <Button onClick={handleEnable2FA} disabled={isLoading}>
          <Lock className="h-4 w-4 mr-2" /> Enable 2FA
        </Button>
      )}

      {userInfo?.twoFactorEnabled &&
        !showDisable2FA &&
        !showRegenerateRecoveryCodes && (
          <div className="flex gap-2">
            <Button
              onClick={() => setShowDisable2FA(true)}
              variant="outline"
              className="border-destructive/50 text-destructive hover:bg-destructive/10"
            >
              <XCircle className="h-4 w-4 mr-2" /> Disable 2FA
            </Button>
            <Button
              onClick={() => setShowRegenerateRecoveryCodes(true)}
              variant="outline"
            >
              <RefreshCw className="h-4 w-4 mr-2" /> Regenerate Recovery Codes
            </Button>
          </div>
        )}

      {/* Setup QR flow */}
      {show2FASetup && enable2FaMutation.data?.data && (
        <div className="mt-6 rounded-lg border border-primary/20 bg-muted/10 p-6 space-y-6">
          {/* Step 1 - QR Code */}
          <div className="mt-4 md:mt-0 flex-1 space-y-3">
            <h3 className="font-semibold text-base text-primary">
              Step 1: Scan the QR Code
            </h3>
            <p className="text-sm text-muted-foreground leading-relaxed">
              Scan this QR code with your authenticator app (Google
              Authenticator, Authy, etc.). If you can’t scan the QR code, use
              the manual entry key below.
            </p>
          </div>
          <div className="flex flex-col md:flex-row md:items-center md:gap-8">
            <div className="flex-shrink-0 self-center">
              <div className="bg-white p-4 rounded-lg border border-border inline-block">
                <QRCodeSVG
                  value={enable2FaMutation.data.data.authenticatorUri}
                  size={180}
                  level="M"
                />
              </div>
            </div>
          </div>

          <div className="space-y-2">
            <Label className="text-xs font-medium uppercase text-muted-foreground tracking-wide">
              Manual Entry Key
            </Label>
            <div className="flex items-center gap-2">
              <code className="flex-1 px-3 py-2 bg-background border border-border rounded text-xs font-mono break-all">
                {enable2FaMutation.data.data.sharedKey}
              </code>
              <Button
                size="sm"
                variant="outline"
                onClick={() =>
                  copyToClipboard(enable2FaMutation.data.data.sharedKey)
                }
              >
                {copiedSecret ? (
                  <Check className="h-4 w-4" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>

          {/* Step 2 - Recovery Codes */}
          {enable2FaMutation.data.data.recoveryCodes?.length > 0 && (
            <div className="rounded-lg border border-orange-200 dark:border-orange-900 bg-orange-50 dark:bg-orange-950/20 p-4 space-y-3">
              <div className="flex items-center justify-between">
                <Label className="text-sm font-medium text-orange-600 dark:text-orange-400">
                  Step 2: Save Your Recovery Codes
                </Label>
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() =>
                    downloadRecoveryCodes(
                      enable2FaMutation.data.data.recoveryCodes,
                    )
                  }
                >
                  <Download className="h-3 w-3 mr-1" /> Download
                </Button>
              </div>
              <p className="text-xs text-orange-800 dark:text-orange-200">
                ⚠️ Each code can be used only once. Store them safely — they’ll
                let you access your account if you lose your authenticator.
              </p>
              <div className="grid grid-cols-2 sm:grid-cols-3 gap-1 bg-background border border-border rounded p-3 max-h-48 overflow-y-auto">
                {enable2FaMutation.data.data.recoveryCodes.map((code, idx) => (
                  <code key={idx} className="text-xs font-mono">
                    {idx + 1}. {code}
                  </code>
                ))}
              </div>
            </div>
          )}

          {/* Step 3 - Verification */}
          <div className="space-y-2">
            <Label htmlFor="verification-code" className="text-sm font-medium">
              Step 3: Enter 6-Digit Code
            </Label>
            <div className="flex flex-col items-center gap-3">
              <InputOTP
                id="verification-code"
                maxLength={6}
                value={verificationCode}
                onChange={setVerificationCode}
              >
                <InputOTPGroup className={'border-1 border-zinc-100'}>
                  {[...Array(6)].map((_, i) => (
                    <InputOTPSlot key={i} index={i} />
                  ))}
                </InputOTPGroup>
              </InputOTP>
            </div>
          </div>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-3 pt-2">
            <Button
              onClick={handleConfirm2FA}
              disabled={verificationCode.length !== 6 || isLoading}
              className="flex-1"
            >
              <CheckCircle2 className="h-4 w-4 mr-2" /> Confirm & Enable
            </Button>
            <Button
              variant="outline"
              onClick={() => {
                setShow2FASetup(false)
                setVerificationCode('')
              }}
              disabled={isLoading}
              className="flex-1"
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      {/* Disable 2FA Flow */}
      {showDisable2FA && (
        <div className="mt-4 p-4 bg-destructive/5 border border-destructive/30 rounded-lg space-y-4">
          <p className="text-sm text-muted-foreground">
            To disable two-factor authentication, please confirm your password.
          </p>

          <div>
            <Label htmlFor="disable-password">Password</Label>
            <Input
              id="disable-password"
              type="password"
              placeholder="Enter your password"
              value={disablePassword}
              onChange={(e) => setDisablePassword(e.target.value)}
              onKeyDown={async (e) => {
                if (e.key === 'Enter' && disablePassword) {
                  await handleDisable2FA()
                }
              }}
            />
          </div>

          <div className="flex gap-2">
            <Button
              onClick={handleDisable2FA}
              disabled={!disablePassword || isLoading}
              variant="outline"
              className="border-destructive/50 text-destructive hover:bg-destructive/10"
            >
              <XCircle className="h-4 w-4 mr-2" /> Disable 2FA
            </Button>
            <Button
              onClick={handleCancelDisable}
              variant="outline"
              disabled={isLoading}
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      {/* Regenerate Recovery Codes Flow */}
      {showRegenerateRecoveryCodes && (
        <div className="mt-4 p-4 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-900 rounded-lg space-y-4">
          <div className="flex items-start gap-2">
            <Shield className="h-5 w-5 text-blue-600 dark:text-blue-400 mt-0.5 flex-shrink-0" />
            <div>
              <h3 className="font-semibold text-sm text-blue-900 dark:text-blue-100 mb-1">
                Regenerate Recovery Codes
              </h3>
              <p className="text-xs text-blue-800 dark:text-blue-200">
                This will invalidate all your existing recovery codes and
                generate 10 new ones. Make sure to save the new codes securely.
              </p>
            </div>
          </div>

          {!regenerateRecoveryCodesMutation.data ? (
            <>
              <div>
                <Label htmlFor="regenerate-password">Confirm Password</Label>
                <Input
                  id="regenerate-password"
                  type="password"
                  placeholder="Enter your password"
                  value={regeneratePassword}
                  onChange={(e) => setRegeneratePassword(e.target.value)}
                  onKeyDown={async (e) => {
                    if (e.key === 'Enter' && regeneratePassword) {
                      await handleRegenerateRecoveryCodes()
                    }
                  }}
                />
              </div>

              <div className="flex gap-2">
                <Button
                  onClick={handleRegenerateRecoveryCodes}
                  disabled={!regeneratePassword || isLoading}
                  variant="default"
                >
                  <RefreshCw className="h-4 w-4 mr-2" /> Generate New Codes
                </Button>
                <Button
                  onClick={handleCancelRegenerate}
                  variant="outline"
                  disabled={isLoading}
                >
                  Cancel
                </Button>
              </div>
            </>
          ) : (
            <>
              <div className="p-4 bg-orange-50 dark:bg-orange-950/20 border border-orange-200 dark:border-orange-900 rounded space-y-2">
                <div className="flex items-center justify-between mb-2">
                  <Label className="text-xs font-medium text-orange-600 dark:text-orange-400">
                    Your New Recovery Codes
                  </Label>
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() =>
                      downloadRecoveryCodes(
                        regenerateRecoveryCodesMutation.data.data.recoveryCodes,
                      )
                    }
                  >
                    <Download className="h-3 w-3 mr-1" /> Download
                  </Button>
                </div>
                <p className="text-xs text-orange-800 dark:text-orange-200 mb-2">
                  ⚠️ Save these codes now! Your old recovery codes no longer
                  work.
                </p>
                <div className="p-3 bg-white dark:bg-background border border-border rounded space-y-1 max-h-48 overflow-y-auto">
                  {regenerateRecoveryCodesMutation.data.data.recoveryCodes.map(
                    (code, idx) => (
                      <code key={idx} className="block text-xs font-mono">
                        {idx + 1}. {code}
                      </code>
                    ),
                  )}
                </div>
              </div>

              <Button
                onClick={handleCancelRegenerate}
                variant="outline"
                className="w-full"
              >
                Done
              </Button>
            </>
          )}
        </div>
      )}
    </section>
  )
}
