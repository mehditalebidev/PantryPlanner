using MediatR;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.Features.Units;

public sealed record ListUnitsQuery : IRequest<Result<IReadOnlyCollection<UnitDefinitionResponse>>>;
