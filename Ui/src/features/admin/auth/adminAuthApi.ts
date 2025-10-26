import { apiClient } from '@/lib/apiClient.ts'
import {
  AdminUserInfoResponse,
  ConfirmEnable2FaAdminRequest,
  ConfirmEnable2FaAdminResponse,
  Disable2FaAdminRequest,
  Disable2FaAdminResponse,
  Enable2FaAdminResponse,
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  RefreshTokenResponse,
  Verify2FaAdminRequest,
  Verify2FaAdminResponse,
  VerifyAdminUserResponse,
} from '@/features/admin/auth/adminAuthTypes.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'
import { CreateUserRequest } from '@/features/admin/manageUser/adminManageType.ts'

const baseUrl = '/auth'

const endpoint = {
  login: `${baseUrl}/login}`,
  createUser: `${baseUrl}/register`,
  me: `${baseUrl}/me`,
  enable2fa: `${baseUrl}/2fa/enable`,
  disable2fa: `${baseUrl}/2fa/disable`,
  confirm2fa: `${baseUrl}/2fa/enable/confirm`,
  verify2fa: `${baseUrl}/verify-2fa`,
  verify: `${baseUrl}/verify`,
  refresh: `${baseUrl}/refresh`,
  logout: `${baseUrl}/logout`,
}

export const authApi = {
  login: (payload: LoginRequest): Promise<ApiResponse<LoginResponse>> =>
    apiClient.post<ApiResponse<LoginResponse>>(endpoint.login, payload),

  createUser: (payload: CreateUserRequest): Promise<ApiResponse<string>> =>
    apiClient.post<ApiResponse<string>>(endpoint.createUser, payload),

  me: (): Promise<ApiResponse<AdminUserInfoResponse>> =>
    apiClient.get<ApiResponse<AdminUserInfoResponse>>(endpoint.me),

  enable2fa: (): Promise<ApiResponse<Enable2FaAdminResponse>> =>
    apiClient.post<ApiResponse<Enable2FaAdminResponse>>(endpoint.enable2fa),
  disable2fa: (
    payload: Disable2FaAdminRequest,
  ): Promise<ApiResponse<Disable2FaAdminResponse>> =>
    apiClient.post<ApiResponse<Disable2FaAdminResponse>>(
      endpoint.disable2fa,
      payload,
    ),
  verify2fa: (
    payload: Verify2FaAdminRequest,
  ): Promise<ApiResponse<Verify2FaAdminResponse>> =>
    apiClient.post(endpoint.verify2fa, payload),
  confirm2fa: (
    payload: ConfirmEnable2FaAdminRequest,
  ): Promise<ApiResponse<ConfirmEnable2FaAdminResponse>> =>
    apiClient.post(endpoint.confirm2fa, payload),
  refresh: (
    payload: RefreshTokenRequest,
  ): Promise<ApiResponse<RefreshTokenResponse>> =>
    apiClient.post<ApiResponse<RefreshTokenResponse>>(
      endpoint.refresh,
      payload,
    ),
  verify: (): Promise<ApiResponse<VerifyAdminUserResponse>> =>
    apiClient.get(endpoint.verify),
  logout: (): Promise<ApiResponse<string>> => apiClient.post(endpoint.logout),
}
