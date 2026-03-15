namespace PantryPlanner.Api.Features.Units;

public sealed record UnitDefinition(
    string Code,
    string DisplayName,
    string Abbreviation,
    string Family,
    string? BaseUnitCode,
    decimal? ConversionFactor,
    bool IsConvertible);
