using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

public sealed class DeleteRecipeMediaHandler : IRequestHandler<DeleteRecipeMediaCommand, Result>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly IRepository _repository;

    public DeleteRecipeMediaHandler(IRepository repository, IMediaStorage mediaStorage)
    {
        _repository = repository;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result> Handle(DeleteRecipeMediaCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.Query<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.Id == request.RecipeId && recipe.UserId == request.UserId, cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        var mediaAsset = await _repository.Query<RecipeMediaAsset>()
            .SingleOrDefaultAsync(mediaAsset => mediaAsset.Id == request.MediaId && mediaAsset.RecipeId == request.RecipeId, cancellationToken);

        if (mediaAsset is null)
        {
            return Result.Failure(MediaErrors.NotFound(request.MediaId));
        }

        var storageKey = mediaAsset.StorageKey;

        _repository.Remove(mediaAsset);
        await _repository.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(storageKey))
        {
            await _mediaStorage.DeleteIfExistsAsync(storageKey, cancellationToken);
        }

        return Result.Success();
    }
}
