using Microsoft.Extensions.Options;

namespace PantryPlanner.Api.Features.Media;

public sealed class LocalMediaStorage : IMediaStorage
{
    private readonly string _storageRoot;

    public LocalMediaStorage(IOptions<MediaOptions> options, IWebHostEnvironment environment)
    {
        var configuredRoot = options.Value.StorageRoot;

        if (string.IsNullOrWhiteSpace(configuredRoot))
        {
            throw new InvalidOperationException("Media:StorageRoot must be configured.");
        }

        _storageRoot = Path.IsPathRooted(configuredRoot)
            ? configuredRoot
            : Path.Combine(environment.ContentRootPath, configuredRoot);
    }

    public async Task<StoredMediaFile> SaveRecipeMediaAsync(
        Guid userId,
        Guid recipeId,
        string contentType,
        string originalFileName,
        Stream content,
        CancellationToken cancellationToken)
    {
        var extension = NormalizeExtension(originalFileName);
        var storageKey = $"recipes/{userId:N}/{recipeId:N}/{Guid.NewGuid():N}{extension}";
        var filePath = GetAbsolutePath(storageKey);
        var directoryPath = Path.GetDirectoryName(filePath)!;

        Directory.CreateDirectory(directoryPath);

        await using var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);

        return new StoredMediaFile(storageKey, MediaUrlBuilder.Build(storageKey), contentType.Trim());
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetAbsolutePath(storageKey);
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var filePath = GetAbsolutePath(storageKey);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string storageKey)
    {
        var normalizedStorageKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
        return Path.Combine(_storageRoot, normalizedStorageKey);
    }

    private static string NormalizeExtension(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName)?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(extension) || extension.Length > 10)
        {
            return string.Empty;
        }

        return extension.All(character => char.IsLetterOrDigit(character) || character == '.')
            ? extension
            : string.Empty;
    }
}
