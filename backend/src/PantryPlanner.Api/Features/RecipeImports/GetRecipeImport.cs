using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Common.Results;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed partial class RecipeImportsController
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecipeImportResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeImportResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRecipeImportQuery(User.GetRequiredUserId(), id), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record GetRecipeImportQuery(Guid UserId, Guid RecipeImportId) : IRequest<Result<RecipeImportResponse>>;

public sealed class GetRecipeImportHandler : IRequestHandler<GetRecipeImportQuery, Result<RecipeImportResponse>>
{
    private readonly PantryPlannerDbContext _dbContext;

    public GetRecipeImportHandler(PantryPlannerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<RecipeImportResponse>> Handle(GetRecipeImportQuery request, CancellationToken cancellationToken)
    {
        var recipeImport = await _dbContext.Set<RecipeImport>()
            .SingleOrDefaultAsync(
                importArtifact => importArtifact.Id == request.RecipeImportId && importArtifact.UserId == request.UserId,
                cancellationToken);

        if (recipeImport is null)
        {
            return Result<RecipeImportResponse>.Failure(RecipeImportErrors.NotFound(request.RecipeImportId));
        }

        return Result<RecipeImportResponse>.Success(request.ToResponse(recipeImport));
    }
}

file static class GetRecipeImportQueryMappings
{
    public static RecipeImportResponse ToResponse(this GetRecipeImportQuery _, RecipeImport recipeImport)
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
