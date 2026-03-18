using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Security;

namespace PantryPlanner.Api.Features.Users;

public sealed partial class AuthController
{
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record LoginCommand : IRequest<Result<AuthResponse>>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(200)
            .WithMessage("Email must be 200 characters or fewer.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be between 8 and 100 characters.")
            .MaximumLength(100)
            .WithMessage("Password must be between 8 and 100 characters.");
    }
}

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public LoginHandler(
        PantryPlannerDbContext dbContext,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);

        var user = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(candidate => candidate.Email == normalizedEmail, cancellationToken);

        if (user is null || !_passwordService.VerifyPassword(user, request.Password))
        {
            return Result<AuthResponse>.Failure(UserErrors.InvalidCredentials());
        }

        var issuedToken = _tokenService.CreateAccessToken(user);

        return Result<AuthResponse>.Success(request.ToResponse(user, issuedToken));
    }
}

file static class LoginCommandMappings
{
    public static AuthResponse ToResponse(this LoginCommand _, User user, IssuedToken issuedToken)
    {
        return new AuthResponse(
            issuedToken.AccessToken,
            issuedToken.ExpiresAt,
            new UserResponse(user.Id, user.Email, user.DisplayName));
    }
}
