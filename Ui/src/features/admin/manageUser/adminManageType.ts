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
  createdAt: Date
}

export type RoleResponseList = RoleResponse[]

export type GetUserInRoleRequest = {
  roleId: string
}
export type UserRoleResponse = {
  userId: string
  email: string
  firstName: string
  lastName: string
}
export type UserRoleResponseList = RoleResponseList[]

export type GetUserRolesRequest = {
  userId: string
}

