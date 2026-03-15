using PantryPlanner.Api.Features.Units;

namespace PantryPlanner.Api.UnitTests.Common.Measurements;

public sealed class MeasurementNormalizerTests
{
    private readonly MeasurementNormalizer _normalizer = new(new InMemoryUnitCatalog());

    [Fact]
    public void Normalize_ReturnsBaseUnitMeasurement_ForConvertibleUnit()
    {
        var normalizedMeasurement = _normalizer.Normalize(2m, "lb");

        Assert.NotNull(normalizedMeasurement);
        Assert.Equal(907.18474m, normalizedMeasurement.Quantity);
        Assert.Equal("g", normalizedMeasurement.UnitCode);
    }

    [Fact]
    public void Normalize_ReturnsNull_ForNonConvertibleUnit()
    {
        var normalizedMeasurement = _normalizer.Normalize(1m, "pinch");

        Assert.Null(normalizedMeasurement);
    }
}
