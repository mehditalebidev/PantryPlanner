using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Users;

public sealed partial class UsersController
{
    [HttpGet("me")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery(User.GetRequiredUserId());
        var result = await _sender.Send(query, cancellationToken);

        return result.ToActionResult(this);
    }
}

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserResponse>>;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetCurrentUserHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            return Result<UserResponse>.Failure(UserErrors.NotFound());
        }

        return Result<UserResponse>.Success(request.ToResponse(user));
    }
}

file static class GetCurrentUserQueryMappings
{
    public static UserResponse ToResponse(this GetCurrentUserQuery _, User user)
    {
        return new UserResponse(user.Id, user.Email, user.DisplayName);
    }
}
