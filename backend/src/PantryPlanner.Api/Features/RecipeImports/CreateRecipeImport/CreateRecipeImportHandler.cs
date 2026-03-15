using MediatR;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class CreateRecipeImportHandler : IRequestHandler<CreateRecipeImportCommand, Result<RecipeImportResponse>>
{
    private readonly IRecipeImportDraftFactory _draftFactory;
    private readonly IRepository _repository;

    public CreateRecipeImportHandler(IRecipeImportDraftFactory draftFactory, IRepository repository)
    {
        _draftFactory = draftFactory;
        _repository = repository;
    }

    public async Task<Result<RecipeImportResponse>> Handle(CreateRecipeImportCommand request, CancellationToken cancellationToken)
    {
        var buildResult = _draftFactory.CreateFromUrl(request.SourceUrl);
        var recipeImport = RecipeImport.CreateFromUrl(request.UserId, request.SourceUrl, buildResult.Draft, buildResult.Warnings);

        await _repository.AddAsync(recipeImport, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Result<RecipeImportResponse>.Success(recipeImport.ToResponse());
    }
}
