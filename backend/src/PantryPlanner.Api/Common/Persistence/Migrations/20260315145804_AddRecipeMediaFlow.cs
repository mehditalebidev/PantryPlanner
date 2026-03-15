using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryPlanner.Api.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeMediaFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "recipe_media_assets",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipe_media_assets_StorageKey",
                table: "recipe_media_assets",
                column: "StorageKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_recipe_media_assets_StorageKey",
                table: "recipe_media_assets");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "recipe_media_assets");
        }
    }
}
