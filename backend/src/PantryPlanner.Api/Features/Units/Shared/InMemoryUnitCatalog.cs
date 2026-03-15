namespace PantryPlanner.Api.Features.Units;

public sealed class InMemoryUnitCatalog : IUnitCatalog
{
    private static readonly IReadOnlyCollection<UnitDefinition> Units =
    [
        new("g", "Gram", "g", "mass", "g", 1m, true),
        new("kg", "Kilogram", "kg", "mass", "g", 1000m, true),
        new("mg", "Milligram", "mg", "mass", "g", 0.001m, true),
        new("oz", "Ounce", "oz", "mass", "g", 28.349523125m, true),
        new("lb", "Pound", "lb", "mass", "g", 453.59237m, true),
        new("ml", "Milliliter", "mL", "volume", "ml", 1m, true),
        new("l", "Liter", "L", "volume", "ml", 1000m, true),
        new("tsp", "Teaspoon", "tsp", "volume", "ml", 4.92892159375m, true),
        new("tbsp", "Tablespoon", "tbsp", "volume", "ml", 14.78676478125m, true),
        new("cup", "Cup", "cup", "volume", "ml", 236.5882365m, true),
        new("floz", "Fluid ounce", "fl oz", "volume", "ml", 29.5735295625m, true),
        new("piece", "Piece", "pc", "count", "piece", 1m, true),
        new("dozen", "Dozen", "doz", "count", "piece", 12m, true),
        new("pinch", "Pinch", "pinch", "custom", null, null, false),
        new("dash", "Dash", "dash", "custom", null, null, false),
        new("bunch", "Bunch", "bunch", "custom", null, null, false),
        new("package", "Package", "pkg", "custom", null, null, false)
    ];

    private static readonly IReadOnlyDictionary<string, UnitDefinition> UnitsByCode = Units
        .ToDictionary(unit => unit.Code, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<UnitDefinition> GetAll()
    {
        return Units;
    }

    public bool TryGet(string code, out UnitDefinition? unitDefinition)
    {
        return UnitsByCode.TryGetValue(code.Trim(), out unitDefinition);
    }
}
