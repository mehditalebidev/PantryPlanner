using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using PantryPlanner.Api.Common.Api;
using PantryPlanner.Api.Common.Results;

namespace PantryPlanner.Api.UnitTests.Common.Api;

public sealed class ProblemDetailsExtensionsTests
{
    [Fact]
    public void ToActionResult_ReturnsOkResult_WhenResultSucceeds()
    {
        var controller = CreateController();
        var result = Result<string>.Success("ok");

        var actionResult = result.ToActionResult(controller);

        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal("ok", okResult.Value);
    }

    [Fact]
    public void ToActionResult_ReturnsNoContentResult_WhenNonGenericResultSucceeds()
    {
        var controller = CreateController();
        var result = Result.Success();

        var actionResult = result.ToActionResult(controller);

        Assert.IsType<NoContentResult>(actionResult);
    }

    [Fact]
    public void ToProblem_ReturnsProblemDetailsWithCodeExtension()
    {
        var controller = CreateController();
        var error = new Error("invalid_credentials", "Credentials are invalid.", "Email or password is incorrect.", 401);

        var result = controller.ToProblem(error);

        var objectResult = Assert.IsType<ObjectResult>(result);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Equal(401, objectResult.StatusCode);
        Assert.Equal("https://httpstatuses.com/401", problemDetails.Type);
        Assert.Equal("Credentials are invalid.", problemDetails.Title);
        Assert.Equal("Email or password is incorrect.", problemDetails.Detail);
        Assert.Equal("invalid_credentials", problemDetails.Extensions["code"]);
    }

    private static TestController CreateController()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ProblemDetailsFactory, TestProblemDetailsFactory>();

        return new TestController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = services.BuildServiceProvider()
                }
            }
        };
    }

    private sealed class TestController : ControllerBase
    {
    }

    private sealed class TestProblemDetailsFactory : ProblemDetailsFactory
    {
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            return new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };
        }

        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string? title = null,
            string? type = null,
            string? detail = null,
            string? instance = null)
        {
            return new ValidationProblemDetails(modelStateDictionary)
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };
        }
    }
}
