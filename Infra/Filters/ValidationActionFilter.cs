using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IbraHabra.NET.Infra.Filters;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ValidateModelAttribute : ActionFilterAttribute
{
    private readonly Type _validatorType;

    public ValidateModelAttribute(Type validatorType)
    {
        _validatorType = validatorType;
    }

    public override async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var serviceProvider = context.HttpContext.RequestServices;

        // Get the validator from DI
        if (serviceProvider.GetService(_validatorType) is not IValidator validator)
        {
            await next();
            return;
        }

        // Find the command/query to validate
        object? modelToValidate = null;
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument != null && IsValidatableType(argument.GetType()))
            {
                modelToValidate = argument;
                break;
            }
        }

        if (modelToValidate == null)
        {
            await next();
            return;
        }

        // Validate
        var validationContext = new ValidationContext<object>(modelToValidate);
        var validationResult = await validator.ValidateAsync(validationContext);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            context.Result = new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = "Validation failed",
                errors
            });
            return;
        }

        await next();
    }

    private static bool IsValidatableType(Type type)
    {
        return !type.IsPrimitive 
               && type != typeof(string) 
               && type != typeof(Guid) 
               && type != typeof(DateTime);
    }
}