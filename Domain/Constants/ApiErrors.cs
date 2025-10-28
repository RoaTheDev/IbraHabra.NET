using IbraHabra.NET.Domain.Contract;

namespace IbraHabra.NET.Domain.Constants;

public static class ApiErrors
{
    public static ApiError Validation(string fieldName, string msg) => new(
        $"{fieldName.ToUpper()}_INVALID",
        msg,
        ErrorType.Validation,
        fieldName
    );

    public static class User
    {
        public static ApiError DuplicateEmail() => new(
            "DUPLICATE_EMAIL",
            "This email address is already registered. Please use a different email or sign in to your existing account.",
            ErrorType.Conflict
        );

        public static ApiError DuplicateUsername() => new(
            "DUPLICATE_USERNAME",
            "This username is already taken. Please choose a different username.",
            ErrorType.Conflict
        );
        public static ApiError FailToCreateUser(string msg) => new(
            "USER_CREATION_FAILED",
            $"User creation failed due to {msg}",
            ErrorType.Conflict
        );
        
        public static ApiError FailToDisable2Fa(string msg) => new(
            "DiSABLE_2FA_FAILED",
            $"Disable Two Factor failed due to {msg}",
            ErrorType.Conflict
        );
        
        
        public static ApiError FailToEnable2Fa(string msg) => new(
            "ENABLE_2FA_FAILED",
            $"Enable Two Factor failed due to {msg}",
            ErrorType.Conflict
        );
        public static ApiError NotFound() => new(
            "USER_NOT_FOUND",
            "User not found. The user may have been deleted or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError EmailNotVerified() => new(
            "EMAIL_NOT_VERIFIED",
            "Please verify your email address before proceeding. Check your inbox for the verification link.",
            ErrorType.Unauthorized
        );
        public static ApiError EmailAlreadyVerified() => new(
            "EMAIL_VERIFIED",
            "Email already verified. You don't need to verify again",
            ErrorType.BusinessRule
        );

        public static ApiError AccountLocked(int minutes) => new(
            "ACCOUNT_LOCKED",
            $"Your account has been locked due to multiple failed login attempts. Please try again in {minutes} minutes or reset your password.",
            ErrorType.Unauthorized
        );
   
        public static ApiError AccountDisabled() => new(
            "ACCOUNT_DISABLED",
            "Your account has been disabled. Please contact support for assistance.",
            ErrorType.Forbidden
        );

        public static ApiError CannotDeleteSelf() => new(
            "CANNOT_DELETE_SELF",
            "You cannot delete your own account. Please contact another administrator.",
            ErrorType.BusinessRule
        );
        public static ApiError InvalidEmailConfirmationToken() => new(
            "EMAIL_CONFIRMATION_TOKEN_INVALID",
            "The confirmation token is not valid or has expired.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotModifySystemUser() => new(
            "CANNOT_MODIFY_SYSTEM_USER",
            "System users cannot be modified or deleted.",
            ErrorType.BusinessRule
        );

        public static ApiError WeakPassword() => new(
            "WEAK_PASSWORD",
            "Password does not meet the minimum security requirements. Please use a stronger password.",
            ErrorType.Validation
        );

        public static ApiError PasswordRequirementNotMet(string msg) => new(
            "WEAK_PASSWORD",
            $"Password does not meet '{msg}'. Please ensure you have it.",
            ErrorType.Validation
        );
        
        public static ApiError InvalidTwoFactorCode() => new(
            "TWO_FACTOR_DOES_NOT_EXIST",
            "Two-factor code does not exist or is incorrect.",
            ErrorType.BusinessRule
        );

        
        public static ApiError CannotDisableTwoFactor() => new(
            "CANNOT_DISABLE_TWO_FACTOR_DOES_NOT_EXIST",
            "Two-factor authentication must be enabled first to disable.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotEnableTwoFactor() => new(
            "TWO_FACTOR_ALREADY_EXIST",
            "Two-factor authentication already  enabled.",
            ErrorType.BusinessRule
        );

        public static ApiError FailToDisableTwoFactor() => new(
            "DISABLE_TWO_FACTOR_NOT_WORKING",
            "Disable two-factor not working. Please ensure there're no business rule violation",
            ErrorType.BusinessRule
        );

