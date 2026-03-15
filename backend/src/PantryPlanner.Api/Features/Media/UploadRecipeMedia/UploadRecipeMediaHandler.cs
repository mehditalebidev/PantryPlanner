using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

public sealed class UploadRecipeMediaHandler : IRequestHandler<UploadRecipeMediaCommand, Result<RecipeMediaAssetResponse>>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly IRepository _repository;

    public UploadRecipeMediaHandler(IRepository repository, IMediaStorage mediaStorage)
    {
        _repository = repository;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result<RecipeMediaAssetResponse>> Handle(UploadRecipeMediaCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _repository.Query<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeMediaAssetResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        await using var fileStream = request.File.OpenReadStream();
        var storedMedia = await _mediaStorage.SaveRecipeMediaAsync(
            request.UserId,
            request.RecipeId,
            request.File.ContentType,
            request.File.FileName,
            fileStream,
            cancellationToken);

        var mediaAsset = RecipeMediaAsset.Create(
            request.Kind,
            storedMedia.StorageKey,
            storedMedia.Url,
            storedMedia.ContentType,
            request.Caption,
            request.SortOrder);

        recipe.AddMediaAsset(mediaAsset);
        _repository.DbContext.Entry(mediaAsset).State = EntityState.Added;

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _mediaStorage.DeleteIfExistsAsync(storedMedia.StorageKey, cancellationToken);
            throw;
        }

        return Result<RecipeMediaAssetResponse>.Success(mediaAsset.ToResponse());
    }
}
