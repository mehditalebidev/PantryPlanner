namespace PantryPlanner.Api.Features.GroceryLists;

public sealed record GroceryListResponse(
    Guid Id,
    Guid MealPlanId,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime GeneratedAt,
    IReadOnlyCollection<GroceryListItemResponse> Items);

public sealed record GroceryListItemResponse(
    Guid Id,
    Guid? IngredientId,
    string Name,
    decimal Quantity,
    string UnitCode,
    bool IsChecked,
    int SourceCount);
