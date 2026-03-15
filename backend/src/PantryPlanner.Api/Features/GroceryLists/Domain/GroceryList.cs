namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GroceryList
{
    private readonly List<GroceryListItem> _items = [];

    private GroceryList()
    {
    }

    private GroceryList(Guid userId, Guid mealPlanId, DateOnly startDate, DateOnly endDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        MealPlanId = mealPlanId;
        StartDate = startDate;
        EndDate = endDate;
        GeneratedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid MealPlanId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public DateTime GeneratedAt { get; private set; }

    public IReadOnlyCollection<GroceryListItem> Items => _items;

    public static GroceryList Create(Guid userId, Guid mealPlanId, DateOnly startDate, DateOnly endDate)
    {
        return new GroceryList(userId, mealPlanId, startDate, endDate);
    }

    public void ReplaceItems(IEnumerable<GroceryListItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
    }
}
