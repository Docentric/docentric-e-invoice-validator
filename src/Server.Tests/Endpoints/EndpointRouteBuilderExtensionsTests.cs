using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Verifies that the API endpoints are registered by the application routing configuration.
/// </summary>
/// <param name="factory">The in-memory server factory shared by the tests in this class.</param>
public class EndpointRouteBuilderExtensionsTests(RestServerFactory factory) : IClassFixture<RestServerFactory>
{
    /// <summary>
    /// Verifies that the given API route is registered when the application starts.
    /// </summary>
    /// <param name="route">The route pattern expected to be registered.</param>
    [Theory]
    [InlineData("/api/health")]
    [InlineData("/api/pdf/validate-zugferd")]
    [InlineData("/api/pdf/extract-zugferd-xml")]
    [InlineData("/api/xml/validate")]
    [InlineData("/api/xml/convert-to-pdf")]
    public void WhenApplicationStartsThenApiRouteIsRegistered(string route)
    {
        EndpointDataSource dataSource = factory.Services.GetRequiredService<EndpointDataSource>();

        IEnumerable<string> patterns = dataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => "/" + e.RoutePattern.RawText?.TrimStart('/'));

        Assert.Contains(route, patterns);
    }
}
