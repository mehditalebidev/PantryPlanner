using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class CreateIngredientHandler : IRequestHandler<CreateIngredientCommand, Result<IngredientResponse>>
{
    private readonly IRepository _repository;

    public CreateIngredientHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IngredientResponse>> Handle(CreateIngredientCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = Ingredient.NormalizeName(request.Name);

        var exists = await _repository.Query<Ingredient>()
            .AnyAsync(
                ingredient => ingredient.UserId == request.UserId && ingredient.NormalizedName == normalizedName,
                cancellationToken);

        if (exists)
        {
            return Result<IngredientResponse>.Failure(IngredientErrors.NameAlreadyExists());
        }

        var ingredient = Ingredient.Create(request.UserId, request.Name);
        await _repository.AddAsync(ingredient, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<IngredientResponse>.Success(ingredient.ToResponse());
    }
}
