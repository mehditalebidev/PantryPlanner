using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed partial class IngredientsController
{
    [HttpPost]
    [ProducesResponseType<IngredientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientResponse>> Create([FromBody] CreateIngredientCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateIngredientCommand : IRequest<Result<IngredientResponse>>
{
    public Guid UserId { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class CreateIngredientCommandValidator : AbstractValidator<CreateIngredientCommand>
{
    public CreateIngredientCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or fewer.");
    }
}

public sealed class CreateIngredientHandler : IRequestHandler<CreateIngredientCommand, Result<IngredientResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public CreateIngredientHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IngredientResponse>> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = Ingredient.NormalizeName(request.Name);

        var exists = await _dbContext.Set<Ingredient>()
            .AnyAsync(
                ingredient => ingredient.UserId == request.UserId && ingredient.NormalizedName == normalizedName,
                cancellationToken);

        if (exists)
        {
            return Result<IngredientResponse>.Failure(IngredientErrors.NameAlreadyExists());
        }

        var ingredient = Ingredient.Create(request.UserId, request.Name);
        await _dbContext.AddAsync(ingredient, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<IngredientResponse>.Success(request.ToResponse(ingredient));
    }
}

file static class CreateIngredientCommandMappings
{
    public static IngredientResponse ToResponse(this CreateIngredientCommand _, Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.NormalizedName,
            ingredient.CreatedAt,
            ingredient.UpdatedAt);
    }
}
