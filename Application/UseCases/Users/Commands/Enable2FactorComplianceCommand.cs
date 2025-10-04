using IbraHabra.NET.Application.Dto.Response;
using IbraHabra.NET.Domain.Contract.Services;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Users.Commands;


public record Enable2FaComplianceCommand(string ComplianceToken, string Code, string ClientId);

public record Enable2FaComplianceResponse(string[] RecoveryCodes, Guid UserId);

public class Enable2FComplianceHandler : IWolverineHandler
{
    public static async Task<ApiResult<Enable2FaComplianceResponse>> Handle(
        Enable2FaComplianceCommand command,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ITwoFactorTokenService tokenService)
    {
        // Validate token (one-time use)
        var tokenData = await tokenService.ValidateAndRemoveTokenAsync(command.ComplianceToken);
        if (tokenData == null)
            return ApiResult<Enable2FaComplianceResponse>.Fail(401, "Invalid or expired token.");

        // Verify client matches
        if (tokenData.Value.ClientId != command.ClientId)
            return ApiResult<Enable2FaComplianceResponse>.Fail(401, "Token not valid for this client.");

        // Get user
        var user = await userManager.FindByIdAsync(tokenData.Value.UserId.ToString());
        if (user == null)
            return ApiResult<Enable2FaComplianceResponse>.Fail(401, "User not found.");

        // Verify code
        var isValidCode = await userManager.VerifyTwoFactorTokenAsync(
            user, 
            userManager.Options.Tokens.AuthenticatorTokenProvider, 
            command.Code);

        if (!isValidCode)
            return ApiResult<Enable2FaComplianceResponse>.Fail(400, "Invalid authenticator code.");

        // Enable 2FA
        var result = await userManager.SetTwoFactorEnabledAsync(user, true);
        if (!result.Succeeded)
            return ApiResult<Enable2FaComplianceResponse>.Fail(500, "Failed to enable 2FA.");

        // Generate recovery codes
        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        // Sign in the user
        await signInManager.SignInAsync(user, isPersistent: false);

        return ApiResult<Enable2FaComplianceResponse>.Ok(new(
            RecoveryCodes: recoveryCodes!.ToArray(),
            UserId: user.Id));
    }
}