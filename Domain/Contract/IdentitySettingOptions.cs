namespace IbraHabra.NET.Domain.Contract;

public class IdentitySettingOptions
{
    public PasswordSettings Password { get; set; } = new PasswordSettings();
    public LockoutSettings Lockout { get; set; } = new LockoutSettings();
    public SignInSettings SignIn { get; set; } = new SignInSettings();

    public class PasswordSettings
    {
        public bool RequireDigit { get; set; } = true;
        public bool RequireLowercase { get; set; } = true;
        public bool RequireNonAlphanumeric { get; set; } = true;
        public bool RequireUppercase { get; set; } = true;
        public int RequiredLength { get; set; } = 8;
        public int RequiredUniqueChars { get; set; } = 1;
    }

    public class LockoutSettings
    {
        public int MaxFailedAccessAttempts { get; set; } = 5;
        public int DefaultLockoutMinutes { get; set; } = 5;
        public bool AllowedForNewUsers { get; set; } = true;
    }

    public class SignInSettings
    {
        public bool RequireConfirmedEmail { get; set; } = true;
        public bool RequireConfirmedPhoneNumber { get; set; } = false;
        public bool RequireConfirmedAccount { get; set; } = false;
    }
}
