using PantryPlanner.Api.Features.Users;

namespace PantryPlanner.Api.UnitTests.Features.Users.Shared;

public sealed class UserMappingsTests
{
    [Fact]
    public void ToResponse_MapsUserFields()
    {
        var user = User.Create("mehdi@example.com", "Mehdi");

        var response = user.ToResponse();

        Assert.Equal(user.Id, response.Id);
        Assert.Equal(user.Email, response.Email);
        Assert.Equal(user.DisplayName, response.DisplayName);
    }
}
