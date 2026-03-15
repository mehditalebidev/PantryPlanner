using PantryPlanner.Api.Features.MealPlans;

namespace PantryPlanner.Api.UnitTests.Features.MealPlans.CreateMealPlan;

public sealed class CreateMealPlanCommandValidatorTests
{
    private readonly CreateMealPlanCommandValidator _validator = new();

    [Fact]
    public void Validate_ReturnsNoErrors_ForValidCommand()
    {
        var command = new CreateMealPlanCommand
        {
            Title = "Week of March 16",
            StartDate = new DateOnly(2026, 3, 16),
            EndDate = new DateOnly(2026, 3, 22),
            Slots =
            [
                new MealSlotWriteModel
                {
                    ReferenceKey = "breakfast",
                    Name = "Breakfast",
                    SortOrder = 1,
                    IsDefault = true
                }
            ],
            Entries =
            [
                new PlannedMealWriteModel
                {
                    PlannedDate = new DateOnly(2026, 3, 16),
                    MealSlotReferenceKey = "breakfast",
                    RecipeId = Guid.NewGuid(),
                    ServingsOverride = 6,
                    Note = "Leftovers"
                }
            ]
        };

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_ReturnsExpectedErrors_ForInvalidDatesAndSlotReferences()
    {
        var command = new CreateMealPlanCommand
        {
            Title = "Broken Plan",
            StartDate = new DateOnly(2026, 3, 22),
            EndDate = new DateOnly(2026, 3, 16),
            Slots =
            [
                new MealSlotWriteModel
                {
                    ReferenceKey = "breakfast",
                    Name = "Breakfast",
                    SortOrder = 1
                },
                new MealSlotWriteModel
                {
                    ReferenceKey = "breakfast",
                    Name = "Snack",
                    SortOrder = 2
                }
            ],
            Entries =
            [
                new PlannedMealWriteModel
                {
                    PlannedDate = new DateOnly(2026, 3, 30),
                    MealSlotReferenceKey = "dinner",
                    RecipeId = Guid.NewGuid()
                },
                new PlannedMealWriteModel
                {
                    PlannedDate = new DateOnly(2026, 3, 30),
                    MealSlotReferenceKey = "dinner",
                    RecipeId = Guid.NewGuid()
                }
            ]
        };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "EndDate must be on or after StartDate.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Meal slot reference keys must be unique.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Every planned meal date must fall inside the meal plan range.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Every planned meal must reference a valid meal slot reference key.");
        Assert.Contains(result.Errors, error => error.ErrorMessage == "A meal slot can only have one planned recipe per day.");
    }
}
