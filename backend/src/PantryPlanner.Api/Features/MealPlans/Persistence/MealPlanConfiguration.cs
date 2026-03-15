using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.ToTable("meal_plans");

        builder.HasKey(mealPlan => mealPlan.Id);

        builder.Property(mealPlan => mealPlan.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mealPlan => mealPlan.StartDate)
            .HasColumnType("date");

        builder.Property(mealPlan => mealPlan.EndDate)
            .HasColumnType("date");

        builder.Property(mealPlan => mealPlan.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(mealPlan => mealPlan.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(mealPlan => mealPlan.Slots)
            .WithOne(slot => slot.MealPlan)
            .HasForeignKey(slot => slot.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(mealPlan => mealPlan.Entries)
            .WithOne(entry => entry.MealPlan)
            .HasForeignKey(entry => entry.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(mealPlan => new { mealPlan.UserId, mealPlan.StartDate });
    }
}
