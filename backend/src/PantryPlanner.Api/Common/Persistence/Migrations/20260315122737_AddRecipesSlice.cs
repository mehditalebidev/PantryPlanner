using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryPlanner.Api.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipesSlice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Servings = table.Column<int>(type: "integer", nullable: false),
                    PrepTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    CookTimeMinutes = table.Column<int>(type: "integer", nullable: true),
                    SourceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "recipe_ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NormalizedQuantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    NormalizedUnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PreparationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_recipe_ingredients_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_media_assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Caption = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_media_assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_media_assets_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Instruction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_steps_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recipe_step_ingredient_references",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeStepId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeIngredientId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recipe_step_ingredient_references", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recipe_step_ingredient_references_recipe_ingredients_Recipe~",
                        column: x => x.RecipeIngredientId,
                        principalTable: "recipe_ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recipe_step_ingredient_references_recipe_steps_RecipeStepId",
                        column: x => x.RecipeStepId,
                        principalTable: "recipe_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ingredients_UserId_NormalizedName",
                table: "ingredients",
                columns: new[] { "UserId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_IngredientId",
                table: "recipe_ingredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_ingredients_RecipeId_ReferenceKey",
                table: "recipe_ingredients",
                columns: new[] { "RecipeId", "ReferenceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipe_media_assets_RecipeId",
                table: "recipe_media_assets",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_step_ingredient_references_RecipeIngredientId",
                table: "recipe_step_ingredient_references",
                column: "RecipeIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_recipe_step_ingredient_references_RecipeStepId_RecipeIngred~",
                table: "recipe_step_ingredient_references",
                columns: new[] { "RecipeStepId", "RecipeIngredientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_recipe_steps_RecipeId",
                table: "recipe_steps",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_recipes_UserId_Title",
                table: "recipes",
                columns: new[] { "UserId", "Title" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recipe_media_assets");

            migrationBuilder.DropTable(
                name: "recipe_step_ingredient_references");

            migrationBuilder.DropTable(
                name: "recipe_ingredients");

            migrationBuilder.DropTable(
                name: "recipe_steps");

            migrationBuilder.DropTable(
                name: "ingredients");

            migrationBuilder.DropTable(
                name: "recipes");
        }
    }
}
