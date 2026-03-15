using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class UpdateIngredientHandler : IRequestHandler<UpdateIngredientCommand, Result<IngredientResponse>>
{
    private readonly IRepository _repository;

    public UpdateIngredientHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IngredientResponse>> Handle(UpdateIngredientCommand request, CancellationToken cancellationToken)
    {
        var ingredient = await _repository.Query<Ingredient>()
            .SingleOrDefaultAsync(
                candidate => candidate.UserId == request.UserId && candidate.Id == request.IngredientId,
                cancellationToken);

        if (ingredient is null)
        {
            return Result<IngredientResponse>.Failure(IngredientErrors.NotFound(request.IngredientId));
        }

        var normalizedName = Ingredient.NormalizeName(request.Name);

        var nameInUse = await _repository.Query<Ingredient>()
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
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<IngredientResponse>.Success(ingredient.ToResponse());
    }
}
