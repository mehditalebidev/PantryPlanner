using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed partial class IngredientsController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<IngredientResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetIngredientQuery(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GetIngredientQuery(Guid UserId, Guid IngredientId) : IRequest<Result<IngredientResponse>>;

public sealed class GetIngredientHandler : IRequestHandler<GetIngredientQuery, Result<IngredientResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetIngredientHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IngredientResponse>> Handle(GetIngredientQuery request, CancellationToken cancellationToken)
    {
        var ingredient = await _dbContext.Set<Ingredient>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                ingredient => ingredient.UserId == request.UserId && ingredient.Id == request.IngredientId,
                cancellationToken);

        return ingredient is null
            ? Result<IngredientResponse>.Failure(IngredientErrors.NotFound(request.IngredientId))
            : Result<IngredientResponse>.Success(request.ToResponse(ingredient));
    }
}

file static class GetIngredientQueryMappings
{
    public static IngredientResponse ToResponse(this GetIngredientQuery _, Ingredient ingredient)
    {
        return new IngredientResponse(
            ingredient.Id,
            ingredient.Name,
            ingredient.NormalizedName,
            ingredient.CreatedAt,
            ingredient.UpdatedAt);
    }
}
