using FluentValidation;

namespace IbraHabra.NET.Application.UseCases.Users.Commands.LoginUser;

public class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty();
        
        RuleFor(x => x.Password)
            .NotEmpty();
        
        RuleFor(x => x.ClientId)
            .NotEmpty();
    }
}