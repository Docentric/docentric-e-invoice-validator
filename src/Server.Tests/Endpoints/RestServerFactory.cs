using Docentric.EInvoice.Validator.RestServer.Contracts;

using Microsoft.AspNetCore.Mvc.Testing;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Hosts the REST server in memory for endpoint tests.
/// </summary>
/// <remarks>
/// The generated <c>Program</c> class is internal, so a public type from the same assembly is used
/// to locate the entry point instead of making <c>Program</c> public.
/// </remarks>
public sealed class RestServerFactory : WebApplicationFactory<FileUploadRequest>
{
    /// <summary>
    /// Creates a client that talks HTTPS and does not follow redirects, so HTTPS redirection does not interfere.
    /// </summary>
    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        AllowAutoRedirect = false,
    });
}
