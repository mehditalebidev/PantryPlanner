using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.ToTable("recipe_ingredients");

        builder.HasKey(recipeIngredient => recipeIngredient.Id);

        builder.Property(recipeIngredient => recipeIngredient.ReferenceKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(recipeIngredient => new { recipeIngredient.RecipeId, recipeIngredient.ReferenceKey })
            .IsUnique();

        builder.Property(recipeIngredient => recipeIngredient.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(recipeIngredient => recipeIngredient.UnitCode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(recipeIngredient => recipeIngredient.NormalizedQuantity)
            .HasPrecision(18, 6);

        builder.Property(recipeIngredient => recipeIngredient.NormalizedUnitCode)
            .HasMaxLength(50);

        builder.Property(recipeIngredient => recipeIngredient.PreparationNote)
            .HasMaxLength(500);

        builder.HasOne(recipeIngredient => recipeIngredient.Ingredient)
            .WithMany()
            .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
