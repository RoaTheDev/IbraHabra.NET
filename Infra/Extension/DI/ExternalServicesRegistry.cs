using FluentValidation;
using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Infra.Docs;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Wolverine;

namespace IbraHabra.NET.Infra.Extension.DI;

public static class ExternalServicesRegistry
{
    public static void AddDatabaseConfig(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
        {
            var connectionString =
                $@"Server={Environment.GetEnvironmentVariable("DB_SERVER")};
                   Database={Environment.GetEnvironmentVariable("DB_NAME")};
                   User Id={Environment.GetEnvironmentVariable("DB_USER")};
                   Password={Environment.GetEnvironmentVariable("DB_PASS")};";

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

            opts.UseLazyLoadingProxies();
        });

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database");
    }

    public static void AddWolverineConfig(this IHostBuilder hostBuilder) => hostBuilder.UseWolverine(opts =>
    {
        opts.Durability.Mode = DurabilityMode.MediatorOnly;
    });

    public static void AddScalarConfig(this IServiceCollection services)
    {
        services.AddTransient<OpenApiTransformer>();
        services.AddOpenApi(options =>
        {
            var sp = services.BuildServiceProvider();
            var transformer = sp.GetRequiredService<OpenApiTransformer>();

            options.AddDocumentTransformer((document, context, cancellationToken) =>
                transformer.TransformAsync(document, context, cancellationToken)
            );
        });
    }

    public static void AddFluentConfig(this IServiceCollection services) =>
        services.AddValidatorsFromAssemblyContaining<Program>();

    public static void AddEnvBoundValues(this IServiceCollection services, IConfiguration config) =>
        services.Configure<JwtOptions>(config.GetSection("JWT"))
            .Configure<IdentitySettingOptions>(config.GetSection("IDENTITY"))
            .Configure<CorsSettings>(config.GetSection("CORS"));

    public static void AddCachingConfig(this IServiceCollection services, IConfiguration config)
    {
        var redisKey = config.GetSection("REDIS");
        services.Configure<RedisOptions>(redisKey);
        var redisOptions = redisKey.Get<RedisOptions>();
        if (redisOptions is { UseRedis: true })
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.RedisConnectionString;
                options.InstanceName = "IbraHabra:";
            });
        }
        else
        {
            services.AddMemoryCache();
        }

        services.AddScoped<ICacheService, CacheService>();
    }

    public static void AddLoggerConfig(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((hostingContext, loggerConfig) =>
            loggerConfig.ReadFrom.Configuration(hostingContext.Configuration));
    }
}