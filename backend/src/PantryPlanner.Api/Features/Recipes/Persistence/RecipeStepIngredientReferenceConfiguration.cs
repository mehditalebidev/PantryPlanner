using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeStepIngredientReferenceConfiguration : IEntityTypeConfiguration<RecipeStepIngredientReference>
{
    public void Configure(EntityTypeBuilder<RecipeStepIngredientReference> builder)
    {
        builder.ToTable("recipe_step_ingredient_references");

        builder.HasKey(reference => reference.Id);

        builder.HasIndex(reference => new { reference.RecipeStepId, reference.RecipeIngredientId })
            .IsUnique();

        builder.HasOne(reference => reference.RecipeIngredient)
            .WithMany()
            .HasForeignKey(reference => reference.RecipeIngredientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
