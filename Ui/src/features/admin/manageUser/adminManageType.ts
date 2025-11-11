export type CreateUserRequest = {
  email: string
  password: string
  firstName: string
  lastName: string
  roles: string[]
}
export type AssignRoleRequest = {
  userId: string
  roleName: string
}

export type RoleResponse = {
  roleId: string
  roleName: string
  createdAt: string
}

export type RoleResponseList = RoleResponse[]

export type GetUserInRoleRequest = {
  roleId: string
}
export type UserInRoleResponse = {
  userId: string
  email: string
  firstName: string | null
  lastName: string  | null
}
export type UserRoleResponse = {
  roleId: string
  roleName: string
}
export type UserRoleResponseList = UserRoleResponse[]
export type UserInRoleResponseList = UserInRoleResponse[]
export type GetUserRolesRequest = {
  userId: string
}

