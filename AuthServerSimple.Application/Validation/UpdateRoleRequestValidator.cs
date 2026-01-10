using AuthServerSimple.Dtos.Requests;
using FluentValidation;

namespace AuthServerSimple.Application.Validation;

public class UpdateRoleRequestValidator : AbstractValidator<UpdateRoleRequest>
{
    public UpdateRoleRequestValidator()
    {
        RuleFor(x => x.OldRoleName)
            .NotEmpty().WithMessage("Old role name is required.");

        RuleFor(x => x.NewRoleName)
            .NotEmpty().WithMessage("New role name is required.")
            .MaximumLength(100).WithMessage("New role name must not exceed 100 characters.");
    }
}
