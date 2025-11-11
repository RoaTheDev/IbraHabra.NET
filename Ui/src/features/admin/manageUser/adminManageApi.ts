import {
  adminAuthEndpoint,
  adminRoleEndpoint,
  apiClient,
} from '@/lib/apiClient.ts'
import { ApiResponse } from '@/types/ApiResponse.ts'
import {
  AssignRoleRequest,
  CreateUserRequest,
  RoleResponse,
  RoleResponseList,
  UserInRoleResponseList,
  UserRoleResponseList,
} from '@/features/admin/manageUser/adminManageType.ts'

const endpoint = {
  createUser: `${adminAuthEndpoint}/create-user`,
  getAllRoles: `${adminRoleEndpoint}`,
  getRole: (roleId: string) => `${adminRoleEndpoint}/roles/${roleId}`,
  assignRole: `${adminRoleEndpoint}/assign`,
  getUserInRoles: (roleId: string) => `${adminRoleEndpoint}/${roleId}/users`,
  getUserRole: (userId: string) => `${adminRoleEndpoint}/users/${userId}`,
}

export const adminManageApi = {
  createUser: (payload: CreateUserRequest): Promise<ApiResponse<string>> =>
    apiClient.post<ApiResponse<string>>(endpoint.createUser, payload),
  getAllRoles: (): Promise<ApiResponse<RoleResponseList>> =>
    apiClient.get<ApiResponse<RoleResponseList>>(endpoint.getAllRoles),
  getRole: (roleId: string): Promise<ApiResponse<RoleResponse>> =>
    apiClient.get<ApiResponse<RoleResponse>>(endpoint.getRole(roleId)),
  assignRole: (payload: AssignRoleRequest): Promise<ApiResponse<object>> =>
    apiClient.post(endpoint.assignRole, payload),
  getUserInRoles: (
    roleId: string,
  ): Promise<ApiResponse<UserInRoleResponseList>> =>
    apiClient.get(endpoint.getUserInRoles(roleId)),
  getUserRole: (userId: string): Promise<ApiResponse<UserRoleResponseList>> =>
    apiClient.get(endpoint.getUserRole(userId)),
}
