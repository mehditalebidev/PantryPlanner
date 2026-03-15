namespace PantryPlanner.Api.Common.Results;

public sealed record Error(string Code, string Title, string Detail, int StatusCode);
