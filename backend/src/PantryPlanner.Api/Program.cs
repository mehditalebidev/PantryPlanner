using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PantryPlanner.Api.Common.Behaviors;
using PantryPlanner.Api.Common.Persistence;
using PantryPlanner.Api.Common.Security;
using PantryPlanner.Api.Features.GroceryLists;
using PantryPlanner.Api.Features.Ingredients;
using PantryPlanner.Api.Features.Media;
using PantryPlanner.Api.Features.MealPlans;
using PantryPlanner.Api.Features.RecipeImports;
using PantryPlanner.Api.Features.Recipes;
using PantryPlanner.Api.Features.Units;
using PantryPlanner.Api.Middleware;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader(),
            new HeaderApiVersionReader("X-Api-Version"),
            new QueryStringApiVersionReader("api-version"));
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddOpenApi("v1");

builder.Services.AddProblemDetails();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.SectionName));

builder.Services.AddDbContext<PantryPlannerDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PantryPlannerDatabase")));

builder.Services.AddMediatR(configuration =>
    configuration.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

builder.Services.AddScoped<IIngredientCatalogSeeder, IngredientCatalogSeeder>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMediaStorage, LocalMediaStorage>();
builder.Services.AddSingleton<IUnitCatalog, InMemoryUnitCatalog>();
builder.Services.AddScoped<IMeasurementNormalizer, MeasurementNormalizer>();
builder.Services.AddScoped<IRecipeContentFactory, RecipeContentFactory>();
builder.Services.AddScoped<IMealPlanContentFactory, MealPlanContentFactory>();
builder.Services.AddScoped<IGroceryListGenerator, GroceryListGenerator>();
builder.Services.AddScoped<IRecipeImportDraftFactory, RecipeImportDraftFactory>();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");

if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
{
    throw new InvalidOperationException("Jwt:SigningKey must be configured.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication is required.",
                    Detail = "A valid bearer token is required to access this resource.",
                    Type = "https://httpstatuses.com/401"
                };

                problemDetails.Extensions["code"] = "unauthorized";

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Access is forbidden.",
                    Detail = "You do not have permission to access this resource.",
                    Type = "https://httpstatuses.com/403"
                };

                problemDetails.Extensions["code"] = "forbidden";

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken: context.HttpContext.RequestAborted);
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PantryPlannerDbContext>();

    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
    else
    {
        await dbContext.Database.EnsureCreatedAsync();
    }

    var ingredientCatalogSeeder = scope.ServiceProvider.GetRequiredService<IIngredientCatalogSeeder>();
    await ingredientCatalogSeeder.SeedDefaultsForExistingUsersAsync(CancellationToken.None);
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.WithTitle("PantryPlanner API Reference");
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.MapControllers();

app.Run();

public partial class Program;
