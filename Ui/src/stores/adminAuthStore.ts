import { Store } from '@tanstack/store'

export type AdminUser = {
  userId: string
  email: string
}

export type AdminAuthState = {
  user: AdminUser | null
  token: string | null
  expiresAt: string | null
  loading: boolean
  requiresTwoFactor: boolean
  twoFactorToken?: string | null
  error?: string | null
}

export const adminAuthStore = new Store<AdminAuthState>({
  user: null,
  token: null,
  expiresAt: null,
  loading: false,
  requiresTwoFactor: false,
  twoFactorToken: null,
})
