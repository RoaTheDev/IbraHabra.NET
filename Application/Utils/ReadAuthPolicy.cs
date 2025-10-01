using System.Text.Json;
using IbraHabra.NET.Domain.SharedKernel.ValueObject;

namespace IbraHabra.NET.Application.Utils;

public class ReadAuthPolicy
{
    public static AuthPolicy GetAuthPolicy(string? properties)
    {
        if (string.IsNullOrEmpty(properties))
            return new AuthPolicy();

        try
        {
            using var document = JsonDocument.Parse(properties);
            if (document.RootElement.TryGetProperty("authPolicy", out var policyElement))
            {
                return JsonSerializer.Deserialize<AuthPolicy>(policyElement.GetRawText(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new AuthPolicy();
            }
        }
        catch (JsonException)
        {
        }

        return new AuthPolicy();
    }
    public static (bool isPassed, string? errorMsg) ValidatePasswordAgainstPolicy(string password, AuthPolicy policy)
    {
        if (password.Length < policy.MinPasswordLength)
            return (false, $"Password must be at least {policy.MinPasswordLength}");
        if (policy.RequireDigit && !password.Any(char.IsDigit)) return (false, "Password required a digit.");
        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            return (false, "Password required to have one uppercase.");
        if (policy.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            return (false, "password required a symbol.");
        return (true, null);
    }

}