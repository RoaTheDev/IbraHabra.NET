using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Infra.Persistent;
using JasperFx.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.ErrorHandling;

namespace IbraHabra.NET.Infra.Extension;

public static class ExternalExtensions
{
    public static void AddDatabaseConfig(this IServiceCollection services, IConfiguration config)
    {
        // services.AddDbContext<AppDbContext>(options =>
        // {
        //     options.UseNpgsql(config.GetConnectionString("CONNECTION_STR"));
        //     options.UseOpenIddict();
        //     
        // });
        services.AddDbContext<AppDbContext>(opts =>
        {
            var connectionString = config.GetConnectionString("DB_CONNECTION_STR");
                                 
            opts.UseNpgsql(connectionString, npgsqlOpts =>
            {
                npgsqlOpts.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);

                npgsqlOpts.CommandTimeout(30);

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    opts.EnableSensitiveDataLogging();
                    opts.EnableDetailedErrors();
                }
            });

            opts.UseOpenIddict();

            opts.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

            opts.UseLazyLoadingProxies();
        });

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");
    }

    public static void AddWolverineConfig(this IHostBuilder hostBuilder) => hostBuilder.UseWolverine(opts =>
    {
        opts.Durability.Mode = DurabilityMode.MediatorOnly;

        opts.Policies.AutoApplyTransactions();

        opts.Policies.OnException<InvalidOperationException>()
            .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

        opts.Policies.OnException<ArgumentException>()
            .MoveToErrorQueue();
    });

    public static void AddIdentityConfig(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = config.GetValue("Identity:Password:RequireDigit", true);
                options.Password.RequireLowercase = config.GetValue("Identity:Password:RequireLowercase", true);
                options.Password.RequireNonAlphanumeric =
                    config.GetValue("Identity:Password:RequireNonAlphanumeric", true);
                options.Password.RequireUppercase = config.GetValue("Identity:Password:RequireUppercase", true);
                options.Password.RequiredLength = config.GetValue("Identity:Password:RequiredLength", 8);
                options.Password.RequiredUniqueChars = config.GetValue("Identity:Password:RequiredUniqueChars", 1);

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.SignIn.RequireConfirmedEmail =
                    config.GetValue("Identity:Login:Require_Email_Confirmation", true);
                options.SignIn.RequireConfirmedPhoneNumber =
                    config.GetValue("Identity:Login:Require_PhoneNumber_Confirmation", false);
                options.SignIn.RequireConfirmedAccount =
                    config.GetValue("Identity:Login:Require_Account_Confirmation", false);
                Console.WriteLine(options.SignIn);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddApiEndpoints()
            .AddDefaultTokenProviders();
    }
    
    
}