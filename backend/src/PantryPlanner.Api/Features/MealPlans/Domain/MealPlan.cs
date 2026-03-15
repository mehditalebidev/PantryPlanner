namespace PantryPlanner.Api.Features.MealPlans;

public sealed class MealPlan
{
    private readonly List<MealSlot> _slots = [];
    private readonly List<PlannedMeal> _entries = [];

    private MealPlan()
    {
    }

    private MealPlan(Guid userId, string title, DateOnly startDate, DateOnly endDate)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Title = title.Trim();
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<MealSlot> Slots => _slots;

    public IReadOnlyCollection<PlannedMeal> Entries => _entries;

    public static MealPlan Create(Guid userId, string title, DateOnly startDate, DateOnly endDate)
    {
        return new MealPlan(userId, title, startDate, endDate);
    }

    public void UpdateDetails(string title, DateOnly startDate, DateOnly endDate)
    {
        Title = title.Trim();
        StartDate = startDate;
        EndDate = endDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceSlots(IEnumerable<MealSlot> slots)
    {
        _slots.Clear();
        _slots.AddRange(slots);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceEntries(IEnumerable<PlannedMeal> entries)
    {
        _entries.Clear();
        _entries.AddRange(entries);
        UpdatedAt = DateTime.UtcNow;
    }
}
