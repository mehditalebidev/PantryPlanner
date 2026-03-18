using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class PlannedMealConfiguration : IEntityTypeConfiguration<PlannedMeal>
{
    public void Configure(EntityTypeBuilder<PlannedMeal> builder)
    {
        builder.ToTable("planned_meals");

        builder.HasKey(plannedMeal => plannedMeal.Id);

        builder.Property(plannedMeal => plannedMeal.PlannedDate)
            .HasColumnType("date");

        builder.Property(plannedMeal => plannedMeal.Note)
            .HasMaxLength(500);

        builder.HasIndex(plannedMeal => new { plannedMeal.MealPlanId, plannedMeal.PlannedDate, plannedMeal.MealSlotId })
            .IsUnique();

        builder.HasOne(plannedMeal => plannedMeal.MealSlot)
            .WithMany()
            .HasForeignKey(plannedMeal => plannedMeal.MealSlotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(plannedMeal => plannedMeal.Recipe)
            .WithMany()
            .HasForeignKey(plannedMeal => plannedMeal.RecipeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
