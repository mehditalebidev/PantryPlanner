using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Media;

public sealed record GetMediaContentQuery(Guid UserId, string StorageKey) : IRequest<Result<MediaContentResponse>>;
