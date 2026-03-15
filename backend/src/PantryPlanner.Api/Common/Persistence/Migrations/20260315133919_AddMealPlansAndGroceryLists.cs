using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PantryPlanner.Api.Common.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMealPlansAndGroceryLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "grocery_lists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grocery_lists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meal_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "grocery_list_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroceryListId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    UnitCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsChecked = table.Column<bool>(type: "boolean", nullable: false),
                    SourceCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grocery_list_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grocery_list_items_grocery_lists_GroceryListId",
                        column: x => x.GroceryListId,
                        principalTable: "grocery_lists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "meal_slots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meal_slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_meal_slots_meal_plans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "planned_meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    MealSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServingsOverride = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planned_meals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_planned_meals_meal_plans_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "meal_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_planned_meals_meal_slots_MealSlotId",
                        column: x => x.MealSlotId,
                        principalTable: "meal_slots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_planned_meals_recipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "recipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_grocery_list_items_GroceryListId",
                table: "grocery_list_items",
                column: "GroceryListId");

            migrationBuilder.CreateIndex(
                name: "IX_grocery_lists_UserId_GeneratedAt",
                table: "grocery_lists",
                columns: new[] { "UserId", "GeneratedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_meal_plans_UserId_StartDate",
                table: "meal_plans",
                columns: new[] { "UserId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_meal_slots_MealPlanId_Name",
                table: "meal_slots",
                columns: new[] { "MealPlanId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meal_slots_MealPlanId_ReferenceKey",
                table: "meal_slots",
                columns: new[] { "MealPlanId", "ReferenceKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_meal_slots_MealPlanId_SortOrder",
                table: "meal_slots",
                columns: new[] { "MealPlanId", "SortOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planned_meals_MealPlanId_PlannedDate_MealSlotId",
                table: "planned_meals",
                columns: new[] { "MealPlanId", "PlannedDate", "MealSlotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_planned_meals_MealSlotId",
                table: "planned_meals",
                column: "MealSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_planned_meals_RecipeId",
                table: "planned_meals",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grocery_list_items");

            migrationBuilder.DropTable(
                name: "planned_meals");

            migrationBuilder.DropTable(
                name: "grocery_lists");

            migrationBuilder.DropTable(
                name: "meal_slots");

            migrationBuilder.DropTable(
                name: "meal_plans");
        }
    }
}
