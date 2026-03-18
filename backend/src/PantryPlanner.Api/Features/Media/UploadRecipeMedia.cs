using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.Recipes;
using Microsoft.AspNetCore.Http;
using PantryPlanner.Api.Common.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.Media;

public sealed partial class RecipeMediaController
{
    [HttpPost]
    [ProducesResponseType<RecipeMediaAssetResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeMediaAssetResponse>> Upload(
        Guid recipeId,
        [FromForm] UploadRecipeMediaCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { RecipeId = recipeId, UserId = User.GetRequiredUserId() }, cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record UploadRecipeMediaCommand : IRequest<Result<RecipeMediaAssetResponse>>
{
    public Guid UserId { get; init; }

    public Guid RecipeId { get; init; }

    public string Kind { get; init; } = string.Empty;

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    public IFormFile File { get; init; } = null!;
}

public sealed class UploadRecipeMediaCommandValidator : AbstractValidator<UploadRecipeMediaCommand>
{
    private static readonly string[] AllowedKinds = ["image", "video"];

    public UploadRecipeMediaCommandValidator()
    {
        RuleFor(command => command.Kind)
            .NotEmpty()
            .WithMessage("Media kind is required.")
            .Must(kind => AllowedKinds.Contains(kind, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Media kind must be image or video.");

        RuleFor(command => command.Caption)
            .MaximumLength(500)
            .WithMessage("Caption must be 500 characters or fewer.");

        RuleFor(command => command.SortOrder)
            .GreaterThan(0)
            .WithMessage("Media sort order must be greater than 0.");

        RuleFor(command => command.File)
            .NotNull()
            .WithMessage("Media file is required.")
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Media file must not be empty.");

        RuleFor(command => command)
            .Custom((command, context) =>
            {
                if (command.File is null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(command.File.ContentType))
                {
                    context.AddFailure(nameof(command.File), "Media file content type is required.");
                    return;
                }

                if (command.Kind.Equals("image", StringComparison.OrdinalIgnoreCase)
                    && !command.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    context.AddFailure(nameof(command.File), "Media file content type must match the image kind.");
                }

                if (command.Kind.Equals("video", StringComparison.OrdinalIgnoreCase)
                    && !command.File.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                {
                    context.AddFailure(nameof(command.File), "Media file content type must match the video kind.");
                }
            });
    }
}

public sealed class UploadRecipeMediaHandler : IRequestHandler<UploadRecipeMediaCommand, Result<RecipeMediaAssetResponse>>
{
    private readonly IMediaStorage _mediaStorage;
    private readonly PantryPlannerDbContext _dbContext;

    public UploadRecipeMediaHandler(PantryPlannerDbContext dbContext, IMediaStorage mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    public async Task<Result<RecipeMediaAssetResponse>> Handle(UploadRecipeMediaCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _dbContext.Set<Recipe>()
            .SingleOrDefaultAsync(recipe => recipe.UserId == request.UserId && recipe.Id == request.RecipeId, cancellationToken);

        if (recipe is null)
        {
            return Result<RecipeMediaAssetResponse>.Failure(RecipeErrors.NotFound(request.RecipeId));
        }

        await using var fileStream = request.File.OpenReadStream();
        var storedMedia = await _mediaStorage.SaveRecipeMediaAsync(
            request.UserId,
            request.RecipeId,
            request.File.ContentType,
            request.File.FileName,
            fileStream,
            cancellationToken);

        var mediaAsset = RecipeMediaAsset.Create(
            request.Kind,
            storedMedia.StorageKey,
            storedMedia.Url,
            storedMedia.ContentType,
            request.Caption,
            request.SortOrder);

        recipe.AddMediaAsset(mediaAsset);
        _dbContext.Entry(mediaAsset).State = EntityState.Added;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await _mediaStorage.DeleteIfExistsAsync(storedMedia.StorageKey, cancellationToken);
            throw;
        }

        return Result<RecipeMediaAssetResponse>.Success(request.ToResponse(mediaAsset));
    }
}

file static class UploadRecipeMediaCommandMappings
{
    public static RecipeMediaAssetResponse ToResponse(this UploadRecipeMediaCommand _, RecipeMediaAsset mediaAsset)
    {
        return new RecipeMediaAssetResponse(
            mediaAsset.Id,
            mediaAsset.Kind,
            mediaAsset.StorageKey,
            mediaAsset.Url,
            mediaAsset.ContentType,
            mediaAsset.Caption,
            mediaAsset.SortOrder);
    }
}
