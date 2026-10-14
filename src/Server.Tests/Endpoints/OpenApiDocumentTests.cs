using System.Text.Json;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Tests that the generated OpenAPI document describes every endpoint and every parameter of the REST server.
/// </summary>
/// <param name="factory">The in-memory server factory shared by the tests in this class.</param>
public class OpenApiDocumentTests(RestServerFactory factory) : IClassFixture<RestServerFactory>
{
    private static readonly string[] ExpectedOperations =
    [
        "GET /api/health",
        "POST /api/pdf/extract-zugferd-xml",
        "POST /api/pdf/validate-zugferd",
        "POST /api/xml/convert-to-pdf",
        "POST /api/xml/validate",
    ];

    /// <summary>
    /// Verifies that the OpenAPI document contains exactly the expected endpoints and HTTP methods.
    /// </summary>
    [Fact]
    public async Task WhenOpenApiDocumentIsRequestedThenItContainsAllEndpoints()
    {
        using JsonDocument document = await GetOpenApiDocumentAsync();

        string[] operations = [.. GetOperations(document).Select(o => o.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(ExpectedOperations, operations);
    }

    /// <summary>
    /// Verifies that every operation has a non-empty summary.
    /// </summary>
    [Fact]
    public async Task WhenOpenApiDocumentIsRequestedThenEveryOperationHasSummary()
    {
        using JsonDocument document = await GetOpenApiDocumentAsync();

        string[] withoutSummary = [.. GetOperations(document).Where(o => string.IsNullOrWhiteSpace(GetString(o.Element, "summary"))).Select(o => o.Name)];

        Assert.Empty(withoutSummary);
    }

    /// <summary>
    /// Verifies that every operation has a non-empty description.
    /// </summary>
    [Fact]
    public async Task WhenOpenApiDocumentIsRequestedThenEveryOperationHasDescription()
    {
        using JsonDocument document = await GetOpenApiDocumentAsync();

        string[] withoutDescription = [.. GetOperations(document).Where(o => string.IsNullOrWhiteSpace(GetString(o.Element, "description"))).Select(o => o.Name)];

        Assert.Empty(withoutDescription);
    }

    /// <summary>
    /// Verifies that every operation parameter has a non-empty description.
    /// </summary>
    [Fact]
    public async Task WhenOpenApiDocumentIsRequestedThenEveryParameterHasDescription()
    {
        using JsonDocument document = await GetOpenApiDocumentAsync();

        string[] withoutDescription =
        [
            .. GetOperations(document)
                .SelectMany(o => GetParameters(o.Element).Select(p => (o.Name, Parameter: p)))
                .Where(x => string.IsNullOrWhiteSpace(GetString(x.Parameter, "description")))
                .Select(x => $"{x.Name} {GetString(x.Parameter, "name")}")
        ];

        Assert.Empty(withoutDescription);
    }

    /// <summary>
    /// Verifies that the convert-to-pdf operation documents the optional language query parameter.
    /// </summary>
    [Fact]
    public async Task WhenOpenApiDocumentIsRequestedThenConvertToPdfDocumentsLanguageQueryParameter()
    {
        using JsonDocument document = await GetOpenApiDocumentAsync();
        JsonElement convertToPdf = GetOperations(document).Single(o => o.Name == "POST /api/xml/convert-to-pdf").Element;

        Assert.Contains(GetParameters(convertToPdf), p => GetString(p, "name") == "language" && GetString(p, "in") == "query");
    }

    private async Task<JsonDocument> GetOpenApiDocumentAsync()
    {
        using HttpClient client = factory.CreateApiClient();
        return JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json", TestContext.Current.CancellationToken));
    }

    private static IEnumerable<(string Name, JsonElement Element)> GetOperations(JsonDocument document)
        => document.RootElement.GetProperty("paths").EnumerateObject()
            .SelectMany(path => path.Value.EnumerateObject()
                .Select(method => ($"{method.Name.ToUpperInvariant()} {path.Name}", method.Value)));

    private static IEnumerable<JsonElement> GetParameters(JsonElement operation)
        => operation.TryGetProperty("parameters", out JsonElement parameters) ? parameters.EnumerateArray() : [];

    private static string? GetString(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out JsonElement value) ? value.GetString() : null;
}
