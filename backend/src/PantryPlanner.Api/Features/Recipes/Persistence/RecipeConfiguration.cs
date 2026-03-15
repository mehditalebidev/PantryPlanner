using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("recipes");

        builder.HasKey(recipe => recipe.Id);

        builder.Property(recipe => recipe.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(recipe => recipe.Description)
            .HasMaxLength(2000);

        builder.Property(recipe => recipe.SourceUrl)
            .HasMaxLength(500);

        builder.Property(recipe => recipe.CreatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(recipe => recipe.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasMany(recipe => recipe.Ingredients)
            .WithOne(recipeIngredient => recipeIngredient.Recipe)
            .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(recipe => recipe.Steps)
            .WithOne(recipeStep => recipeStep.Recipe)
            .HasForeignKey(recipeStep => recipeStep.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(recipe => recipe.MediaAssets)
            .WithOne(mediaAsset => mediaAsset.Recipe)
            .HasForeignKey(mediaAsset => mediaAsset.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(recipe => new { recipe.UserId, recipe.Title });
    }
}