        public static ApiError FailAuthKeyGeneration() => new(
            "FAIL_TO_GENERATE_AUTHENTICATION_KEY",
            "Authentication Key for 2Fa not working. Please ensure there's no business rule violation",
            ErrorType.BusinessRule
        );

        public static ApiError InvalidPasswordResetToken() => new(
            "INVALID_RESET_TOKEN",
            "Password reset token is invalid or has expired. Please request a new password reset.",
            ErrorType.Unauthorized
        );
    }

    public static class Authentication
    {
        public static ApiError InvalidCredentials() => new(
            "INVALID_CREDENTIALS",
            "Invalid email or password. Please check your credentials and try again.",
            ErrorType.Unauthorized
        );
        public static ApiError InvalidSession() => new(
            "INVALID_SESSION",
            "Session key not found. Please check your credentials and try again.",
            ErrorType.Unauthorized
        );
        public static ApiError InvalidToken() => new(
            "INVALID_TOKEN",
            "The provided token is invalid or has expired. Please sign in again.",
            ErrorType.Unauthorized
        );

        public static ApiError TokenExpired() => new(
            "TOKEN_EXPIRED",
            "Your session has expired. Please sign in again to continue.",
            ErrorType.Unauthorized
        );

        public static ApiError RefreshTokenExpired() => new(
            "REFRESH_TOKEN_EXPIRED",
            "Your refresh token has expired. Please sign in again.",
            ErrorType.Unauthorized
        );

        public static ApiError InvalidRefreshToken() => new(
            "INVALID_REFRESH_TOKEN",
            "The provided refresh token is invalid or has been revoked.",
            ErrorType.Unauthorized
        );

        public static ApiError TwoFactorRequired() => new(
            "TWO_FACTOR_REQUIRED",
            "Two-factor authentication is required. Please provide your verification code.",
            ErrorType.Unauthorized
        );

        public static ApiError InvalidTwoFactorCode() => new(
            "INVALID_TWO_FACTOR_CODE",
            "The verification code is invalid or has expired. Please try again.",
            ErrorType.Unauthorized
        );

        public static ApiError SessionNotFound() => new(
            "SESSION_NOT_FOUND",
            "Session not found or has been terminated. Please sign in again.",
            ErrorType.Unauthorized
        );

        public static ApiError MaxSessionsExceeded() => new(
            "MAX_SESSIONS_EXCEEDED",
            "Maximum number of active sessions reached. Please sign out from another device.",
            ErrorType.BusinessRule
        );

        public static ApiError SuspiciousActivity() => new(
            "SUSPICIOUS_ACTIVITY",
            "Suspicious activity detected. Please verify your identity or contact support.",
            ErrorType.Unauthorized
        );

        public static ApiError ImpossibleTravel() => new(
            "IMPOSSIBLE_TRAVEL",
            "Login detected from an unusual location. Please verify your identity.",
            ErrorType.Unauthorized
        );
    }

    public static class Authorization
    {
        public static ApiError InsufficientPermissions() => new(
            "INSUFFICIENT_PERMISSIONS",
            "You don't have permission to perform this action. Please contact your administrator if you need access.",
            ErrorType.Forbidden
        );

        public static ApiError RoleNotFound() => new(
            "ROLE_NOT_FOUND",
            "Role not found. The role may have been deleted or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError CannotDeleteSystemRole() => new(
            "CANNOT_DELETE_SYSTEM_ROLE",
            "System roles cannot be deleted or modified.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotRemoveLastAdmin() => new(
            "CANNOT_REMOVE_LAST_ADMIN",
            "Cannot remove the last administrator. At least one administrator must remain.",
            ErrorType.BusinessRule
        );

        public static ApiError RoleAlreadyAssigned() => new(
            "ROLE_ALREADY_ASSIGNED",
            "This role is already assigned to the user.",
            ErrorType.Conflict
        );

        public static ApiError InvalidScope() => new(
            "INVALID_SCOPE",
            "One or more requested scopes are invalid or not available.",
            ErrorType.Validation
        );

