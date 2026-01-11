using AuthServerSimple.Dtos.Requests;
using FluentValidation;

namespace AuthServerSimple.Application.Validation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$").WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one capital letter.")
            .Matches(@"[^0-9]").WithMessage("Password must contain at least one non-numerical character.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");
    }
}
