import { createFileRoute, useNavigate } from '@tanstack/react-router'
import { useLogin } from '@/features/admin/auth/useAdminAuth.ts'
import { useForm } from '@tanstack/react-form'
import { LoginRequest, loginRequestSchema } from '@/features/admin/auth/adminAuthTypes.ts'
import { Lock, Shield } from 'lucide-react'
import { Field, FieldDescription, FieldError, FieldLegend, FieldSet } from '@/components/ui/field.tsx'
import { ApiErrorsMessage } from '@/components/ApiErrorsMessage.tsx'
import { Label } from '@/components/ui/label.tsx'
import { Input } from '@/components/ui/input.tsx'
import { Button } from '@/components/ui/button.tsx'

export const Route = createFileRoute('/auth/login')({
  component: Login,
})

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
          const requireTwoFactor = response.data.requiresTwoFactor
          requireTwoFactor
            ? navigate({ to: '/auth/2fa' })
            : navigate({ to: '/' })
        },
      })
    },
  })

  return (

      <div className="w-full max-w-md relative z-10">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-lg bg-primary/20 border-glow-info mb-4">
            <Shield className="w-8 h-8 text-primary" />
          </div>
          <h1 className="text-3xl font-bold text-foreground mb-2">SECURE ACCESS</h1>
          <p className="text-muted-foreground">Identity & Access Management</p>
        </div>

        {/* Login Form Card */}
        <div className="bg-card border border-border rounded-lg p-8 shadow-2xl backdrop-blur-sm border-glow-cyan">
          <form
            onSubmit={async (e) => {
              e.preventDefault()
              await form.handleSubmit()
            }}
            className="space-y-6"
          >
            <FieldSet>
              <FieldLegend className="text-xl mb-2">Login</FieldLegend>
              <FieldDescription className="text-muted-foreground mb-6">
                Welcome to Ibrahabra IAM
              </FieldDescription>

              <ApiErrorsMessage apiErrors={apiErrors} />

              {/* Email Field */}
              <form.Field
                name="email"
                validators={{ onChange: loginRequestSchema.shape.email }}
              >
                {({ name, state, handleChange }) => (
                  <Field className="mb-4">
                    <Label htmlFor={name} className="text-foreground mb-2 block">
                      Email Address
                    </Label>
                    <Input
                      id={name}
                      type="email"
                      value={state.value}
                      onChange={(e) => handleChange(e.target.value)}
                      placeholder="admin@ibrahabra.com"
                      className="w-full bg-input border-border focus:border-primary focus:ring-primary/50 transition-all"
                    />
                    {state.meta.errors.length > 0 && (
                      <FieldError className="text-destructive text-sm mt-1">
                        {state.meta.errors[0]?.message}
                      </FieldError>
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
                  <Field className="mb-6">
                    <Label htmlFor={name} className="text-foreground mb-2 block">
                      Password
                    </Label>
                    <div className="relative">
                      <Lock className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                      <Input
                        id={name}
                        type="password"
                        value={state.value}
                        onChange={(e) => handleChange(e.target.value)}
                        placeholder="••••••••"
                        className="w-full pl-10 bg-input border-border focus:border-primary focus:ring-primary/50 transition-all"
                      />
                    </div>
                    {state.meta.errors.length > 0 && (
                      <FieldError className="text-destructive text-sm mt-1">
                        {state.meta.errors[0]?.message}
                      </FieldError>
                    )}
                  </Field>
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
                    Authenticating...
                  </span>
                ) : (
                  'LOGIN'
                )}
              </Button>
            </FieldSet>
          </form>
        </div>

        {/* Footer */}
        <div className="text-center mt-6 text-sm text-muted-foreground">
          <p>Secured by Ibrahabra Security Systems</p>
        </div>
      </div>
  )
}
