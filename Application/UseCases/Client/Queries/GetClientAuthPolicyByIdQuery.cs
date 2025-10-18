using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Constants.ValueObject;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Entities;
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
            return ApiResult<AuthPolicy>.Fail(ApiErrors.OAuthApplication.NotFound());

        var policy = ReadAuthPolicy.GetAuthPolicy(app.Properties);

        return ApiResult<AuthPolicy>.Ok(policy);
    }
}