        public static ApiError ScopeNotGranted() => new(
            "SCOPE_NOT_GRANTED",
            "The requested scope has not been granted to this application.",
            ErrorType.Forbidden
        );
    }

    public static class Project
    {
        public static ApiError NotFound() => new(
            "PROJECT_NOT_FOUND",
            "Project not found. The project may have been deleted or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError DuplicateName() => new(
            "DUPLICATE_PROJECT_NAME",
            "A project with this name already exists. Please use a different name.",
            ErrorType.Conflict
        );

        public static ApiError CannotDeleteWithActiveMembers() => new(
            "CANNOT_DELETE_PROJECT_WITH_MEMBERS",
            "Cannot delete project with active members. Please remove all members first.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotDeleteWithActiveApplications() => new(
            "CANNOT_DELETE_PROJECT_WITH_APPS",
            "Cannot delete project with active OAuth applications. Please remove all applications first.",
            ErrorType.BusinessRule
        );

        public static ApiError ProjectInactive() => new(
            "PROJECT_INACTIVE",
            "This project is inactive. Please reactivate the project before making changes.",
            ErrorType.BusinessRule
        );

        public static ApiError RegistrationDisabled() => new(
            "REGISTRATION_DISABLED",
            "Registration is currently disabled for this project.",
            ErrorType.BusinessRule
        );

        public static ApiError SocialLoginDisabled() => new(
            "SOCIAL_LOGIN_DISABLED",
            "Social login is disabled for this project.",
            ErrorType.BusinessRule
        );
    }

    public static class ProjectMember
    {
        public static ApiError NotFound() => new(
            "PROJECT_MEMBER_NOT_FOUND",
            "Project member not found. The member may have been removed or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError AlreadyExists() => new(
            "MEMBER_ALREADY_EXISTS",
            "This user is already a member of the project.",
            ErrorType.Conflict
        );

        public static ApiError CannotRemoveSelf() => new(
            "CANNOT_REMOVE_SELF",
            "You cannot remove yourself from the project. Ask another administrator to remove you.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotRemoveLastOwner() => new(
            "CANNOT_REMOVE_LAST_OWNER",
            "Cannot remove the last owner from the project. Transfer ownership first.",
            ErrorType.BusinessRule
        );

        public static ApiError InvalidRole() => new(
            "INVALID_PROJECT_ROLE",
            "The specified project role is invalid or not available.",
            ErrorType.Validation
        );

        public static ApiError InsufficientRolePermissions() => new(
            "INSUFFICIENT_ROLE_PERMISSIONS",
            "You don't have permission to assign this role. Contact a project owner.",
            ErrorType.Forbidden
        );
    }

    public static class ProjectRole
    {
        public static ApiError NotFound() => new(
            "PROJECT_ROLE_NOT_FOUND",
            "Project role not found. The role may have been deleted.",
            ErrorType.NotFound
        );

        public static ApiError DuplicateName() => new(
            "DUPLICATE_ROLE_NAME",
            "A role with this name already exists in the project. Please use a different name.",
            ErrorType.Conflict
        );

        public static ApiError CannotDeleteSystemRole() => new(
            "CANNOT_DELETE_SYSTEM_ROLE",
            "System roles cannot be deleted or modified.",
            ErrorType.BusinessRule
        );

        public static ApiError CannotDeleteRoleWithMembers() => new(
            "CANNOT_DELETE_ROLE_WITH_MEMBERS",
            "Cannot delete role that is assigned to project members. Reassign members first.",
            ErrorType.BusinessRule
        );

        public static ApiError InvalidPermission() => new(
            "INVALID_PERMISSION",
            "One or more specified permissions are invalid.",
            ErrorType.Validation
        );

        public static ApiError PermissionAlreadyAssigned() => new(
            "PERMISSION_ALREADY_ASSIGNED",
            "This permission is already assigned to the role.",
            ErrorType.Conflict
        );
    }

    public static class Permission
    {
        public static ApiError NotFound() => new(
            "PERMISSION_NOT_FOUND",
            "Permission not found. The permission may have been deleted or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError DuplicateName() => new(
            "DUPLICATE_PERMISSION_NAME",
            "A permission with this name already exists.",
            ErrorType.Conflict
        );

