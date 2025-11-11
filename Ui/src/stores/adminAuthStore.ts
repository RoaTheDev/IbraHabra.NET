import { Store } from '@tanstack/store'
import { localStorageUtils } from '@/lib/localStorageUtils'
import { clientCacheKeys } from '@/constants/clientCacheKeys.ts'

export type AdminUser = {
  userId: string
  email: string
  requiresTwoFactor: boolean
}

export type AdminAuthPersisted = {
  user: AdminUser | null
  sessionCode2Fa: string | null
}

export type AdminAuthState = {
  user: AdminUser | null
  sessionCode2Fa: string | null
  loading: boolean
  isAuthenticated: boolean
}


export const adminAuthStore = new Store<AdminAuthState>({
  user: null,
  sessionCode2Fa: null,
  loading: false,
  isAuthenticated: false,
})

let isRehydrating = false
adminAuthStore.subscribe((value) => {
  if (isRehydrating) return

  const { currentVal } = value
  const persisted: AdminAuthPersisted = {
    user: currentVal.user,
    sessionCode2Fa: currentVal.sessionCode2Fa,
  }

  if (currentVal.user) {
    localStorageUtils.set<AdminAuthPersisted>(clientCacheKeys.user_meta, persisted)
  } else {
    localStorageUtils.remove(clientCacheKeys.user_meta)
  }
})

export const adminAuthStoreAction = {
  setLoading: (loading: boolean) =>
    adminAuthStore.setState((prev) => ({ ...prev, loading })),

  setAuth: (user: AdminUser, sessionCode2Fa: string | null) => {
    adminAuthStore.setState({
      user,
      sessionCode2Fa,
      loading: false,
      isAuthenticated: true,
    })
  },

  set2FaSessionCode: (session: string | null) =>
    adminAuthStore.setState((prev) => ({
      ...prev,
      sessionCode2Fa: session,
    })),

  reset: () => {
    adminAuthStore.setState({
      user: null,
      sessionCode2Fa: null,
      loading: false,
      isAuthenticated: false,
    })
    localStorageUtils.remove(clientCacheKeys.user_meta)
  },

  rehydrate: () => {
    const saved = localStorageUtils.get<AdminAuthPersisted>(clientCacheKeys.user_meta)
    if (saved) {
      try {
        isRehydrating = true
        adminAuthStore.setState({
          ...saved,
          loading: false,
          isAuthenticated: saved.user !== null,
        })
      } finally {
        isRehydrating = false
      }
    }
  },

}