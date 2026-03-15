using FluentValidation;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class UpdateGroceryListItemCommandValidator : AbstractValidator<UpdateGroceryListItemCommand>
{
    public UpdateGroceryListItemCommandValidator()
    {
        RuleFor(command => command.GroceryListItemId)
            .NotEqual(Guid.Empty)
            .WithMessage("GroceryListItemId is required.");
    }
}
