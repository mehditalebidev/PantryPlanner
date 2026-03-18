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
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<IngredientResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<IngredientResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListIngredientsQuery(User.GetRequiredUserId()), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record ListIngredientsQuery(Guid UserId) : IRequest<Result<IReadOnlyCollection<IngredientResponse>>>;

public sealed class ListIngredientsHandler : IRequestHandler<ListIngredientsQuery, Result<IReadOnlyCollection<IngredientResponse>>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public ListIngredientsHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IReadOnlyCollection<IngredientResponse>>> Handle(ListIngredientsQuery request, CancellationToken cancellationToken)
    {
        var ingredients = await _dbContext.Set<Ingredient>()
            .AsNoTracking()
            .Where(ingredient => ingredient.UserId == request.UserId)
            .OrderBy(ingredient => ingredient.Name)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyCollection<IngredientResponse>>.Success(request.ToResponse(ingredients));
    }
}

file static class ListIngredientsQueryMappings
{
    public static IReadOnlyCollection<IngredientResponse> ToResponse(this ListIngredientsQuery _, IEnumerable<Ingredient> ingredients)
    {
        return ingredients
            .Select(ingredient => new IngredientResponse(
                ingredient.Id,
                ingredient.Name,
                ingredient.NormalizedName,
                ingredient.CreatedAt,
                ingredient.UpdatedAt))
            .ToArray();
    }
}
