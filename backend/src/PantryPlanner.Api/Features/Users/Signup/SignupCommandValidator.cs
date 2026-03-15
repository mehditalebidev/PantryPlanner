using FluentValidation;

namespace PantryPlanner.Api.Features.Users;

public sealed class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(200)
            .WithMessage("Email must be 200 characters or fewer.");

        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required.")
            .MinimumLength(2)
            .WithMessage("DisplayName must be between 2 and 100 characters.")
            .MaximumLength(100)
            .WithMessage("DisplayName must be between 2 and 100 characters.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be between 8 and 100 characters.")
            .MaximumLength(100)
            .WithMessage("Password must be between 8 and 100 characters.");
    }
}
