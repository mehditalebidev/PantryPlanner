using FluentValidation;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class CreateRecipeImportCommandValidator : AbstractValidator<CreateRecipeImportCommand>
{
    public CreateRecipeImportCommandValidator()
    {
        RuleFor(command => command.SourceUrl)
            .NotEmpty()
            .WithMessage("SourceUrl is required.")
            .MaximumLength(1000)
            .WithMessage("SourceUrl must be 1000 characters or fewer.")
            .Must(sourceUrl => Uri.TryCreate(sourceUrl, UriKind.Absolute, out _))
            .WithMessage("SourceUrl must be a valid absolute URL.");
    }
}
