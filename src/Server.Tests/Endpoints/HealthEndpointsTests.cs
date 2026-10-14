using System.Net;
using System.Net.Http.Json;

using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Tests.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Tests the health endpoint of the REST server.
/// </summary>
/// <param name="factory">The in-memory server factory shared by the tests in this class.</param>
public class HealthEndpointsTests(RestServerFactory factory) : IClassFixture<RestServerFactory>
{
    /// <summary>
    /// Verifies that the health endpoint responds with 200 OK when Java is installed.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenHealthReturnsOk()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();

        using HttpResponseMessage response = await client.GetAsync("/api/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that the health endpoint reports Java as present when Java is installed.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenHealthReportsJavaPresent()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();

        ServerHealthResponse? health = await client.GetFromJsonAsync<ServerHealthResponse>("/api/health", TestContext.Current.CancellationToken);

        Assert.True(health?.JavaPresent);
    }

    /// <summary>
    /// Verifies that the health endpoint reports Mustang CLI as present when Java is installed.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenHealthReportsMustangCliPresent()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();

        ServerHealthResponse? health = await client.GetFromJsonAsync<ServerHealthResponse>("/api/health", TestContext.Current.CancellationToken);

        Assert.True(health?.MustangCliPresent);
    }
}
