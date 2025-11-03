import { useMutation, useQuery } from '@tanstack/react-query'
import { authApi } from '@/features/admin/auth/adminAuthApi.ts'
import {
  adminAuthStore,
  adminAuthStoreAction,
  AdminUser,
} from '@/stores/adminAuthStore.ts'
import {
  AdminUserInfoResponse,
  ConfirmEnable2FaAdminRequest,
  ConfirmEnable2FaAdminResponse,
  Disable2FaAdminRequest,
  Disable2FaAdminResponse,
  Enable2FaAdminResponse,
  LoginRequest,
  LoginResponse,
  Verify2FaAdminRequest,
  Verify2FaAdminResponse,
} from '@/features/admin/auth/adminAuthTypes.ts'
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
    AxiosError<ApiResponse<null>>,
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
      adminAuthStoreAction.setAuth(
        user,
        data.token,
        data.expiresAt,
        data.session2Fa,
      )
    },
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useVerify2Fa = () =>
  useMutation<
    ApiResponse<Verify2FaAdminResponse>,
    AxiosError<ApiResponse<null>>,
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
      adminAuthStoreAction.setAuth(userData, data.token, data.expiresAt, null)
    },
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useEnable2Fa = () =>
  useMutation<
    ApiResponse<Enable2FaAdminResponse>,
    AxiosError<ApiResponse<null>>
  >({
    mutationKey: AdminAuthQueryKeys.twoFactor.enable(),
    mutationFn: authApi.enable2fa,
    onMutate: () => adminAuthStoreAction.setLoading(true),
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })
export const useConfirm2Fa = () =>
  useMutation<
    ApiResponse<ConfirmEnable2FaAdminResponse>,
    AxiosError<ApiResponse<null>>,
    ConfirmEnable2FaAdminRequest
  >({
    mutationKey: AdminAuthQueryKeys.twoFactor.enable(),
    mutationFn: authApi.confirm2fa,
    onMutate: () => adminAuthStoreAction.setLoading(true),
    onSuccess: () => {
      const { user, token, expiresAt, sessionCode2Fa } = adminAuthStore.state

      if (!user || !token || !expiresAt) return

      const newUserData: AdminUser = {
        ...user,
        requiresTwoFactor: true,
      }

      adminAuthStoreAction.setAuth(
        newUserData,
        token,
        expiresAt,
        sessionCode2Fa ?? null,
      )
    },
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useDisable2Fa = () =>
  useMutation<
    ApiResponse<Disable2FaAdminResponse>,
    AxiosError<ApiResponse<null>>,
    Disable2FaAdminRequest
  >({
    mutationKey: AdminAuthQueryKeys.twoFactor.disable(),
    mutationFn: authApi.disable2fa,
    onSuccess: () => {
      const { user, token, expiresAt, sessionCode2Fa } = adminAuthStore.state

      if (!user || !token || !expiresAt) return

      const newUserData: AdminUser = {
        ...user,
        requiresTwoFactor: false,
      }

      adminAuthStoreAction.setAuth(
        newUserData,
        token,
        expiresAt,
        sessionCode2Fa ?? null,
      )
    },
    onMutate: () => adminAuthStoreAction.setLoading(true),
    onSettled: () => adminAuthStoreAction.setLoading(false),
  })

export const useAuthUser = () =>
  useQuery<AdminUserInfoResponse, AxiosError<ApiResponse<null>>>({
    queryKey: AdminAuthQueryKeys.me(),
    queryFn: async () => {
      const response = await authApi.me()
      return response.data
    },
    staleTime: 5 * 60 * 1000,
    gcTime: 10 * 60 * 1000,
    refetchOnMount: false,
    refetchOnWindowFocus: false,
  })
