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
            .WithSummary("Checks server health")
            .WithDescription("Verifies that the required dependencies (Java and the Mustang CLI jar) are available. Always returns 200; check the presence flags in the response to detect a missing dependency.")
            .Produces<ServerHealthResponse>(StatusCodes.Status200OK, "application/json");

        return endpoints;
    }

    /// <summary>
    /// Handles health check requests by verifying the availability of required dependencies.
    /// </summary>
    /// <returns>
    /// Always returns 200 OK with the health status; missing dependencies are reported in the response body.
    /// </returns>
    private static async Task<IResult> HealthHandler(JavaService javaService, MustangCliService mustangCliService, CancellationToken cancellationToken)
    {
        (bool javaPresent, string? javaVersion) = await CheckJavaVersionAsync(javaService, cancellationToken);
        (bool mustangPresent, string? mustangVersion) = await CheckMustangCliVersionAsync(mustangCliService, cancellationToken);

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
    private static async Task<(bool present, string? version)> CheckJavaVersionAsync(JavaService javaService, CancellationToken cancellationToken)
    {
        try
        {
            JavaInfoResult javaInfo = await javaService.GetJavaInfoAsync(cancellationToken);
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
    ///
    private static async Task<(bool present, string? version)> CheckMustangCliVersionAsync(MustangCliService mustangCliService, CancellationToken cancellationToken)
    {
        bool isAvailable = await mustangCliService.CheckAvailabilityAsync(cancellationToken);

        return (isAvailable, isAvailable ? mustangCliService.GetMustangCliVersion() : null);
    }
}
