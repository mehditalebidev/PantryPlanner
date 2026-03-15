using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Units;

public sealed class ListUnitsHandler : IRequestHandler<ListUnitsQuery, Result<IReadOnlyCollection<UnitDefinitionResponse>>>
{
    private readonly IUnitCatalog _unitCatalog;

    public ListUnitsHandler(IUnitCatalog unitCatalog)
    {
        _unitCatalog = unitCatalog;
    }

    public Task<Result<IReadOnlyCollection<UnitDefinitionResponse>>> Handle(ListUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = _unitCatalog.GetAll()
            .OrderBy(unit => unit.Family)
            .ThenBy(unit => unit.DisplayName)
            .Select(unit => unit.ToResponse())
            .ToArray();

        return Task.FromResult(Result<IReadOnlyCollection<UnitDefinitionResponse>>.Success(units));
    }
}
