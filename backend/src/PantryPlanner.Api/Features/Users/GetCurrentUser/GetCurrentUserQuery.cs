using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Users;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserResponse>>;
