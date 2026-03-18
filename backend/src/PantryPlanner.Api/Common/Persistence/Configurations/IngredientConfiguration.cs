using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Ingredients;

public sealed class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("ingredients");

        builder.HasKey(ingredient => ingredient.Id);

        builder.Property(ingredient => ingredient.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ingredient => ingredient.NormalizedName)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(ingredient => new { ingredient.UserId, ingredient.NormalizedName })
            .IsUnique();

        builder.Property(ingredient => ingredient.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(ingredient => ingredient.UpdatedAt)
            .HasColumnType("timestamp with time zone");
    }
}
