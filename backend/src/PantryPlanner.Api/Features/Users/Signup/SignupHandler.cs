using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.Users;

public sealed class SignupHandler : IRequestHandler<SignupCommand, Result<AuthResponse>>
{
    private readonly IRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public SignupHandler(
        IRepository repository,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _repository = repository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);

        var emailInUse = await _repository.Query<User>()
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailInUse)
        {
            return Result<AuthResponse>.Failure(UserErrors.EmailInUse());
        }

        var user = User.Create(normalizedEmail, request.DisplayName);
        user.SetPasswordHash(_passwordService.HashPassword(user, request.Password));

        await _repository.AddAsync(user, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var issuedToken = _tokenService.CreateAccessToken(user);

        return Result<AuthResponse>.Success(new AuthResponse(
            issuedToken.AccessToken,
            issuedToken.ExpiresAt,
            user.ToResponse()));
    }
}
