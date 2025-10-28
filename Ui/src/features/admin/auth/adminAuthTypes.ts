import { z } from 'zod'

export const loginRequestSchema = z.object({
  email: z.email('email is invalid').min(1, 'Email is required'),
  password: z
    .string()
    .refine(
      (val) => /[A-Z]/.test(val),
      'Password must have at least one uppercase letter',
    )
    .refine(
      (val) => /[a-z]/.test(val),
      'Password must have at least one lowercase letter',
    )
    .min(8, 'Password must be at least 8 characters'),
})

export type LoginRequest = z.infer<typeof loginRequestSchema>

export type LoginResponse = {
  userId: string
  email: string
  token: string
  expiresAt: string
  requiresTwoFactor: boolean
  session2Fa: string | null
}

export type RefreshTokenRequest = {
  token: string
}
export type RefreshTokenResponse = {
  token: string
  ExpiredAt: Date
}

export type Verify2FaAdminRequest = {
  twoFactorCode: string
  code: string
}
export type Verify2FaAdminResponse = {
  userId: string
  email: string
  token: string
  expiresAt: string
}
export type Enable2FaAdminResponse = {
  sharedKey: string
  authenticatorUri: string
  recoveryCodes: string[]
}
export type Disable2FaAdminRequest = {
  password: string
}
export type Disable2FaAdminResponse = {
  success: boolean
  message: string
}
export type ConfirmEnable2FaAdminRequest = {
  code: string
}

export type ConfirmEnable2FaAdminResponse = {
  success: boolean
  message: string
}

export type AdminUserInfoResponse = {
  userId: string
  email: string
  firstName?: string
  lastName?: string
  roles: string[]
  emailConfirmed: boolean
  twoFactorEnabled: boolean
  createdAt: Date
}

export type VerifyAdminUserResponse = {
   valid: boolean
  message: string
}