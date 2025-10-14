using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
using OpenIddict.Abstractions;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Client.Queries;

public record GetClientAuthPolicyQuery(string ClientId);

public class GetClientAuthPolicyHandler : IWolverineHandler
{
    public static async Task<ApiResult<AuthPolicy>> Handle(
        GetClientAuthPolicyQuery query,
        IRepo<OauthApplication, string> appRepo)
    {
        var app = await appRepo.GetViaConditionAsync(a => a.ClientId == query.ClientId,
            c => new { c.Properties });

        if (app == null)
            return ApiResult<AuthPolicy>.Fail(404, "Client not found.");

        var policy = ReadAuthPolicy.GetAuthPolicy(app.Properties);

        return ApiResult<AuthPolicy>.Ok(policy);
    }
}