        public static ApiError CannotDeleteSystemPermission() => new(
            "CANNOT_DELETE_SYSTEM_PERMISSION",
            "System permissions cannot be deleted or modified.",
            ErrorType.BusinessRule
        );

        public static ApiError PermissionInUse() => new(
            "PERMISSION_IN_USE",
            "This permission is currently assigned to one or more roles and cannot be deleted.",
            ErrorType.BusinessRule
        );
    }

    public static class OAuthApplication
    {
        public static ApiError NotFound() => new(
            "OAUTH_APPLICATION_NOT_FOUND",
            "OAuth application not found. The application may have been deleted or the ID is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError SecretKeyRuleViolation() => new("CLIENT_TYPE_VIOLATION",
            "Only confidential clients can have client secrets.",
            ErrorType.BusinessRule);

        public static ApiError InvalidClientId() => new(
            "INVALID_CLIENT_ID",
            "The provided client ID is invalid.",
            ErrorType.Unauthorized
        );
        public static ApiError InvalidClient() => new(
            "INVALID_CLIENT",
            "The provided client has no clue on the end user. please make sure you entered the correct client",
            ErrorType.Unauthorized
        );
        public static ApiError InvalidClientSecret() => new(
            "INVALID_CLIENT_SECRET",
            "The provided client secret is invalid.",
            ErrorType.Unauthorized
        );

        public static ApiError InvalidRedirectUri() => new(
            "INVALID_REDIRECT_URI",
            "The redirect URI is not registered for this application.",
            ErrorType.Validation
        );

        public static ApiError InvalidGrantType() => new(
            "INVALID_GRANT_TYPE",
            "The grant type is not supported or not allowed for this application.",
            ErrorType.Validation
        );

        public static ApiError ApplicationInactive() => new(
            "APPLICATION_INACTIVE",
            "This OAuth application is inactive and cannot be used.",
            ErrorType.BusinessRule
        );

        public static ApiError DuplicateClientId() => new(
            "DUPLICATE_CLIENT_ID",
            "An application with this client ID already exists.",
            ErrorType.Conflict
        );

        public static ApiError InvalidClientType() => new(
            "INVALID_CLIENT_TYPE",
            "The specified client type is invalid. Must be 'confidential' or 'public'.",
            ErrorType.Validation
        );

        public static ApiError MissingRequiredScopes() => new(
            "MISSING_REQUIRED_SCOPES",
            "One or more required scopes are missing from the request.",
            ErrorType.Validation
        );

        public static ApiError ExceededMaxApplications() => new(
            "EXCEEDED_MAX_APPLICATIONS",
            "Maximum number of OAuth applications reached for this project.",
            ErrorType.BusinessRule
        );
    }

    public static class Scope
    {
        public static ApiError NotFound() => new(
            "SCOPE_NOT_FOUND",
            "Scope not found. The scope may have been deleted or the name is incorrect.",
            ErrorType.NotFound
        );

        public static ApiError DuplicateName() => new(
            "DUPLICATE_SCOPE_NAME",
            "A scope with this name already exists in the project.",
            ErrorType.Conflict
        );

        public static ApiError CannotDeleteSystemScope() => new(
            "CANNOT_DELETE_SYSTEM_SCOPE",
            "System scopes cannot be deleted or modified.",
            ErrorType.BusinessRule
        );

        public static ApiError ScopeInactive() => new(
            "SCOPE_INACTIVE",
            "This scope is inactive and cannot be used.",
            ErrorType.BusinessRule
        );

        public static ApiError ScopeInUse() => new(
            "SCOPE_IN_USE",
            "This scope is currently in use by one or more applications and cannot be deleted.",
            ErrorType.BusinessRule
        );
    }

    public static class AuditTrail
    {
        public static ApiError NotFound() => new(
            "AUDIT_TRAIL_NOT_FOUND",
            "Audit trail record not found.",
            ErrorType.NotFound
        );

        public static ApiError InvalidDateRange() => new(
            "INVALID_DATE_RANGE",
            "The specified date range is invalid. End date must be after start date.",
            ErrorType.Validation
        );

