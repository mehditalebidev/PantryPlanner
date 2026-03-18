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
    [HttpPut("{id:guid}")]
    [ProducesResponseType<IngredientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientResponse>> Update(
        Guid id,
        [FromBody] UpdateIngredientCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId(), IngredientId = id }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record UpdateIngredientCommand : IRequest<Result<IngredientResponse>>
{
    public Guid UserId { get; init; }

    public Guid IngredientId { get; init; }

    public string Name { get; init; } = string.Empty;
}

public sealed class UpdateIngredientCommandValidator : AbstractValidator<UpdateIngredientCommand>
{
    public UpdateIngredientCommandValidator()
    {
        RuleFor(command => command.IngredientId)
            .NotEmpty()
            .WithMessage("IngredientId is required.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(200)
            .WithMessage("Name must be 200 characters or fewer.");
    }
}

public sealed class UpdateIngredientHandler : IRequestHandler<UpdateIngredientCommand, Result<IngredientResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public UpdateIngredientHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IngredientResponse>> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await _dbContext.Set<Ingredient>()
            .SingleOrDefaultAsync(
                candidate => candidate.UserId == request.UserId && candidate.Id == request.IngredientId,
                cancellationToken);

        if (ingredient is null)
        {
            return Result<IngredientResponse>.Failure(IngredientErrors.NotFound(request.IngredientId));
        }

        var normalizedName = Ingredient.NormalizeName(request.Name);

        var nameInUse = await _dbContext.Set<Ingredient>()
            .AnyAsync(
                candidate => candidate.UserId == request.UserId
                    && candidate.Id != request.IngredientId
                    && candidate.NormalizedName == normalizedName,
                cancellationToken);

        if (nameInUse)
        {
            return Result<IngredientResponse>.Failure(IngredientErrors.NameAlreadyExists());
        }

        ingredient.Rename(request.Name);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<IngredientResponse>.Success(request.ToResponse(ingredient));
    }
}

file static class UpdateIngredientCommandMappings
{
    public static IngredientResponse ToResponse(this UpdateIngredientCommand _, Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.NormalizedName,
            ingredient.CreatedAt,
            ingredient.UpdatedAt);
    }
}
