// using IbraHabra.NET.Application.Dto.Response;
// using IbraHabra.NET.Domain.Entities;
// using Microsoft.AspNetCore.Identity;
// using Wolverine;
//
// namespace IbraHabra.NET.Application.UseCases.Admin.Commands.RemoveRole;
//
// public record RemoveRoleCommand(Guid UserId, string RoleName);
//
// public class RemoveRoleHandler : IWolverineHandler
// {
//     public static async Task<ApiResult> Handle(
//         RemoveRoleCommand command,
//         UserManager<User> userManager)
//     {
//         var user = await userManager.FindByIdAsync(command.UserId.ToString());
//         if (user == null)
//             return ApiResult.Fail(404, "User not found.");
//
//         var isInRole = await userManager.IsInRoleAsync(user, command.RoleName);
//         if (!isInRole)
//             return ApiResult.Fail(404, $"User does not have the '{command.RoleName}' role.");
//
//         var result = await userManager.RemoveFromRoleAsync(user, command.RoleName);
//         if (!result.Succeeded)
//         {
//             var errors = string.Join(", ", result.Errors.Select(e => e.Description));
//             return ApiResult.Fail(400, $"Failed to remove role: {errors}");
//         }
//
//         return ApiResult.Ok();
//     }
// }
