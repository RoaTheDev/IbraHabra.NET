import {Store} from '@tanstack/store'

export type AdminUser = {
    userId: string
    email: string
    requiresTwoFactor: boolean

}

export type AdminAuthState = {
    user: AdminUser | null
    token: string | null
    expiresAt: string | null
    twoFactorToken?: string | null
    loading: boolean
}

export const adminAuthStore = new Store<AdminAuthState>({
    user: null,
    token: null,
    twoFactorToken: null,
    loading: false,
    expiresAt: null,
})


export const adminAuthStoreAction = {
    setLoading: (loading: boolean) => adminAuthStore.setState(prev => ({...prev, loading})),
    setUser: (user: AdminUser) => adminAuthStore.setState(prev => ({...prev, user})),
    setExpireAt: (expiresAt: string) => adminAuthStore.setState(prev => ({...prev, expiresAt})),
    setToken: (token: string) => adminAuthStore.setState(prev => ({...prev, token})),
    
    reset: () =>
        adminAuthStore.setState(prev => ({
            ...prev,
            user: null,
            loading: false,
            error: null,
            token: null,
        })),
}