using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.UnitTests.Common.Results;

public sealed class ResultTests
{
    [Fact]
    public void Success_ReturnsSuccessfulResult()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void GenericFailure_Throws_WhenValueIsAccessed()
    {
        var error = new Error("error_code", "Error title", "Error detail", 400);
        var result = Result<string>.Failure(error);

        var exception = Assert.Throws<InvalidOperationException>(() => _ = result.Value);

        Assert.Equal("Cannot access the value of a failed result.", exception.Message);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }
}
