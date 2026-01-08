using Docentric.EInvoice.Validator.RestServer.Configuration;
using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Services;

namespace Docentric.EInvoice.Validator.RestServer.Endpoints;

/// <summary>
/// Provides health check endpoints for monitoring server status and dependencies.
/// </summary>
public static class HealthEndpoints
{
    /// <summary>
    /// Maps health check endpoints to the application's routing configuration.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/health", HealthHandler)
            .WithTags("Server health")
            .Produces<ServerHealthResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ServerHealthResponse>(StatusCodes.Status404NotFound, "application/json");

        return endpoints;
    }

    /// <summary>
    /// Handles health check requests by verifying the availability of required dependencies.
    /// </summary>
    /// <returns>
    /// Returns 200 OK with health status when all dependencies are available,
    /// or 404 Not Found when dependencies are missing.
    /// </returns>
    private static async Task<IResult> HealthHandler(JavaService javaService, MustangCliService mustangCliService)
    {
        (bool javaPresent, string? javaVersion) = await CheckJavaVersionAsync(javaService);
        (bool mustangPresent, string? mustangVersion) = await CheckMustangCliVersionAsync(mustangCliService);

        ServerHealthResponse response = new()
        {
            JavaPresent = javaPresent,
            JavaVersion = javaVersion ?? string.Empty,
            MustangCliPresent = mustangPresent,
            MustangCliVersion = mustangVersion
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Checks if Java runtime is available and retrieves its version.
    /// </summary>
    /// <returns>A tuple indicating Java presence and version information.</returns>
    private static async Task<(bool present, string? version)> CheckJavaVersionAsync(JavaService javaService)
    {
        try
        {
            JavaInfoResult javaInfo = await javaService.GetJavaInfoAsync();
            return (javaInfo.IsAvailable, javaInfo.RuntimeVersion);
        }
        catch
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Checks if Mustang CLI tool is available and functional.
    /// </summary>
    /// <returns>True if Mustang CLI is available, false otherwise.</returns>
    private static async Task<(bool present, string? version)> CheckMustangCliVersionAsync(MustangCliService mustangCliService)
    {
        bool isAvailable = await mustangCliService.CheckAvailabilityAsync();

        return (isAvailable, isAvailable ? MustangCliService.MustangCliVersion : null);
    }
}
