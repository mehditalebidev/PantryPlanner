using FluentValidation;

namespace PantryPlanner.Api.Features.MealPlans;

internal static class MealPlanValidation
{
    public static void ApplyMealPlanRules<T>(AbstractValidator<T> validator)
        where T : IMealPlanUpsertRequest
    {
        validator.RuleFor(command => command.Title)
            .NotEmpty()
            .WithMessage("Title is required.")
            .MaximumLength(200)
            .WithMessage("Title must be 200 characters or fewer.");

        validator.RuleFor(command => command.EndDate)
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("EndDate must be on or after StartDate.");

        validator.RuleFor(command => command.Slots)
            .NotEmpty()
            .WithMessage("At least one meal slot is required.");

        validator.RuleForEach(command => command.Slots)
            .SetValidator(new MealSlotWriteModelValidator());

        validator.RuleFor(command => command.Slots)
            .Must(HaveDistinctSlotReferenceKeys)
            .WithMessage("Meal slot reference keys must be unique.");

        validator.RuleFor(command => command.Slots)
            .Must(HaveDistinctSlotNames)
            .WithMessage("Meal slot names must be unique.");

        validator.RuleFor(command => command.Slots)
            .Must(HaveDistinctSlotSortOrders)
            .WithMessage("Meal slot sort orders must be unique.");

        validator.RuleForEach(command => command.Entries)
            .SetValidator(new PlannedMealWriteModelValidator());

        validator.RuleFor(command => command)
            .Must(HaveEntriesInsideRange)
            .WithMessage("Every planned meal date must fall inside the meal plan range.");

        validator.RuleFor(command => command)
            .Must(HaveValidSlotReferences)
            .WithMessage("Every planned meal must reference a valid meal slot reference key.");

        validator.RuleFor(command => command.Entries)
            .Must(HaveDistinctEntryKeys)
            .WithMessage("A meal slot can only have one planned recipe per day.");
    }

    private static bool HaveDistinctSlotReferenceKeys(IReadOnlyCollection<MealSlotWriteModel> slots)
    {
        return slots.Count == slots
            .Select(slot => slot.ReferenceKey.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private static bool HaveDistinctSlotNames(IReadOnlyCollection<MealSlotWriteModel> slots)
    {
        return slots.Count == slots
            .Select(slot => slot.Name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();
    }

    private static bool HaveDistinctSlotSortOrders(IReadOnlyCollection<MealSlotWriteModel> slots)
    {
        return slots.Count == slots.Select(slot => slot.SortOrder).Distinct().Count();
    }

    private static bool HaveEntriesInsideRange<T>(T command)
        where T : IMealPlanUpsertRequest
    {
        return command.Entries.All(entry => entry.PlannedDate >= command.StartDate && entry.PlannedDate <= command.EndDate);
    }

    private static bool HaveValidSlotReferences<T>(T command)
        where T : IMealPlanUpsertRequest
    {
        var slotReferenceKeys = command.Slots
            .Select(slot => slot.ReferenceKey.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return command.Entries.All(entry => slotReferenceKeys.Contains(entry.MealSlotReferenceKey.Trim()));
    }

    private static bool HaveDistinctEntryKeys(IReadOnlyCollection<PlannedMealWriteModel> entries)
    {
        return entries.Count == entries
            .Select(entry => $"{entry.PlannedDate:yyyy-MM-dd}|{entry.MealSlotReferenceKey.Trim().ToLowerInvariant()}")
            .Distinct(StringComparer.Ordinal)
            .Count();
    }
}

internal sealed class MealSlotWriteModelValidator : AbstractValidator<MealSlotWriteModel>
{
    public MealSlotWriteModelValidator()
    {
        RuleFor(slot => slot.ReferenceKey)
            .NotEmpty()
            .WithMessage("Meal slot reference key is required.")
            .MaximumLength(100)
            .WithMessage("Meal slot reference key must be 100 characters or fewer.");

        RuleFor(slot => slot.Name)
            .NotEmpty()
            .WithMessage("Meal slot name is required.")
            .MaximumLength(100)
            .WithMessage("Meal slot name must be 100 characters or fewer.");

        RuleFor(slot => slot.SortOrder)
            .GreaterThan(0)
            .WithMessage("Meal slot sort order must be greater than 0.");
    }
}

internal sealed class PlannedMealWriteModelValidator : AbstractValidator<PlannedMealWriteModel>
{
    public PlannedMealWriteModelValidator()
    {
        RuleFor(entry => entry.MealSlotReferenceKey)
            .NotEmpty()
            .WithMessage("Meal slot reference key is required.")
            .MaximumLength(100)
            .WithMessage("Meal slot reference key must be 100 characters or fewer.");

        RuleFor(entry => entry.RecipeId)
            .NotEqual(Guid.Empty)
            .WithMessage("RecipeId is required.");

        RuleFor(entry => entry.ServingsOverride)
            .GreaterThan(0)
            .When(entry => entry.ServingsOverride.HasValue)
            .WithMessage("ServingsOverride must be greater than 0 when provided.");

        RuleFor(entry => entry.Note)
            .MaximumLength(500)
            .WithMessage("Note must be 500 characters or fewer.");
    }
}
