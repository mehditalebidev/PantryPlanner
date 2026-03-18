using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Ingredients;

namespace PantryPlanner.Api.Features.Users;

public sealed partial class AuthController
{
    [HttpPost("signup")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Signup(
        [FromBody] SignupCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record SignupCommand : IRequest<Result<AuthResponse>>
{
    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed class SignupCommandValidator : AbstractValidator<SignupCommand>
{
    public SignupCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(200)
            .WithMessage("Email must be 200 characters or fewer.");

        RuleFor(command => command.DisplayName)
            .NotEmpty()
            .WithMessage("DisplayName is required.")
            .MinimumLength(2)
            .WithMessage("DisplayName must be between 2 and 100 characters.")
            .MaximumLength(100)
            .WithMessage("DisplayName must be between 2 and 100 characters.");

        RuleFor(command => command.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be between 8 and 100 characters.")
            .MaximumLength(100)
            .WithMessage("Password must be between 8 and 100 characters.");
    }
}

public sealed class SignupHandler : IRequestHandler<SignupCommand, Result<AuthResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;
    private readonly IIngredientCatalogSeeder _ingredientCatalogSeeder;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public SignupHandler(
        PantryPlannerDbContext dbContext,
        IIngredientCatalogSeeder ingredientCatalogSeeder,
        IPasswordService passwordService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _ingredientCatalogSeeder = ingredientCatalogSeeder;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> Handle(SignupCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = User.NormalizeEmail(request.Email);

        var emailInUse = await _dbContext.Set<User>()
            .AnyAsync(user => user.Email == normalizedEmail, cancellationToken);

        if (emailInUse)
        {
            return Result<AuthResponse>.Failure(UserErrors.EmailInUse());
        }

        var user = User.Create(normalizedEmail, request.DisplayName);
        user.SetPasswordHash(_passwordService.HashPassword(user, request.Password));

        await _dbContext.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await _ingredientCatalogSeeder.SeedDefaultsForUserAsync(user.Id, cancellationToken);

        var issuedToken = _tokenService.CreateAccessToken(user);

        return Result<AuthResponse>.Success(request.ToResponse(user, issuedToken));
    }
}

file static class SignupCommandMappings
{
    public static AuthResponse ToResponse(this SignupCommand _, User user, IssuedToken issuedToken)
    {
        return new AuthResponse(
            issuedToken.AccessToken,
            issuedToken.ExpiresAt,
            new UserResponse(user.Id, user.Email, user.DisplayName));
    }
}
