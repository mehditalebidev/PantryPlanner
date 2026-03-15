namespace PantryPlanner.Api.Features.Units;

public interface IMeasurementNormalizer
{
    NormalizedMeasurement? Normalize(decimal quantity, string unitCode);
}
