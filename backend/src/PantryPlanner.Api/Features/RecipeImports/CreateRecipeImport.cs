using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed partial class RecipeImportsController
{
    [HttpPost]
    [ProducesResponseType<RecipeImportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RecipeImportResponse>> Create(
        [FromBody] CreateRecipeImportCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record CreateRecipeImportCommand : IRequest<Result<RecipeImportResponse>>
{
    public Guid UserId { get; init; }

    public string SourceUrl { get; init; } = string.Empty;
}

public sealed class CreateRecipeImportCommandValidator : AbstractValidator<CreateRecipeImportCommand>
{
    public CreateRecipeImportCommandValidator()
    {
        RuleFor(command => command.SourceUrl)
            .NotEmpty()
            .WithMessage("SourceUrl is required.")
            .MaximumLength(1000)
            .WithMessage("SourceUrl must be 1000 characters or fewer.")
            .Must(sourceUrl => Uri.TryCreate(sourceUrl, UriKind.Absolute, out _))
            .WithMessage("SourceUrl must be a valid absolute URL.");
    }
}

public sealed class CreateRecipeImportHandler : IRequestHandler<CreateRecipeImportCommand, Result<RecipeImportResponse>>
{
    private readonly IRecipeImportDraftFactory _draftFactory;
    private readonly PantryPlannerDbContext _dbContext;

    public CreateRecipeImportHandler(IRecipeImportDraftFactory draftFactory, PantryPlannerDbContext dbContext)
    {
        _draftFactory = draftFactory;
        _dbContext = dbContext;
    }

    public async Task<Result<RecipeImportResponse>> Handle(CreateRecipeImportCommand request, CancellationToken cancellationToken)
    {
        var buildResult = _draftFactory.CreateFromUrl(request.SourceUrl);
        var recipeImport = RecipeImport.CreateFromUrl(request.UserId, request.SourceUrl, buildResult.Draft, buildResult.Warnings);

        await _dbContext.AddAsync(recipeImport, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<RecipeImportResponse>.Success(request.ToResponse(recipeImport));
    }
}

file static class CreateRecipeImportCommandMappings
{
    public static RecipeImportResponse ToResponse(this CreateRecipeImportCommand _, RecipeImport recipeImport)
    {
        return new RecipeImportResponse(
            recipeImport.Id,
            recipeImport.SourceType,
            recipeImport.SourceUrl,
            recipeImport.Status,
            recipeImport.GetDraft(),
            recipeImport.GetWarnings(),
            recipeImport.CreatedAt,
            recipeImport.UpdatedAt);
    }
}
