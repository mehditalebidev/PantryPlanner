namespace PantryPlanner.Api.Features.Units;

public static class UnitMappings
{
    public static UnitDefinitionResponse ToResponse(this UnitDefinition unitDefinition)
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
