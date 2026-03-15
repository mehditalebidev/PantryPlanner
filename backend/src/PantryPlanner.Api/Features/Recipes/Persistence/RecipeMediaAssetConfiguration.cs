using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PantryPlanner.Api.Features.Recipes;

public sealed class RecipeMediaAssetConfiguration : IEntityTypeConfiguration<RecipeMediaAsset>
{
    public void Configure(EntityTypeBuilder<RecipeMediaAsset> builder)
    {
        builder.ToTable("recipe_media_assets");

        builder.HasKey(mediaAsset => mediaAsset.Id);

        builder.Property(mediaAsset => mediaAsset.Kind)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(mediaAsset => mediaAsset.StorageKey)
            .HasMaxLength(500);

        builder.Property(mediaAsset => mediaAsset.Url)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(mediaAsset => mediaAsset.Caption)
            .HasMaxLength(500);
    }
}
