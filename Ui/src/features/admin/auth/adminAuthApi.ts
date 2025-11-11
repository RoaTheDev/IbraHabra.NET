import { apiClient, adminAuthEndpoint } from '@/lib/apiClient.ts'
import {
  AdminUserInfoResponse,
  ConfirmEnable2FaAdminRequest,
  ConfirmEnable2FaAdminResponse,
  Disable2FaAdminRequest,
  Disable2FaAdminResponse,
  Enable2FaAdminResponse,
  LoginRequest,
  LoginResponse,
  RegenerateRecoveryCodesAdminRequest,
  RegenerateRecoveryCodesAdminResponse,
  Verify2FaAdminRequest,
  Verify2FaAdminResponse,
  VerifyAdminUserResponse,
  VerifyRecovery2FaAdminRequest,
  VerifyRecovery2FaAdminResponse,
} from '@/features/admin/auth/adminAuthTypes.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'

const endpoint = {
  login: `${adminAuthEndpoint}/login`,
  me: `${adminAuthEndpoint}/me`,
  enable2fa: `${adminAuthEndpoint}/2fa/enable`,
  disable2fa: `${adminAuthEndpoint}/2fa/disable`,
  confirm2fa: `${adminAuthEndpoint}/2fa/enable/confirm`,
  verify2fa: `${adminAuthEndpoint}/verify-2fa`,
  recovery2fa: `${adminAuthEndpoint}/2fa/recovery`,
  verify: `${adminAuthEndpoint}/verify`,
  logout: `${adminAuthEndpoint}/logout`,
  regenerateRecoveryCodes: `${adminAuthEndpoint}/regenerate-recovery-codes`,
}

export const authApi = {
  login: (payload: LoginRequest): Promise<ApiResponse<LoginResponse>> =>
    apiClient.post<ApiResponse<LoginResponse>>(endpoint.login, payload),


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
  verify: (): Promise<ApiResponse<VerifyAdminUserResponse>> =>
    apiClient.get(endpoint.verify),
  logout: (): Promise<ApiResponse<string>> => apiClient.post(endpoint.logout),
  recovery2fa: (
    payload: VerifyRecovery2FaAdminRequest,
  ): Promise<ApiResponse<VerifyRecovery2FaAdminResponse>> =>
    apiClient.post<ApiResponse<VerifyRecovery2FaAdminResponse>>(
      endpoint.recovery2fa,
      payload,
    ),
  regenerateRecoveryCodes: async (
    payload: RegenerateRecoveryCodesAdminRequest,
  ) => {
    return apiClient.post<ApiResponse<RegenerateRecoveryCodesAdminResponse>>(
      endpoint.regenerateRecoveryCodes,
      payload,
    )
  },
}