        public static ApiError DateRangeTooLarge() => new(
            "DATE_RANGE_TOO_LARGE",
            "The specified date range is too large. Maximum range is 90 days.",
            ErrorType.Validation
        );

        public static ApiError RetrievalFailed() => new(
            "AUDIT_TRAIL_RETRIEVAL_FAILED",
            "Unable to retrieve audit trail records. Please try again or contact support.",
            ErrorType.ServerError
        );
    }

    public static class Session
    {
        public static ApiError NotFound() => new(
            "SESSION_NOT_FOUND",
            "Session not found or has been terminated.",
            ErrorType.NotFound
        );

        public static ApiError Expired() => new(
            "SESSION_EXPIRED",
            "Your session has expired. Please sign in again.",
            ErrorType.Unauthorized
        );

        public static ApiError InvalidToken() => new(
            "INVALID_SESSION_TOKEN",
            "The session token is invalid or has been revoked.",
            ErrorType.Unauthorized
        );

        public static ApiError CannotRevokeCurrentSession() => new(
            "CANNOT_REVOKE_CURRENT_SESSION",
            "You cannot revoke your current active session. Please use sign out instead.",
            ErrorType.BusinessRule
        );

        public static ApiError DeviceNotTrusted() => new(
            "DEVICE_NOT_TRUSTED",
            "This device is not trusted. Please verify your identity.",
            ErrorType.Unauthorized
        );

        public static ApiError MaxDevicesExceeded() => new(
            "MAX_DEVICES_EXCEEDED",
            "Maximum number of trusted devices reached. Please remove a device before adding a new one.",
            ErrorType.BusinessRule
        );
    }

    public static class RateLimit
    {
        public static ApiError TooManyRequests(int retryAfterSeconds) => new(
            "RATE_LIMIT_EXCEEDED",
            $"Too many requests. Please try again in {retryAfterSeconds} seconds.",
            ErrorType.BusinessRule
        );

        public static ApiError TooManyLoginAttempts(int minutes) => new(
            "TOO_MANY_LOGIN_ATTEMPTS",
            $"Too many failed login attempts. Please try again in {minutes} minutes.",
            ErrorType.Unauthorized
        );

        public static ApiError TooManyPasswordResets() => new(
            "TOO_MANY_PASSWORD_RESETS",
            "Too many password reset requests. Please try again later.",
            ErrorType.BusinessRule
        );

        public static ApiError TooManyVerificationEmails() => new(
            "TOO_MANY_VERIFICATION_EMAILS",
            "Too many verification email requests. Please check your inbox or try again later.",
            ErrorType.BusinessRule
        );
    }

    public static class Common
    {
        public static ApiError InvalidPaginationParameters() => new(
            "INVALID_PAGINATION",
            "Invalid pagination parameters. Page must be greater than 0 and page size must be between 1 and 100.",
            ErrorType.Validation
        );

        public static ApiError InvalidSortParameter() => new(
            "INVALID_SORT_PARAMETER",
            "The specified sort parameter is invalid.",
            ErrorType.Validation
        );

        public static ApiError InvalidFilterParameter() => new(
            "INVALID_FILTER_PARAMETER",
            "One or more filter parameters are invalid.",
            ErrorType.Validation
        );

        public static ApiError ServerError() => new(
            "INTERNAL_SERVER_ERROR",
            "An unexpected error occurred. Please try again or contact support if the problem persists.",
            ErrorType.ServerError
        );

        public static ApiError ServiceUnavailable() => new(
            "SERVICE_UNAVAILABLE",
            "Service is temporarily unavailable. Please try again later.",
            ErrorType.ServerError
        );

        public static ApiError MaintenanceMode() => new(
            "MAINTENANCE_MODE",
            "System is currently under maintenance. Please try again later.",
            ErrorType.ServerError
        );

        public static ApiError InvalidRequest() => new(
            "INVALID_REQUEST",
            "The request is invalid or malformed. Please check your input and try again.",
            ErrorType.Validation
        );

        public static ApiError ResourceNotFound() => new(
            "RESOURCE_NOT_FOUND",
            "The requested resource was not found.",
            ErrorType.NotFound
        );
    }
}