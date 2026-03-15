namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GroceryListItem
{
    private GroceryListItem()
    {
    }

    private GroceryListItem(Guid? ingredientId, string name, decimal quantity, string unitCode, int sourceCount)
    {
        Id = Guid.NewGuid();
        IngredientId = ingredientId;
        Name = name.Trim();
        Quantity = quantity;
        UnitCode = unitCode.Trim();
        SourceCount = sourceCount;
        IsChecked = false;
    }

    public Guid Id { get; private set; }

    public Guid GroceryListId { get; private set; }

    public GroceryList GroceryList { get; private set; } = null!;

    public Guid? IngredientId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public string UnitCode { get; private set; } = string.Empty;

    public bool IsChecked { get; private set; }

    public int SourceCount { get; private set; }

    public static GroceryListItem Create(Guid? ingredientId, string name, decimal quantity, string unitCode, int sourceCount)
    {
        return new GroceryListItem(ingredientId, name, quantity, unitCode, sourceCount);
    }

    public void SetChecked(bool isChecked)
    {
        IsChecked = isChecked;
    }
}
