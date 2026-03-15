namespace PantryPlanner.Api.Features.Media;

public interface IMediaStorage
{
    Task<StoredMediaFile> SaveRecipeMediaAsync(
        Guid userId,
        Guid recipeId,
        string contentType,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken);

    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken);

    Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken);
}
