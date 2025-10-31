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
adminAuthStore.subscribe((value) => {
  const { prevVal, currentVal } = value

  if (
    prevVal.user !== currentVal.user ||
    prevVal.token !== currentVal.token ||
    prevVal.expiresAt !== currentVal.expiresAt ||
    prevVal.sessionCode2Fa !== currentVal.sessionCode2Fa
  ) {
    const { loading, ...persisted } = currentVal
    localStorageUtils.set(localStorageKeys.auth, persisted)
  }
})

export const adminAuthStoreAction = {
  setLoading: (loading: boolean) =>
    adminAuthStore.setState((prev) => ({ ...prev, loading })),

  setAuth: (user: AdminUser, token: string, expiresAt: string) =>
    adminAuthStore.setState((prev) => ({
      ...prev,
      user,
      token,
      expiresAt,
      sessionCode2Fa: null,
    })),
  setToken: (token: string) =>
    adminAuthStore.setState((prev) => ({ ...prev, token: token })),

  set2FaSessionCode: (session: string | null) =>
    adminAuthStore.setState((prev) => ({ ...prev, sessionCode2Fa: session })),
  reset: () =>
    adminAuthStore.setState({
      user: null,
      token: null,
      expiresAt: null,
      sessionCode2Fa: null,
      loading: false,
    }),

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
}
