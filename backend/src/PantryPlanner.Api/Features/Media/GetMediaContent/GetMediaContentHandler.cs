using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

public sealed class GetMediaContentHandler : IRequestHandler<GetMediaContentQuery, Result<MediaContentResponse>>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly IRepository _repository;

    public GetMediaContentHandler(IRepository repository, IMediaStorage mediaStorage)
    {
        _repository = repository;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result<MediaContentResponse>> Handle(GetMediaContentQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await _repository.Query<RecipeMediaAsset>()
            .Where(mediaAsset => mediaAsset.StorageKey == request.StorageKey && mediaAsset.Recipe.UserId == request.UserId)
            .Select(mediaAsset => new { mediaAsset.StorageKey, mediaAsset.ContentType })
            .SingleOrDefaultAsync(cancellationToken);

        if (mediaAsset is null || string.IsNullOrWhiteSpace(mediaAsset.ContentType))
        {
            return Result<MediaContentResponse>.Failure(MediaErrors.ContentNotFound(request.StorageKey));
        }

        var stream = await _mediaStorage.OpenReadAsync(request.StorageKey, cancellationToken);
        if (stream is null)
        {
            return Result<MediaContentResponse>.Failure(MediaErrors.ContentNotFound(request.StorageKey));
        }

        return Result<MediaContentResponse>.Success(new MediaContentResponse(stream, mediaAsset.ContentType));
    }
}
