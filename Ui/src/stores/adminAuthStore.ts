import { Store } from '@tanstack/store'
import { localStorageKeys } from '@/constants/localStorageConstant.ts'
import { cookieUtils } from '@/lib/cookieStorageUtils.ts'

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

let isRehydrating = false
adminAuthStore.subscribe((value) => {
  if (isRehydrating) return
  const { prevVal, currentVal } = value

  if (
    prevVal.user !== currentVal.user ||
    prevVal.token !== currentVal.token ||
    prevVal.expiresAt !== currentVal.expiresAt ||
    prevVal.sessionCode2Fa !== currentVal.sessionCode2Fa
  ) {
    const { loading, ...persisted } = currentVal
    cookieUtils.set(localStorageKeys.auth, JSON.stringify(persisted))
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
    const saved = cookieUtils.get(localStorageKeys.auth)
    if (saved) {
      try {
        const parsed = JSON.parse(saved)
        isRehydrating = true
        adminAuthStore.setState({ ...parsed, loading: false })
        isRehydrating = false
      } catch {
        cookieUtils.remove(localStorageKeys.auth)
      }
    }
  },
}
