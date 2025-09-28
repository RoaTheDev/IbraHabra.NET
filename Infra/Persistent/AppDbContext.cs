using IbraHabra.NET.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace IbraHabra.NET.Infra.Persistent;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<AvailableScope> AvailableScopes { get; set; }
    public DbSet<OauthApplication> Client { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<ProjectRole> ProjectRoles { get; set; }
    public DbSet<ProjectRolePermission> ProjectRolePermissions { get; set; }
    public DbSet<Projects> Projects { get; set; }
    public DbSet<UserAuditTrail> UserAuditTrails { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureOpenIdDict(builder);
        ConfigureIdentity(builder);
        ConfigureAvailableScope(builder);
        ConfigurePermission(builder);
        ConfigureProjectMember(builder);
        ConfigureProjectRole(builder);
        ConfigureProjectRolePermission(builder);
        ConfigureProject(builder);
        ConfigureUserAuditTrail(builder);
        ConfigureUserSession(builder);
    }

    private void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<User>(e =>
        {
            e.ToTable("users", "identity");
            e.Property(f => f.FirstName).HasMaxLength(50);
            e.Property(f => f.LastName).HasMaxLength(50);
        });
        builder.Entity<Role>().ToTable("roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
    }

    private void ConfigureOpenIdDict(ModelBuilder builder)
    {
        builder.UseOpenIddict();

        // Configure your custom entity - it will inherit from OpenIddictEntityFrameworkCoreApplication
        builder.Entity<OauthApplication>(e =>
        {
            e.ToTable("oauth_applications", "identity",
                tb => tb.HasCheckConstraint("CK_Client_MinPasswordLength",
                    "jsonb_path_exists(\"Properties\", '$.authPolicy.MinPasswordLength') AND " +
                    "(\"Properties\"::jsonb->'authPolicy'->>'MinPasswordLength')::int >= 6"));

            e.Property(f => f.ClientType)
                .HasConversion<string>()
                .IsRequired();

            // Add check constraint for auth policy


            // Index on ProjectId and ClientType
            e.HasIndex(f => new { f.ProjectId, f.ClientType })
                .IsUnique();

            // Relationships
            e.HasOne(oa => oa.Projects)
                .WithMany(p => p.OauthApplications)
                .HasForeignKey(oa => oa.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure other OpenIddict entities
        builder.Entity<OpenIddictEntityFrameworkCoreAuthorization>(e =>
        {
            e.ToTable("oauth_authorizations", "identity");
        });

        builder.Entity<OpenIddictEntityFrameworkCoreScope>(e => { e.ToTable("oauth_scopes", "identity"); });

        builder.Entity<OpenIddictEntityFrameworkCoreToken>(e => { e.ToTable("oauth_tokens", "identity"); });
    }

    private void ConfigureAvailableScope(ModelBuilder builder)
    {
        builder.Entity<AvailableScope>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();

            e.HasIndex(f => new { f.ProjectId, f.Name })
                .IsUnique();

            e.Property(f => f.Name)
                .HasMaxLength(150)
                .IsRequired();

            e.Property(f => f.Description)
                .HasMaxLength(250)
                .IsRequired(false);

            e.HasOne(f => f.Project)
                .WithMany(p => p.AvailableScopes)
                .HasForeignKey(f => f.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.ToTable("available_scopes", "realms");
        });
    }

    private void ConfigurePermission(ModelBuilder builder)
    {
        builder.Entity<Permission>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.Name).HasMaxLength(50).IsRequired();

            // Add unique constraint on permission name
            e.HasIndex(f => f.Name).IsUnique();

            e.ToTable("permissions", "realms");
        });
    }

    private void ConfigureProjectMember(ModelBuilder builder)
    {
        builder.Entity<ProjectMember>(e =>
        {
            e.HasKey(pm => new ProjectMemberId(pm.ProjectId, pm.UserId));

            // Relationships
            e.HasOne(pm => pm.Projects)
                .WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(pm => pm.ProjectRole)
                .WithMany()
                .HasForeignKey(pm => pm.ProjectRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.ToTable("project_members", "realms");
        });
    }

    private void ConfigureProjectRole(ModelBuilder builder)
    {
        builder.Entity<ProjectRole>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.Name).HasMaxLength(50).IsRequired();
            e.Property(f => f.Description).HasMaxLength(250).IsRequired(false);

            // Add foreign key relationship to Projects
            e.HasOne(pr => pr.Project)
                .WithMany(p => p.ProjectRoles)
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraint for role name per project
            e.HasIndex(f => new { f.ProjectId, f.Name }).IsUnique();

            e.ToTable("project_roles", "realms");
        });
    }

    private void ConfigureProjectRolePermission(ModelBuilder builder)
    {
        builder.Entity<ProjectRolePermission>(e =>
        {
            e.HasKey(f => new ProjectRolePermissionId(f.ProjectRoleId, f.PermissionId));


            e.HasOne(prp => prp.ProjectRole)
                .WithMany(pr => pr.ProjectRolePermissions)
                .HasForeignKey(prp => prp.ProjectRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(prp => prp.Permission)
                .WithMany()
                .HasForeignKey(prp => prp.PermissionId)
                .OnDelete(DeleteBehavior.Restrict);

            e.ToTable("project_role_permissions", "realms");
        });
    }

    private void ConfigureProject(ModelBuilder builder)
    {
        builder.Entity<Projects>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.HomePageUrl).HasMaxLength(255).IsRequired();
            e.Property(f => f.LogoUrl).HasMaxLength(255).IsRequired(false);
            e.Property(f => f.Description).HasMaxLength(255).IsRequired(false);
            e.Property(f => f.DisplayName).HasMaxLength(50).IsRequired();
            
            e.HasIndex(f => f.DisplayName).IsUnique();

            e.ToTable("projects", "realms");
        });
    }

    private void ConfigureUserAuditTrail(ModelBuilder builder)
    {
        builder.Entity<UserAuditTrail>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();

            e.Property(f => f.IpAddress).HasMaxLength(50).IsRequired();
            e.Property(f => f.IpAddressHash).HasMaxLength(500).IsRequired();
            e.Property(f => f.Country).HasMaxLength(100).IsRequired();
            e.Property(f => f.City).HasMaxLength(200).IsRequired();
            e.Property(f => f.Region).HasMaxLength(50).IsRequired();
            e.Property(f => f.ClientId).HasMaxLength(255).IsRequired(false);
            e.Property(f => f.FailureReason).HasMaxLength(200).IsRequired(false);
            e.Property(f => f.AlertType).HasMaxLength(150).IsRequired(false);
            e.Property(f => f.UserAgent).HasMaxLength(1000).IsRequired();

            e.HasOne(uat => uat.User)
                .WithMany(f => f.UserAuditTrails)
                .HasForeignKey(uat => uat.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add index for performance on common queries
            e.HasIndex(f => new { f.UserId, f.LoginAt });
            e.HasIndex(f => f.IpAddressHash);

            e.ToTable("user_audit_trails", "realms");
        });
    }

    private void ConfigureUserSession(ModelBuilder builder)
    {
        builder.Entity<UserSession>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).ValueGeneratedOnAdd();
            e.Property(f => f.UserId).IsRequired(); 
            e.Property(f => f.SessionToken).HasMaxLength(255).IsRequired();
            e.Property(f => f.ClientId).HasMaxLength(255).IsRequired();
            e.Property(f => f.DeviceInfo).HasMaxLength(500).IsRequired();
            e.Property(f => f.IpAddress).HasMaxLength(150).IsRequired();
            e.Property(f => f.Country).HasMaxLength(100).IsRequired();

            e.HasOne<User>()
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(f => f.SessionToken).IsUnique();
            e.HasIndex(f => new { f.UserId, f.IsCurrent });

            e.ToTable("user_sessions", "realms");
        });
    }
}