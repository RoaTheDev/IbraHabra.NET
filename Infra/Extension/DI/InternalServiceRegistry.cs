using FluentValidation;
using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Application.UseCases.Admin.Commands.LoginAdmin;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Infra.Persistent;
using IbraHabra.NET.Infra.Repo;

namespace IbraHabra.NET.Infra.Extension.DI;

public static class InternalServiceRegistry
{
    public static void RegisterRepo(this IServiceCollection services) =>
        services.AddScoped(typeof(IRepo<,>), typeof(Repo<,>)).AddScoped<IUnitOfWork, UnitOfWork>();

    public static void RegisterServices(this IServiceCollection services) =>
        services.AddScoped<ICurrentUserService, CurrentUserService>()
            .AddScoped<ITwoFactorTokenService, TwoFactorTokenService>()
            .AddScoped<IValidator<LoginAdminCommand>, LoginAdminValidator>();
}