using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class GetIngredientHandler : IRequestHandler<GetIngredientQuery, Result<IngredientResponse>>
{
    private readonly IRepository _repository;

    public GetIngredientHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IngredientResponse>> Handle(GetIngredientQuery request, CancellationToken cancellationToken)
    {
        var ingredient = await _repository.Query<Ingredient>()
            .AsNoTracking()
            .SingleOrDefaultAsync(
                ingredient => ingredient.UserId == request.UserId && ingredient.Id == request.IngredientId,
                cancellationToken);

        return ingredient is null
            ? Result<IngredientResponse>.Failure(IngredientErrors.NotFound(request.IngredientId))
            : Result<IngredientResponse>.Success(ingredient.ToResponse());
    }
}
