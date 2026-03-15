using FluentValidation;

namespace PantryPlanner.Api.Features.Media;

public sealed class UploadRecipeMediaCommandValidator : AbstractValidator<UploadRecipeMediaCommand>
{
    private static readonly string[] AllowedKinds = ["image", "video"];

    public UploadRecipeMediaCommandValidator()
    {
        RuleFor(command => command.Kind)
            .NotEmpty()
            .WithMessage("Media kind is required.")
            .Must(kind => AllowedKinds.Contains(kind, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Media kind must be image or video.");

        RuleFor(command => command.Caption)
            .MaximumLength(500)
            .WithMessage("Caption must be 500 characters or fewer.");

        RuleFor(command => command.SortOrder)
            .GreaterThan(0)
            .WithMessage("Media sort order must be greater than 0.");

        RuleFor(command => command.File)
            .NotNull()
            .WithMessage("Media file is required.")
            .Must(file => file is not null && file.Length > 0)
            .WithMessage("Media file must not be empty.");

        RuleFor(command => command)
            .Custom((command, context) =>
            {
                if (command.File is null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(command.File.ContentType))
                {
                    context.AddFailure(nameof(command.File), "Media file content type is required.");
                    return;
                }

                if (command.Kind.Equals("image", StringComparison.OrdinalIgnoreCase)
                    && !command.File.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    context.AddFailure(nameof(command.File), "Media file content type must match the image kind.");
                }

                if (command.Kind.Equals("video", StringComparison.OrdinalIgnoreCase)
                    && !command.File.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
                {
                    context.AddFailure(nameof(command.File), "Media file content type must match the video kind.");
                }
            });
    }
}
