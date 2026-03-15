using MediatR;
using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class GetRecipeImportHandler : IRequestHandler<GetRecipeImportQuery, Result<RecipeImportResponse>>
{
    private readonly IRepository _repository;

    public GetRecipeImportHandler(IRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<RecipeImportResponse>> Handle(GetRecipeImportQuery request, CancellationToken cancellationToken)
    {
        var recipeImport = await _repository.Query<RecipeImport>()
            .SingleOrDefaultAsync(
                importArtifact => importArtifact.Id == request.RecipeImportId && importArtifact.UserId == request.UserId,
                cancellationToken);

        if (recipeImport is null)
        {
            return Result<RecipeImportResponse>.Failure(RecipeImportErrors.NotFound(request.RecipeImportId));
        }

        return Result<RecipeImportResponse>.Success(recipeImport.ToResponse());
    }
}
