import { Store } from '@tanstack/store'
import { localStorageUtils } from '@/lib/localStorageUtils.ts'
import { localStorageKeys } from '@/constants/localStorageConstant.ts'

export type AdminUser = {
  userId: string
  email: string
  requiresTwoFactor: boolean
}

export type AdminAuthPersisted = {
  user: AdminUser | null
  token: string | null
  expiresAt: string | null
  sessionCode2Fa?: string | null
}

export type AdminAuthState = {
  user: AdminUser | null
  token: string | null
  expiresAt: string | null
  sessionCode2Fa?: string | null
  loading: boolean
}

export const adminAuthStore = new Store<AdminAuthState>({
  user: null,
  token: null,
  sessionCode2Fa: null,
  loading: false,
  expiresAt: null,
})

export const adminAuthStoreAction = {
  setLoading: (loading: boolean) =>
    adminAuthStore.setState((prev) => ({ ...prev, loading })),
  setUser: (user: AdminUser) =>
    adminAuthStore.setState((prev) => ({ ...prev, user })),
  setExpireAt: (expiresAt: string) =>
    adminAuthStore.setState((prev) => ({ ...prev, expiresAt })),
  setToken: (token: string) =>
    adminAuthStore.setState((prev) => ({ ...prev, token })),
  set2FaSessionCode: (session: string | null) =>
    adminAuthStore.setState((prev) => ({ ...prev, sessionCode2Fa: session })),
  rehydrate: () => {
    const saved = localStorageUtils.get<AdminAuthPersisted>(
      localStorageKeys.auth,
    )
    if (saved) {
      adminAuthStore.setState({
        ...saved,
        loading: false,
      })
    }
  },
  reset: () =>
    adminAuthStore.setState({
      user: null,
      token: null,
      expiresAt: null,
      sessionCode2Fa: null,
      loading: false,
    }),
}
