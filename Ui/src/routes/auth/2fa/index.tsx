import { createFileRoute, redirect, useNavigate } from '@tanstack/react-router'
import { useVerify2Fa } from '@/features/admin/auth/useAdminAuth.ts'
import { useForm } from '@tanstack/react-form'
import { Button } from '@/components/ui/button'
import {
  adminAuthStore,
  adminAuthStoreAction,
} from '@/stores/adminAuthStore.ts'
import { ApiErrorsMessage } from '@/components/ApiErrorsMessage.tsx'
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from '@/components/ui/input-otp.tsx'
import { Shield } from 'lucide-react'
import { TwoFactorSkeleton } from '@/features/admin/auth/components/TwoFactorSkeleton.tsx'

export const Route = createFileRoute('/auth/2fa/')({
  component: TwoFactorPage,
  pendingComponent: TwoFactorSkeleton,
  ssr: false,
  beforeLoad: () => {
    const { token, sessionCode2Fa } = adminAuthStore.state

    if (token) {
      throw redirect({ to: '/' })
    }
    if (!sessionCode2Fa) {
      throw redirect({ to: '/auth/login' })
    }
  },
})

function TwoFactorPage() {
  const navigate = useNavigate()
  const { mutate: verify2fa, isPending, error } = useVerify2Fa()
  const apiErrors = error?.response?.data?.error

  const sessionCode = adminAuthStore.state.sessionCode2Fa

  const form = useForm({
    defaultValues: {
      code: '',
    },
    onSubmit: async ({ value }) => {
      verify2fa(
        { session2Fa: sessionCode!, code: value.code },
        {
          onSuccess: () => {
            navigate({ to: '/' })
          },
        },
      )
    },
  })

  return (
    <div className="w-full max-w-md relative z-10">
      {/* Header */}
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 rounded-lg bg-primary/20 border-glow-info mb-4">
          <Shield className="w-8 h-8 text-primary" />
        </div>
        <h1 className="text-3xl font-bold text-foreground mb-2">
          TWO-FACTOR AUTH
        </h1>
        <p className="text-muted-foreground">Additional Security Required</p>
      </div>

      {/* 2FA Form Card */}
      <div className="bg-card border border-border rounded-lg p-8 shadow-2xl backdrop-blur-sm border-glow-cyan">
        <div className="text-center mb-6">
          <h2 className="text-xl font-semibold mb-2">Verify Your Identity</h2>
          <p className="text-sm text-muted-foreground">
            Enter the 6-digit code from your authenticator app
          </p>
        </div>

        <form
          onSubmit={async (e) => {
            e.preventDefault()
            await form.handleSubmit()
          }}
          className="space-y-6"
        >
          <ApiErrorsMessage apiErrors={apiErrors} />

          <form.Field name="code">
            {({ state, handleChange }) => (
              <div className="flex flex-col items-center gap-2">
                <InputOTP
                  maxLength={6}
                  value={state.value}
                  onChange={(value) => handleChange(value)}
                  disabled={isPending}
                >
                  <InputOTPGroup>
                    {[0, 1, 2, 3, 4, 5].map((i) => (
                      <InputOTPSlot key={i} index={i} />
                    ))}
                  </InputOTPGroup>
                </InputOTP>
              </div>
            )}
          </form.Field>

          <Button
            type="submit"
            disabled={isPending}
            className="w-full bg-primary hover:bg-primary/90 text-primary-foreground font-semibold py-6 glow-info transition-all"
          >
            {isPending ? (
              <span className="flex items-center justify-center gap-2">
                <span className="w-4 h-4 border-2 border-primary-foreground/30 border-t-primary-foreground rounded-full animate-spin" />
                Verifying...
              </span>
            ) : (
              'VERIFY CODE'
            )}
          </Button>
        </form>

        <button
          type="button"
          onClick={async () => {
            adminAuthStoreAction.set2FaSessionCode(null)
            await navigate({ to: '/auth/login' })
          }}
          className="w-full text-sm text-muted-foreground hover:text-foreground transition-colors mt-4"
        >
          ← Back to Login
        </button>
      </div>

      {/* Footer */}
      <div className="text-center mt-6 text-sm text-muted-foreground">
        <p>Secured by Ibrahabra Security Systems</p>
      </div>
    </div>
  )
}
