using MediatR;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class CreateRecipeHandler : IRequestHandler<CreateRecipeCommand, Result<RecipeResponse>>
{
    private readonly IRecipeContentFactory _recipeContentFactory;
    private readonly IRepository _repository;

    public CreateRecipeHandler(IRepository repository, IRecipeContentFactory recipeContentFactory)
    {
        _repository = repository;
        _recipeContentFactory = recipeContentFactory;
    }

    public async Task<Result<RecipeResponse>> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = Recipe.Create(
            request.UserId,
            request.Title,
            request.Description,
            request.Servings,
            request.PrepTimeMinutes,
            request.CookTimeMinutes,
            request.SourceUrl);

        var contentResult = await _recipeContentFactory.BuildAsync(
            request.UserId,
            request.Ingredients,
            request.Steps,
            request.Media,
            cancellationToken);

        if (contentResult.IsFailure)
        {
            return Result<RecipeResponse>.Failure(contentResult.Error!);
        }

        recipe.ReplaceIngredients(contentResult.Value.Ingredients);
        recipe.ReplaceSteps(contentResult.Value.Steps);
        recipe.ReplaceMediaAssets(contentResult.Value.MediaAssets);

        await _repository.AddAsync(recipe, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<RecipeResponse>.Success(recipe.ToResponse());
    }
}
