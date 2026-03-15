using FluentValidation.TestHelper;

using Microsoft.AspNetCore.Http;

using PantryPlanner.Api.Features.Media;

namespace PantryPlanner.Api.UnitTests.Features.Media.UploadRecipeMedia;

public sealed class UploadRecipeMediaCommandValidatorTests
{
    private readonly UploadRecipeMediaCommandValidator _validator = new();

    [Fact]
    public void Validate_Passes_ForValidImageUpload()
    {
        var command = new UploadRecipeMediaCommand
        {
            Kind = "image",
            Caption = "Finished dish",
            SortOrder = 1,
            File = CreateFormFile("dish.jpg", "image/jpeg", new byte[] { 1, 2, 3 })
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Fails_WhenFileContentTypeDoesNotMatchKind()
    {
        var command = new UploadRecipeMediaCommand
        {
            Kind = "video",
            SortOrder = 1,
            File = CreateFormFile("dish.jpg", "image/jpeg", new byte[] { 1, 2, 3 })
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.File)
            .WithErrorMessage("Media file content type must match the video kind.");
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);

        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
