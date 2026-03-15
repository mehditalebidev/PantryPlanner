namespace PantryPlanner.Api.Features.Units;

public interface IUnitCatalog
{
    IReadOnlyCollection<UnitDefinition> GetAll();

    bool TryGet(string code, out UnitDefinition? unitDefinition);
}
