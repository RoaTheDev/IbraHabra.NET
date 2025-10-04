# Admin Authentication & Authorization Endpoints

This document describes the admin-specific authentication and authorization endpoints for the IbraHabra IAM system.

## Overview

The admin endpoints use **JWT-based authentication** separate from the OAuth2/OpenIddict flow used by client applications. This allows administrators to manage the IAM system itself without requiring a client application context.

## Configuration

Add the following to your `appsettings.json` or environment variables:

```json
{
  "Jwt": {
    "Secret": "your-secret-key-min-32-characters-long",
    "Issuer": "IbraHabra",
    "Audience": "IbraHabra.Admin"
  }
}
```

Or set environment variable:
```bash
JWT_SECRET=your-secret-key-min-32-characters-long
```

## Authentication Endpoints

### 1. Admin Login
**POST** `/api/admin/auth/login`

Authenticates an admin user and returns a JWT token.

**Request:**
```json
{
  "email": "admin@example.com",
  "password": "SecurePassword123!"
}
```

**Response (Success):**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": {
    "userId": "guid",
    "email": "admin@example.com",
    "token": "jwt-token-here",
    "expiresAt": "2024-01-01T12:00:00Z",
    "requiresTwoFactor": false
  }
}
```

**Response (2FA Required):**
```json
{
  "isSuccess": true,
  "statusCode": 202,
  "value": {
    "userId": "guid",
    "email": "admin@example.com",
    "token": "",
    "expiresAt": "2024-01-01T12:00:00Z",
    "requiresTwoFactor": true,
    "twoFactorToken": "temp-token"
  }
}
```

### 2. Verify 2FA
**POST** `/api/admin/auth/verify-2fa`

Verifies the 2FA code and returns a JWT token.

**Request:**
```json
{
  "email": "admin@example.com",
  "code": "123456"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": {
    "userId": "guid",
    "email": "admin@example.com",
    "token": "jwt-token-here",
    "expiresAt": "2024-01-01T12:00:00Z"
  }
}
```

### 3. Refresh Token
**POST** `/api/admin/auth/refresh`

Refreshes an existing JWT token.

**Headers:**
```
Authorization: Bearer {jwt-token}
```

**Request:**
```json
{
  "token": "current-jwt-token"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": {
    "token": "new-jwt-token",
    "expiresAt": "2024-01-01T20:00:00Z"
  }
}
```

### 4. Get Current Admin Info
**GET** `/api/admin/auth/me`

Returns information about the currently authenticated admin.

**Headers:**
```
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": {
    "userId": "guid",
    "email": "admin@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "roles": ["Admin", "SuperAdmin"],
    "emailConfirmed": true,
    "twoFactorEnabled": true,
    "createdAt": "2024-01-01T00:00:00Z"
  }
}
```

### 5. Verify Token
**GET** `/api/admin/auth/verify`

Verifies if the current JWT token is valid.

**Headers:**
```
Authorization: Bearer {jwt-token}
```

**Response:**
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "value": {
    "valid": true,
    "message": "Token is valid"
  }
}
```

### 6. Logout
**POST** `/api/admin/auth/logout`

Logs out the admin user (client-side token removal).

**Headers:**
```
Authorization: Bearer {jwt-token}
```

## System Role Management

System roles (Admin, SuperAdmin, etc.) are Identity roles that apply globally across the IAM system.

### 1. Create Role
**POST** `/api/admin/roles`

**Authorization:** SuperAdmin only

**Request:**
```json
{
  "roleName": "Admin"
}
```

### 2. Get All Roles
**GET** `/api/admin/roles`

**Authorization:** Admin, SuperAdmin

### 3. Get Role by ID
**GET** `/api/admin/roles/{roleId}`

**Authorization:** Admin, SuperAdmin

### 4. Assign Role to User
**POST** `/api/admin/roles/assign`

**Authorization:** SuperAdmin only

**Request:**
```json
{
  "userId": "guid",
  "roleName": "Admin"
}
```

### 5. Remove Role from User
**POST** `/api/admin/roles/remove`

**Authorization:** SuperAdmin only

**Request:**
```json
{
  "userId": "guid",
  "roleName": "Admin"
}
```

### 6. Get Users in Role
**GET** `/api/admin/roles/{roleId}/users`

**Authorization:** Admin, SuperAdmin

### 7. Get User's Roles
**GET** `/api/admin/roles/users/{userId}`

**Authorization:** Admin, SuperAdmin

## Project Role Management

Project roles are scoped to individual projects and can have specific permissions.

### 1. Create Project Role
**POST** `/api/admin/projects/{projectId}/roles`

**Authorization:** Admin, SuperAdmin

**Request:**
```json
{
  "name": "Developer",
  "description": "Can read and write code",
  "permissionIds": ["guid1", "guid2"]
}
```

### 2. Get Project Roles
**GET** `/api/admin/projects/{projectId}/roles`

**Authorization:** Admin, SuperAdmin

### 3. Get Project Role by ID
**GET** `/api/admin/projects/{projectId}/roles/{roleId}`

**Authorization:** Admin, SuperAdmin

### 4. Update Project Role
**PUT** `/api/admin/projects/{projectId}/roles/{roleId}`

**Authorization:** Admin, SuperAdmin

**Request:**
```json
{
  "name": "Senior Developer",
  "description": "Updated description",
  "permissionIds": ["guid1", "guid2", "guid3"]
}
```

### 5. Delete Project Role
**DELETE** `/api/admin/projects/{projectId}/roles/{roleId}`

**Authorization:** Admin, SuperAdmin

### 6. Assign Project Role to User
**POST** `/api/admin/projects/{projectId}/roles/{roleId}/assign`

**Authorization:** Admin, SuperAdmin

**Request:**
```json
{
  "userId": "guid"
}
```

### 7. Remove User from Project
**POST** `/api/admin/projects/{projectId}/roles/{roleId}/remove`

**Authorization:** Admin, SuperAdmin

**Request:**
```json
{
  "userId": "guid"
}
```

### 8. Get Project Members
**GET** `/api/admin/projects/{projectId}/roles/members`

**Authorization:** Admin, SuperAdmin

## Key Differences

### System Roles vs Project Roles

- **System Roles (Role entity):**
  - Global across the entire IAM system
  - Examples: Admin, SuperAdmin, User
  - Managed via Identity framework
  - Used for IAM system administration

- **Project Roles (ProjectRole entity):**
  - Scoped to individual projects
  - Examples: Developer, Viewer, Owner
  - Can have specific permissions attached
  - Used for project-level access control

### OAuth Authentication vs Admin JWT

- **OAuth (Client Applications):**
  - Used by client applications
  - Requires ClientId and ClientSecret
  - Uses OpenIddict with authorization code flow
  - Tokens managed by OpenIddict

- **Admin JWT:**
  - Used by IAM administrators
  - Direct username/password authentication
  - Simple JWT tokens (8-hour expiry)
  - Independent of client applications

## Security Notes

1. **JWT Secret:** Must be at least 32 characters long and kept secure
2. **Token Expiry:** Admin tokens expire after 8 hours
3. **Role-Based Access:** Most endpoints require Admin or SuperAdmin role
4. **2FA Support:** Admin login supports two-factor authentication
5. **HTTPS:** Always use HTTPS in production (set `RequireHttpsMetadata = true`)

## Error Responses

All endpoints return consistent error responses:

```json
{
  "isSuccess": false,
  "statusCode": 401,
  "error": "Error message here"
}
```

Common status codes:
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid credentials or token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (duplicate resource)
- `423` - Locked (account locked out)
