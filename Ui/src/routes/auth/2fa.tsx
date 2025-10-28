import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useVerify2Fa } from '@/features/admin/auth/useAdminAuth.ts'
import { useForm } from '@tanstack/react-form'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import {
  adminAuthStore,
  adminAuthStoreAction,
} from '@/stores/adminAuthStore.ts'
import { ApiErrorsMessage } from '@/components/ApiErrorsMessage.tsx'

export const Route = createFileRoute('/auth/2fa')({
  component: TwoFactorPage,
})

function TwoFactorPage() {
  const navigate = useNavigate()
  const { mutate: verify2fa, isPending, error } = useVerify2Fa()
  const apiErrors = error?.response?.data?.error

  const sessionCode = adminAuthStore.state.sessionCode2Fa
  if (!sessionCode) {
    navigate({ to: '/auth/login' }).then((r) => r)
    return null
  }

  const form = useForm({
    defaultValues: {
      code: '',
    },
    onSubmit: async ({ value }) => {
      verify2fa(
        {
          twoFactorCode: sessionCode,
          code: value.code,
        },
        {
          onSuccess: () => {
            adminAuthStoreAction.set2FaSessionCode(null)
            navigate({ to: '/' })
          },
        },
      )
    },
  })

  return (
    <form
      onSubmit={async (e) => {
        e.preventDefault()
        await form.handleSubmit()
      }}
      className="w-full max-w-sm space-y-4"
    >
      <h1 className="text-lg font-semibold">Two-Factor Authentication</h1>
      <p className="text-sm text-muted-foreground">
        Enter the 6-digit code from your authenticator app.
      </p>

      <ApiErrorsMessage apiErrors={apiErrors} />

      <form.Field name="code">
        {({ name, state, handleChange }) => (
          <div className="flex flex-col gap-2">
            <Input
              id={name}
              type="text"
              inputMode="numeric"
              pattern="\d*"
              maxLength={6}
              placeholder="123456"
              value={state.value}
              onChange={(e) => handleChange(e.target.value)}
            />
          </div>
        )}
      </form.Field>

      <Button type="submit" disabled={isPending} className="w-full mt-4">
        {isPending ? 'Verifying...' : 'Verify'}
      </Button>
    </form>
  )
}
