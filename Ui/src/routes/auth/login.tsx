import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useLogin } from '@/features/admin/auth/useAdminAuth.ts'
import { useForm } from '@tanstack/react-form'
import {
  LoginRequest,
  loginRequestSchema,
} from '@/features/admin/auth/adminAuthTypes.ts'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input.tsx'
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLegend,
  FieldSet,
} from '@/components/ui/field.tsx'
import { Label } from '@/components/ui/label.tsx'
import { ApiErrorsMessage } from '@/components/ApiErrorsMessage.tsx'

export const Route = createFileRoute('/auth/login')({ component: Login })

function Login() {
  const navigate = useNavigate()
  const { mutate: login, isPending, error } = useLogin()
  const apiErrors = error?.response?.data?.error

  const form = useForm({
    defaultValues: {
      email: '',
      password: '',
    } as LoginRequest,
    onSubmit: async ({ value }) => {
      login(value, {
        onSuccess: (response) => {
          const requireTwoFactor = response.data.requireTwoFactor
          requireTwoFactor
            ? navigate({ to: '/auth/2fa' })
            : navigate({ to: '/' })
        },
      })
    },
  })

  return (
    <>
      <form
        onSubmit={async (e) => {
          e.preventDefault()
          await form.handleSubmit()
        }}
        className="w-full max-w-sm space-y-4"
      >
        <FieldSet>
          <FieldLegend>Login</FieldLegend>
          <FieldDescription>Welcome to Ibrahabra IAM</FieldDescription>

          <ApiErrorsMessage apiErrors={apiErrors} />
          {/* Email Field */}
          <form.Field
            name="email"
            validators={{ onChange: loginRequestSchema.shape.email }}
          >
            {({ name, state, handleChange }) => (
              <Field>
                <Label htmlFor={name}>Email</Label>
                <Input
                  id={name}
                  type="email"
                  value={state.value}
                  onChange={(e) => handleChange(e.target.value)}
                />
                {state.meta.errors.length > 0 && (
                  <FieldError>{state.meta.errors[0]?.message}</FieldError>
                )}
              </Field>
            )}
          </form.Field>

          {/* Password Field */}
          <form.Field
            name="password"
            validators={{ onChange: loginRequestSchema.shape.password }}
          >
            {({ name, state, handleChange }) => (
              <Field>
                <Label htmlFor={name}>Password</Label>
                <Input
                  id={name}
                  type="password"
                  value={state.value}
                  onChange={(e) => handleChange(e.target.value)}
                />
                {state.meta.errors.length > 0 && (
                  <FieldError>{state.meta.errors[0]?.message}</FieldError>
                )}
              </Field>
            )}
          </form.Field>

          <Button type="submit" disabled={isPending} className="w-full mt-4">
            {isPending ? 'Logging in...' : 'Login'}
          </Button>
        </FieldSet>
      </form>
    </>
  )
}
