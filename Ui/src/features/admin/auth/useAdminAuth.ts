import { useMutation, useQuery } from '@tanstack/react-query'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'
import { adminAuthStoreAction, AdminUser } from '@/stores/adminAuthStore.ts'
import { localStorageUtils } from '@/lib/localStorageUtils.ts'
import {
  LoginRequest,
  LoginResponse,
} from '@/features/admin/auth/adminAuthTypes.ts'
import { localStorageKeys } from '@/constants/localStorageConstant.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'
import { AxiosError } from 'axios'

const AdminAuthQueryKeys = {
  base: ['auth'] as const,
  register: () => [...AdminAuthQueryKeys.base, 'register'] as const,
  login: () => [...AdminAuthQueryKeys.base, 'login'] as const,
  me: () => [...AdminAuthQueryKeys.base, 'me'] as const,
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
      const user: AdminUser = {
        userId: data.userId,
        requiresTwoFactor: data.requiresTwoFactor,
        email: data.email,
      }
      adminAuthStoreAction.setUser(user)
      adminAuthStoreAction.setToken(data.token)
      adminAuthStoreAction.setExpireAt(data.expiresAt)
      localStorageUtils.set<LoginResponse>(localStorageKeys.auth, data)
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
