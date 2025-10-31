import {
  createFileRoute as create2FaRoute,
  useNavigate as useNav2Fa,
} from '@tanstack/react-router'
import { useVerify2Fa } from '@/features/admin/auth/useAdminAuth.ts'
import { useForm as use2FaForm } from '@tanstack/react-form'
import { Button as Button2Fa } from '@/components/ui/button'
import {
  adminAuthStore,
  adminAuthStoreAction,
} from '@/stores/adminAuthStore.ts'
import { ApiErrorsMessage as ApiErrors2Fa } from '@/components/ApiErrorsMessage.tsx'
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from '@/components/ui/input-otp.tsx'

export const Route = create2FaRoute('/auth/2fa')({
  component: TwoFactorPage,
})

function TwoFactorPage() {
  const navigate = useNav2Fa()
  const { mutate: verify2fa, isPending, error } = useVerify2Fa()
  const apiErrors = error?.response?.data?.error

  const sessionCode = adminAuthStore.state.sessionCode2Fa
  if (!sessionCode) {
    navigate({ to: '/auth/login' }).then((r) => r)
    return null
  }

  const form = use2FaForm({
    defaultValues: {
      code: '',
    },
    onSubmit: async ({ value }) => {
      verify2fa(
        { twoFactorCode: sessionCode, code: value.code },
        {
          onSuccess: () => {
            navigate({ to: '/' })
          },
        },
      )
    },
  })

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <div className="w-full max-w-md bg-card border border-border rounded-lg p-8 shadow-xl">
        <h1 className="text-2xl font-bold text-center mb-4">
          Two-Factor Authentication
        </h1>
        <p className="text-center text-muted-foreground mb-6">
          Enter the 6-digit code from your authenticator app
        </p>

        <form
          onSubmit={async (e) => {
            e.preventDefault()
            await form.handleSubmit()
          }}
          className="space-y-6"
        >
          <ApiErrors2Fa apiErrors={apiErrors} />

          <form.Field name="code">
            {({ state, handleChange }) => (
              <div className="flex flex-col items-center gap-2">
                <InputOTP
                  maxLength={6}
                  value={state.value}
                  onChange={(value) => handleChange(value)}
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

          <Button2Fa
            type="submit"
            disabled={isPending}
            className="w-full bg-accent hover:bg-accent/90 text-accent-foreground font-semibold py-6"
          >
            {isPending ? 'Verifying…' : 'Verify Code'}
          </Button2Fa>
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
    </div>
  )
}
