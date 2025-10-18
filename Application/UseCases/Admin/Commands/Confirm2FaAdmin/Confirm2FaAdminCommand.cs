using System.Security.Claims;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Wolverine;

namespace IbraHabra.NET.Application.UseCases.Admin.Commands.Confirm2FaAdmin;

public record ConfirmEnable2FaAdminCommand(string Code);

public record ConfirmEnable2FaAdminResponse(
    bool Success,
    string Message);

public class ConfirmEnable2FaAdminHandler : IWolverineHandler
{
    public static async Task<ApiResult<ConfirmEnable2FaAdminResponse>> Handle(
        ConfirmEnable2FaAdminCommand command,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value
                     ?? httpContextAccessor.HttpContext?.User
                         ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return ApiResult<ConfirmEnable2FaAdminResponse>.Fail(ApiErrors.Authentication.InvalidToken());

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
            return ApiResult<ConfirmEnable2FaAdminResponse>.Fail(ApiErrors.User.NotFound());

        var verificationCode = command.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var is2FaTokenValid = await userManager.VerifyTwoFactorTokenAsync(
            user, userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

        if (!is2FaTokenValid)
        {
            return ApiResult<ConfirmEnable2FaAdminResponse>.Fail(ApiErrors.Authentication.InvalidTwoFactorCode());
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);

        return ApiResult<ConfirmEnable2FaAdminResponse>.Ok(new ConfirmEnable2FaAdminResponse(
            Success: true,
            Message: "Two-factor authentication has been enabled successfully"));
    }
}