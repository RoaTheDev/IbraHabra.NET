import { useMutation, useQuery } from '@tanstack/react-query'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'
import {
  AdminAuthPersisted,
  adminAuthStore,
  adminAuthStoreAction,
  AdminUser,
} from '@/stores/adminAuthStore.ts'
import { localStorageUtils } from '@/lib/localStorageUtils.ts'
import {
  LoginRequest,
  LoginResponse,
  Verify2FaAdminRequest,
  Verify2FaAdminResponse,
} from '@/features/admin/auth/adminAuthTypes.ts'
import { cacheKeys } from '@/constants/cacheKeys.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'
import { AxiosError } from 'axios'

const AdminAuthQueryKeys = {
  base: ['auth'] as const,
  register: () => [...AdminAuthQueryKeys.base, 'register'] as const,
  login: () => [...AdminAuthQueryKeys.base, 'login'] as const,
  me: () => [...AdminAuthQueryKeys.base, 'me'] as const,
  twoFactor: {
    base: () => [...AdminAuthQueryKeys.base, '2fa'] as const,
    status: () => [...AdminAuthQueryKeys.twoFactor.base(), 'status'] as const,
    verify: () => [...AdminAuthQueryKeys.twoFactor.base(), 'verify'] as const,
    enable: () => [...AdminAuthQueryKeys.twoFactor.base(), 'enable'] as const,
    disable: () => [...AdminAuthQueryKeys.twoFactor.base(), 'disable'] as const,
  },
}
export const useLogin = () =>
  useMutation<
    ApiResponse<LoginResponse>,
    AxiosError<ApiResponse<unknown>>,
    LoginRequest
  >({
    mutationKey: AdminAuthQueryKeys.login(),
    mutationFn: authApi.login,
    onMutate: () => adminAuthStoreAction.setLoading(true),
    onSuccess: (response) => {
      const { data } = response
      if (data.requiresTwoFactor) {
        adminAuthStoreAction.set2FaSessionCode(data.session2Fa)
      } else {
        const user: AdminUser = {
          userId: data.userId,
          requiresTwoFactor: data.requiresTwoFactor,
          email: data.email,
        }
        adminAuthStoreAction.setAuth(user, data.token, data.expiresAt)
      }
    },
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useVerify2Fa = () =>
  useMutation<
    ApiResponse<Verify2FaAdminResponse>,
    AxiosError<ApiResponse<unknown>>,
    Verify2FaAdminRequest
  >({
    mutationKey: AdminAuthQueryKeys.twoFactor.base(),
    mutationFn: authApi.verify2fa,
    onMutate: () => adminAuthStoreAction.setLoading(true),
    onSuccess: (response) => {
      const { data } = response
      const userData: AdminUser = {
        userId: data.userId,
        email: data.email,
        requiresTwoFactor: true,
      }

      adminAuthStoreAction.setAuth(userData, data.token, data.expiresAt)
      const { user, token, expiresAt, sessionCode2Fa } = adminAuthStore.state
      const persisted: AdminAuthPersisted = {
        user,
        token,
        expiresAt,
        sessionCode2Fa,
      }
      localStorageUtils.set(cacheKeys.auth, persisted)
    },
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useAuthUser = () =>
  useQuery({
    queryKey: AdminAuthQueryKeys.me(),
    queryFn: async () => {
      const response = await authApi.me()
      return response.data
    },
    enabled: false,
  })
