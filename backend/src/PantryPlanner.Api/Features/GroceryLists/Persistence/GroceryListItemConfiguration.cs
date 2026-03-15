using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.GroceryLists;

public sealed class GroceryListItemConfiguration : IEntityTypeConfiguration<GroceryListItem>
{
    public void Configure(EntityTypeBuilder<GroceryListItem> builder)
    {
        builder.ToTable("grocery_list_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(item => item.UnitCode)
            .HasMaxLength(50)
            .IsRequired();
    }
}
