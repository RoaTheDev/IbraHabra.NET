using IbraHabra.NET.Application.Dto.Request.Client;
using IbraHabra.NET.Application.UseCases.Client.Commands;
using Mapster;

namespace IbraHabra.NET.Application.Mapper;

public class ClientMapper
{
    public static void MappingConfig()
    {
        TypeAdapterConfig<(string ClientId, UpdateApplicationRequest request), PatchClientCommand>.NewConfig();
    }
}