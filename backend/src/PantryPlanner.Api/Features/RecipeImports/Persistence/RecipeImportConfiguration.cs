using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.RecipeImports;

public sealed class RecipeImportConfiguration : IEntityTypeConfiguration<RecipeImport>
{
    public void Configure(EntityTypeBuilder<RecipeImport> builder)
    {
        builder.ToTable("recipe_imports");

        builder.HasKey(recipeImport => recipeImport.Id);

        builder.Property(recipeImport => recipeImport.SourceType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(recipeImport => recipeImport.SourceUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(recipeImport => recipeImport.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(recipeImport => recipeImport.DraftJson)
            .HasColumnName("draft_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(recipeImport => recipeImport.WarningsJson)
            .HasColumnName("warnings_json")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(recipeImport => recipeImport.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(recipeImport => recipeImport.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(recipeImport => new { recipeImport.UserId, recipeImport.CreatedAt });
    }
}
