using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using IbraHabra.NET.Infra.Persistent.Projections;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;

public record Setup2FaComplianceCommand(string Email, string Password, string ClientId);

public record Setup2FaComplianceResponse(
    string QrCodeUri,
    string ManualEntryKey,
    string ComplianceToken);

public class Setup2FaComplianceHandler : IWolverineHandler
{
    public static async Task<ApiResult<Setup2FaComplianceResponse>> Handle(
        Setup2FaComplianceCommand command,
        UserManager<User> userManager,
        IRepo<OauthApplication, string> repo,
        ITwoFactorTokenService tokenService)
    {
        var client = await repo.GetViaConditionAsync(c => c.ClientId == command.ClientId && c.IsActive,
            c => new AuthPolicyAndNameProjection(c.Properties,c.DisplayName));
        if (client == null)
            return ApiResult<Setup2FaComplianceResponse>.Fail(ApiErrors.OAuthApplication.NotFound());

        // var policy = ReadAuthPolicy.GetAuthPolicy(client.Properties);
        // if (!policy.RequireMfa)
        //     return ApiResult<Setup2FaComplianceResponse>.Fail(ApiErrors.);

        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null || !await userManager.CheckPasswordAsync(user, command.Password))
            return ApiResult<Setup2FaComplianceResponse>.Fail(ApiErrors.Authentication.InvalidCredentials());

        if (await userManager.GetTwoFactorEnabledAsync(user))
            return ApiResult<Setup2FaComplianceResponse>.Fail(ApiErrors.User.CannotEnableTwoFactor());

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user);

        if (string.IsNullOrEmpty(key))
            return ApiResult<Setup2FaComplianceResponse>.Fail(ApiErrors.User.InvalidTwoFactorCode());
        
        var complianceToken = await tokenService.CreateTokenAsync(user.Id, command.ClientId);

        var qrCodeUri = TwoFactorUtils. GenerateQrCodeUri(command.Email, key,client.Name!);

        return ApiResult<Setup2FaComplianceResponse>.Ok(new(
            QrCodeUri: qrCodeUri,
            ManualEntryKey: TwoFactorUtils.FormatKey(key),
            ComplianceToken: complianceToken));
    }
}