using FluentValidation;
using IbraHabra.NET.Application.Utils;
using IbraHabra.NET.Infra.Persistent;
using Microsoft.EntityFrameworkCore;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.RegisterUser;

public class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    private readonly AppDbContext _context;

    public RegisterUserValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email cannot be null or empty.")
            .EmailAddress().WithMessage("Must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty();

        RuleFor(x => x.FirstName).MaximumLength(50);
        RuleFor(x => x.LastName).MaximumLength(50);
        RuleFor(x => x.ClientId).NotEmpty();
    }



    
}