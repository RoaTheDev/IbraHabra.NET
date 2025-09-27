using FluentValidation;
using IbraHabra.NET.Application.UseCases.Users;
using IbraHabra.NET.Domain.Entity;
using IbraHabra.NET.Domain.ValueObject;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.EntityFrameworkCore;

namespace IbraHabra.NET.Application.Validations;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly AppDbContext _context;

    public RegisterUserValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MustAsync(MeetPasswordPolicy).WithMessage("Password does not meet client requirements.");

        RuleFor(x => x.FirstName).MaximumLength(50);
        RuleFor(x => x.LastName).MaximumLength(50);
        RuleFor(x => x.ClientId).NotEmpty();
    }

    private async Task<bool> MeetPasswordPolicy(RegisterUserCommand command, string password, CancellationToken token)
    {
        var client = await _context.Set<OauthApplication>()
            .FirstOrDefaultAsync(c => c.ClientId == command.ClientId && c.IsActive, token);

        if (client == null) return false;

        // Extract the policy directly from the Properties JSON field
        var policy = client.GetAuthPolicy();
        return ValidatePasswordAgainstPolicy(password, policy);
    }


    private static bool ValidatePasswordAgainstPolicy(string password, AuthPolicy policy)
    {
        if (password.Length < policy.MinPasswordLength) return false;
        if (policy.RequireDigit && !password.Any(char.IsDigit)) return false;
        if (policy.RequireUppercase && !password.Any(char.IsUpper)) return false;
        if (policy.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit)) return false;
        return true;
    }
}