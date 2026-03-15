using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.ToTable("recipe_steps");

        builder.HasKey(recipeStep => recipeStep.Id);

        builder.Property(recipeStep => recipeStep.Instruction)
            .HasMaxLength(4000)
            .IsRequired();

        builder.HasMany(recipeStep => recipeStep.IngredientReferences)
            .WithOne(reference => reference.RecipeStep)
            .HasForeignKey(reference => reference.RecipeStepId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
