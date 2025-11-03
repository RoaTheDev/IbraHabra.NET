import { useState } from 'react'
import {
  AlertTriangle,
  Check,
  CheckCircle2,
  Copy,
  Lock,
  Smartphone,
  XCircle,
} from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { AdminUserInfoResponse } from '@/features/admin/auth/adminAuthTypes.ts'
import {
  useConfirm2Fa,
  useDisable2Fa,
  useEnable2Fa,
} from '@/features/admin/auth/useAdminAuth'
import { toast } from 'sonner'

export function TwoFactorSection({
  userInfo,
}: {
  userInfo: AdminUserInfoResponse | undefined | null
}) {
  const [show2FASetup, setShow2FASetup] = useState(false)
  const [verificationCode, setVerificationCode] = useState('')
  const [disableCode, setDisableCode] = useState('')
  const [showDisable2FA, setShowDisable2FA] = useState(false)
  const [copiedSecret, setCopiedSecret] = useState(false)

  const enable2FaMutation = useEnable2Fa()
  const confirm2FaMutation = useConfirm2Fa()
  const disable2FaMutation = useDisable2Fa()

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
    if (disableCode.length !== 6) {
      toast.error('Please enter a 6-digit code')
      return
    }

    disable2FaMutation.mutate(
      { code: disableCode },
      {
        onSuccess: (response) => {
          toast.success(response.data.message || '2FA disabled successfully')
          setShowDisable2FA(false)
          setDisableCode('')
        },
        onError: (error) => {
          const errorMessage =
            error.response?.data?.error?.[0]?.message || 'Failed to disable 2FA'
          toast.error(errorMessage)
        },
      },
    )
  }

  const copyToClipboard = async (text: string) => {
    await navigator.clipboard.writeText(text)
    setCopiedSecret(true)
    setTimeout(() => setCopiedSecret(false), 2000)
  }

  const isLoading =
    enable2FaMutation.isPending ||
    confirm2FaMutation.isPending ||
    disable2FaMutation.isPending

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

      {/* Enable / Disable Buttons */}
      {!userInfo?.twoFactorEnabled && !show2FASetup && (
        <Button onClick={handleEnable2FA} disabled={isLoading}>
          <Lock className="h-4 w-4 mr-2" /> Enable 2FA
        </Button>
      )}

      {userInfo?.twoFactorEnabled && !showDisable2FA && (
        <Button
          onClick={() => setShowDisable2FA(true)}
          variant="outline"
          className="border-destructive/50 text-destructive hover:bg-destructive/10"
        >
          <XCircle className="h-4 w-4 mr-2" /> Disable 2FA
        </Button>
      )}

      {/* Setup QR flow */}
      {show2FASetup && enable2FaMutation.data?.data && (
        <div className="mt-4 p-4 bg-muted/30 border border-primary/30 rounded-lg space-y-4">
          <div>
            <h3 className="font-semibold text-sm mb-1">Scan QR Code</h3>
            <img
              src={enable2FaMutation.data.data.authenticatorUri}
              alt="2FA QR Code"
              className="w-48 h-48 rounded"
            />
          </div>

          <div>
            <Label className="text-xs font-medium mb-2 block">
              Manual Entry Key
            </Label>
            <div className="flex items-center gap-2">
              <code className="flex-1 px-3 py-2 bg-muted border border-border rounded text-xs font-mono break-all">
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

          {/* Recovery Codes */}
          {enable2FaMutation.data.data.recoveryCodes &&
            enable2FaMutation.data.data.recoveryCodes.length > 0 && (
              <div>
                <Label className="text-xs font-medium mb-2 block text-orange-600">
                  Recovery Codes (Save these securely!)
                </Label>
                <div className="p-3 bg-muted border border-border rounded space-y-1">
                  {enable2FaMutation.data.data.recoveryCodes.map(
                    (code, idx) => (
                      <code key={idx} className="block text-xs font-mono">
                        {code}
                      </code>
                    ),
                  )}
                </div>
              </div>
            )}

          <div>
            <Label htmlFor="verification-code">Verification Code</Label>
            <Input
              id="verification-code"
              placeholder="000000"
              value={verificationCode}
              onChange={(e) =>
                setVerificationCode(
                  e.target.value.replace(/\D/g, '').slice(0, 6),
                )
              }
              className="font-mono text-center text-lg tracking-widest"
            />
          </div>

          <div className="flex gap-2">
            <Button
              onClick={handleConfirm2FA}
              disabled={verificationCode.length !== 6 || isLoading}
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
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      {/* Disable 2FA Flow */}
      {showDisable2FA && (
        <div className="mt-4 p-4 bg-destructive/5 border border-destructive/30 rounded-lg space-y-4">
          <Label htmlFor="disable-code">Verification Code</Label>
          <Input
            id="disable-code"
            type="text"
            placeholder="000000"
            value={disableCode}
            onChange={(e) =>
              setDisableCode(e.target.value.replace(/\D/g, '').slice(0, 6))
            }
            className="font-mono text-center text-lg tracking-widest"
          />

          <div className="flex gap-2">
            <Button
              onClick={handleDisable2FA}
              disabled={disableCode.length !== 6 || isLoading}
              variant="outline"
              className="border-destructive/50 text-destructive hover:bg-destructive/10"
            >
              <XCircle className="h-4 w-4 mr-2" /> Disable 2FA
            </Button>
            <Button
              onClick={() => {
                setShowDisable2FA(false)
                setDisableCode('')
              }}
              variant="outline"
              disabled={isLoading}
            >
              Cancel
            </Button>
          </div>
        </div>
      )}
    </section>
  )
}
