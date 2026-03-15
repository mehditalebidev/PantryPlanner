namespace PantryPlanner.Api.Features.Units;

public sealed record UnitDefinitionResponse(
    string Code,
    string DisplayName,
    string Abbreviation,
    string Family,
    string? BaseUnitCode,
    decimal? ConversionFactor,
    bool IsConvertible);
