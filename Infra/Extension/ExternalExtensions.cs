using FluentValidation;
using IbraHabra.NET.Infra.Docs;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace IbraHabra.NET.Infra.Extension;

public static class ExternalExtensions
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
}