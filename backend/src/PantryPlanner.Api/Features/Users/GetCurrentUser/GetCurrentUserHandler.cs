using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Users;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<UserResponse>>
{
    private readonly IRepository _repository;

    public GetCurrentUserHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.Query<User>()
            .Where(candidate => candidate.Id == request.UserId)
            .Select(candidate => candidate.ToResponse())
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result<UserResponse>.Failure(UserErrors.NotFound());
        }

        return Result<UserResponse>.Success(user);
    }
}
