namespace PantryPlanner.Api.Features.Media;

public sealed class MediaOptions
{
    public const string SectionName = "Media";

    public string StorageRoot { get; init; } = "App_Data/Media";
}
