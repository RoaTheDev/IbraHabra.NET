using IbraHabra.NET.Domain.SharedKernel.Interface;
using IbraHabra.NET.Infra.Repo;

namespace IbraHabra.NET.Infra.Extension;

public static class InternalServiceRegistry
{
    public static void RegisterDependency(this IServiceCollection services) =>
        services.AddScoped(typeof(IRepo<,>), typeof(Repo<,>));
}