using Microsoft.EntityFrameworkCore;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.UnitTests.Support;

namespace PantryPlanner.Api.UnitTests.Features.Recipes.CreateIngredient;

public sealed class CreateIngredientHandlerTests
{
    [Fact]
    public async Task Handle_CreatesIngredient_WhenNameIsAvailable()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var handler = new CreateIngredientHandler(repository);
        var userId = Guid.NewGuid();

        var result = await handler.Handle(new CreateIngredientCommand
        {
            UserId = userId,
            Name = "  Chicken Thighs  "
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Chicken Thighs", result.Value.Name);
        Assert.Equal("chicken thighs", result.Value.NormalizedName);

        var ingredient = await dbContext.Ingredients.SingleAsync();
        Assert.Equal(userId, ingredient.UserId);
        Assert.Equal("Chicken Thighs", ingredient.Name);
    }

    [Fact]
    public async Task Handle_ReturnsConflict_WhenNameAlreadyExistsForUser()
    {
        await using var dbContext = InMemoryDbContextFactory.Create();
        var repository = new Repository(dbContext);
        var handler = new CreateIngredientHandler(repository);
        var userId = Guid.NewGuid();

        dbContext.Ingredients.Add(Ingredient.Create(userId, "Paprika"));
        await dbContext.SaveChangesAsync();

        var result = await handler.Handle(new CreateIngredientCommand
        {
            UserId = userId,
            Name = " paprika "
        }, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("ingredient_name_in_use", result.Error?.Code);
        Assert.Equal(1, await dbContext.Ingredients.CountAsync());
    }
}
