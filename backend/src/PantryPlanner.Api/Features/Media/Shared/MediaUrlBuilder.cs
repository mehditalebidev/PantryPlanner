namespace PantryPlanner.Api.Features.Media;

public static class MediaUrlBuilder
{
    public static string Build(string storageKey)
    {
        return $"/api/v1/media/{storageKey}";
    }
}
