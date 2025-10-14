using FluentValidation;
using IbraHabra.NET.Application.Dto;
using IbraHabra.NET.Domain.Constants;
using IbraHabra.NET.Domain.Contract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IbraHabra.NET.Infra.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateModelAttribute<TModel> : ActionFilterAttribute
    where TModel : class
{
    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var modelErrors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new ApiError(
                    $"VALIDATION_{x.Key.ToUpper().Replace(".", "_")}_INVALID",
                    string.IsNullOrEmpty(e.ErrorMessage)
                        ? $"Invalid value for {x.Key}"
                        : e.ErrorMessage,
                    ErrorType.Validation,
                    x.Key
                )))
                .ToList();

            context.Result = new BadRequestObjectResult(new ApiResponse<object>
            {
                Data = null,
                Error = modelErrors,
                Meta = new ResponseMeta
                {
                    RequestId = context.HttpContext.TraceIdentifier,
                    Version = context.HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0"
                }
            });
            return;
        }

        var validator = context.HttpContext.RequestServices
            .GetService<IValidator<TModel>>();

        if (validator == null)
        {
            await next();
            return;
        }

        var model = context.ActionArguments.Values
            .OfType<TModel>()
            .FirstOrDefault();

        if (model == null)
        {
            await next();
            return;
        }

        var validationResult = await validator.ValidateAsync(
            model,
            context.HttpContext.RequestAborted
        );

        if (validationResult.IsValid)
        {
            await next();
            return;
        }

        var errors = validationResult.Errors
            .Select(e => new ApiError(
                $"VALIDATION_{e.PropertyName.ToUpper()}_INVALID",
                e.ErrorMessage,
                ErrorType.Validation,
                e.PropertyName
            ))
            .ToList();

        context.Result = new BadRequestObjectResult(new ApiResponse<object>
        {
            Data = null,
            Error = errors,
            Meta = new ResponseMeta
            {
                RequestId = context.HttpContext.TraceIdentifier,
                Version = context.HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0"
            }
        });
    }
}