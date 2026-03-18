namespace PantryPlanner.Api.Features.MealPlans;

public sealed class MealSlot
{
    private MealSlot()
    {
    }

    private MealSlot(string referenceKey, string name, int sortOrder, bool isDefault)
    {
        Id = Guid.NewGuid();
        ReferenceKey = referenceKey.Trim();
        Name = name.Trim();
        SortOrder = sortOrder;
        IsDefault = isDefault;
    }

    public Guid Id { get; private set; }

    public Guid MealPlanId { get; private set; }

    public MealPlan MealPlan { get; private set; } = null!;

    public string ReferenceKey { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }

    public bool IsDefault { get; private set; }

    public static MealSlot Create(string referenceKey, string name, int sortOrder, bool isDefault)
    {
        return new MealSlot(referenceKey, name, sortOrder, isDefault);
    }
}
