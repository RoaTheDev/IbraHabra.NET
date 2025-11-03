import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { useVerifyRecoveryCode } from '@/features/admin/auth/useAdminAuth'
import { useForm } from '@tanstack/react-form'
import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { adminAuthStore } from '@/stores/adminAuthStore.ts'
import { toast } from 'sonner'

export const Route = createFileRoute('/auth/2fa/recovery')({
  component: RecoveryCodePage,
})

function RecoveryCodePage() {
  const navigate = useNavigate()
  const { mutate: verifyRecovery } = useVerifyRecoveryCode()
  const session2Fa = adminAuthStore.state.sessionCode2Fa
  const form = useForm({
    defaultValues: { recoveryCode: '' },
    onSubmit: async ({ value }) => {
      verifyRecovery(
        {
          session2Fa: session2Fa!,
          recoveryCode: value.recoveryCode,
        },
        {
          onSuccess: (data) => {
            const res = data.data
            if (res.remainingRecoveryCodes <= 3) {
              toast.warning(
                `Only ${res.remainingRecoveryCodes} recovery codes left!`,
              )
            }
            navigate({ to: '/' })
          },
        },
      )
    },
  })

  return (
    <div>
      <h1>Use Recovery Code</h1>
      <p>Enter one of your backup recovery codes</p>

      <Input
        type="text"
        placeholder="ABCD-EFGH-IJKL-MNOP"
        className="font-mono"
        {...form}
      />

      <Button type="submit">Verify Recovery Code</Button>

      <button onClick={() => navigate({ to: '/auth/2fa' })}>
        ← Back to authenticator code
      </button>
    </div>
  )
}
