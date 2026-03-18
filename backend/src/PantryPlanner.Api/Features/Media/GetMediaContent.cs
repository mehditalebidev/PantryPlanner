using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Recipes;

namespace PantryPlanner.Api.Features.Media;

public sealed partial class MediaController
{
    [HttpGet("{**storageKey}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string storageKey, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMediaContentQuery(User.GetRequiredUserId(), storageKey), cancellationToken);

        if (result.IsFailure)
        {
            return this.ToProblem(result.Error!);
        }

        return File(result.Value.Content, result.Value.ContentType);
    }
}

public sealed record GetMediaContentQuery(Guid UserId, string StorageKey) : IRequest<Result<MediaContentResponse>>;

public sealed class GetMediaContentHandler : IRequestHandler<GetMediaContentQuery, Result<MediaContentResponse>>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly PantryPlannerDbContext _dbContext;

    public GetMediaContentHandler(PantryPlannerDbContext dbContext, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result<MediaContentResponse>> Handle(GetMediaContentQuery request, CancellationToken cancellationToken)
    {
        var mediaAsset = await _dbContext.Set<RecipeMediaAsset>()
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
