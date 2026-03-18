using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GroceryListConfiguration : IEntityTypeConfiguration<GroceryList>
{
    public void Configure(EntityTypeBuilder<GroceryList> builder)
    {
        builder.ToTable("grocery_lists");

        builder.HasKey(groceryList => groceryList.Id);

        builder.Property(groceryList => groceryList.StartDate)
            .HasColumnType("date");

        builder.Property(groceryList => groceryList.EndDate)
            .HasColumnType("date");

        builder.Property(groceryList => groceryList.GeneratedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(groceryList => groceryList.Items)
            .WithOne(item => item.GroceryList)
            .HasForeignKey(item => item.GroceryListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(groceryList => new { groceryList.UserId, groceryList.GeneratedAt });
    }
}
