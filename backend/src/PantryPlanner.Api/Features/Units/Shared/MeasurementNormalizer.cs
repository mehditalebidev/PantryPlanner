namespace PantryPlanner.Api.Features.Units;

public sealed class MeasurementNormalizer : IMeasurementNormalizer
{
    private readonly IUnitCatalog _unitCatalog;

    public MeasurementNormalizer(IUnitCatalog unitCatalog)
    {
        _unitCatalog = unitCatalog;
    }

    public NormalizedMeasurement? Normalize(decimal quantity, string unitCode)
    {
        if (!_unitCatalog.TryGet(unitCode, out var unitDefinition) || unitDefinition is null)
        {
            return null;
        }

        if (!unitDefinition.IsConvertible || unitDefinition.ConversionFactor is null || string.IsNullOrWhiteSpace(unitDefinition.BaseUnitCode))
        {
            return null;
        }

        var normalizedQuantity = decimal.Round(quantity * unitDefinition.ConversionFactor.Value, 6, MidpointRounding.AwayFromZero);
        return new NormalizedMeasurement(normalizedQuantity, unitDefinition.BaseUnitCode);
    }
}
