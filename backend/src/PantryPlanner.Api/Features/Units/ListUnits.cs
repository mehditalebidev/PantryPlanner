using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Units;

public sealed partial class UnitsController
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<UnitDefinitionResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyCollection<UnitDefinitionResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListUnitsQuery(), cancellationToken);
        return result.ToActionResult(this);
    }
}

public sealed record ListUnitsQuery : IRequest<Result<IReadOnlyCollection<UnitDefinitionResponse>>>;

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
            .Select(unit => request.ToResponse(unit))
            .ToArray();

        return Task.FromResult(Result<IReadOnlyCollection<UnitDefinitionResponse>>.Success(units));
    }
}

file static class ListUnitsQueryMappings
{
    public static UnitDefinitionResponse ToResponse(this ListUnitsQuery _, UnitDefinition unitDefinition)
    {
        return new UnitDefinitionResponse(
            unitDefinition.Code,
            unitDefinition.DisplayName,
            unitDefinition.Abbreviation,
            unitDefinition.Family,
            unitDefinition.BaseUnitCode,
            unitDefinition.ConversionFactor,
            unitDefinition.IsConvertible);
    }
}
