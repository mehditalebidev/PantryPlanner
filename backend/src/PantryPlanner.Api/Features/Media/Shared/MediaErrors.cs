using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Media;

public static class MediaErrors
{
    public static Error NotFound(Guid mediaId)
    {
        return new Error(
            "recipe_media_not_found",
            "Recipe media was not found.",
            $"Recipe media '{mediaId}' was not found for the current user.",
            StatusCodes.Status404NotFound);
    }

    public static Error ContentNotFound(string storageKey)
    {
        return new Error(
            "media_content_not_found",
            "Media content was not found.",
            $"Media content '{storageKey}' was not found for the current user.",
            StatusCodes.Status404NotFound);
    }
}
