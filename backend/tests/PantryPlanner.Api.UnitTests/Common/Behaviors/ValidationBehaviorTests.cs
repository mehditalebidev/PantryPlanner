using FluentValidation;
using MediatR;
using PantryPlanner.Api.Common.Behaviors;

namespace PantryPlanner.Api.UnitTests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_CallsNext_WhenNoValidatorsExist()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenValidationPasses()
    {
        var validators = new IValidator<TestRequest>[]
        {
            new LengthValidator()
        };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var nextCalled = false;

        var result = await behavior.Handle(
            new TestRequest("valid"),
            _ =>
            {
                nextCalled = true;
                return Task.FromResult("ok");
            },
            CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ThrowsValidationException_WhenValidationFails()
    {
        var validators = new IValidator<TestRequest>[]
        {
            new LengthValidator(),
            new ContentValidator()
        };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);

        var exception = await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(
            new TestRequest("bad"),
            _ => Task.FromResult("should-not-run"),
            CancellationToken.None));

        var errorMessages = exception.Errors
            .Select(error => error.ErrorMessage)
            .Distinct()
            .OrderBy(message => message)
            .ToArray();

        Assert.Equal(
            ["Text must be at least 5 characters.", "Text must include 'valid'."],
            errorMessages);
    }

    public sealed record TestRequest(string Text) : IRequest<string>;

    private sealed class LengthValidator : AbstractValidator<TestRequest>
    {
        public LengthValidator()
        {
            RuleFor(request => request.Text)
                .MinimumLength(5)
                .WithMessage("Text must be at least 5 characters.");
        }
    }

    private sealed class ContentValidator : AbstractValidator<TestRequest>
    {
        public ContentValidator()
        {
            RuleFor(request => request.Text)
                .Must(text => text.Contains("valid", StringComparison.Ordinal))
                .WithMessage("Text must include 'valid'.");
        }
    }
}
