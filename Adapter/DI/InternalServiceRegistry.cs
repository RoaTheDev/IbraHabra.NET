using IbraHabra.NET.Application.Services;
using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Domain.SharedKernel.Interface.Services;
using IbraHabra.NET.Infra.Repo;

namespace IbraHabra.NET.Adapter.DI;

public static class InternalServiceRegistry
{
    public static void RegisterDependency(this IServiceCollection services) =>
        services.AddScoped(typeof(IRepo<,>), typeof(Repo<,>));

    public static void RegisterServices(this IServiceCollection services) =>
        services.AddScoped<ICurrentUserService, CurrentUserService>()
            .AddScoped<ITwoFactorTokenService, TwoFactorTokenService>();
}