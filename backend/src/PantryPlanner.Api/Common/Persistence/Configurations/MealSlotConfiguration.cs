using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.MealPlans;

public sealed class MealSlotConfiguration : IEntityTypeConfiguration<MealSlot>
{
    public void Configure(EntityTypeBuilder<MealSlot> builder)
    {
        builder.ToTable("meal_slots");

        builder.HasKey(slot => slot.Id);

        builder.Property(slot => slot.ReferenceKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(slot => slot.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(slot => new { slot.MealPlanId, slot.ReferenceKey })
            .IsUnique();

        builder.HasIndex(slot => new { slot.MealPlanId, slot.Name })
            .IsUnique();

        builder.HasIndex(slot => new { slot.MealPlanId, slot.SortOrder })
            .IsUnique();
    }
}
