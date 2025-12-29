using Docentric.EInvoice.Validator.RestServer.Endpoints;

namespace Docentric.EInvoice.Validator.RestServer.Configuration;

/// <summary>
/// Provides extension methods for configuring API endpoints.
/// </summary>
internal static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps all API endpoints including health checks, PDF, and XML endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    internal static void MapApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapHealthEndpoints();
        app.MapPdfEndpoints();
        app.MapXmlEndpoints();
    }
}
