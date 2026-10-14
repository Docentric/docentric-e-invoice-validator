using System.Net;
using System.Net.Http.Json;

using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Tests.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Tests the PDF endpoints of the REST server using requests that are rejected before Mustang CLI is invoked.
/// </summary>
/// <param name="factory">The in-memory server factory shared by the tests in this class.</param>
public class PdfEndpointsTests(RestServerFactory factory) : IClassFixture<RestServerFactory>
{
    /// <summary>
    /// Verifies that uploading a non-PDF file to a PDF endpoint returns 400 Bad Request.
    /// </summary>
    /// <param name="url">The PDF endpoint URL under test.</param>
    [Theory]
    [InlineData("/api/pdf/validate-zugferd")]
    [InlineData("/api/pdf/extract-zugferd-xml")]
    public async Task WhenUploadedFileIsNotPdfThenReturnsBadRequest(string url)
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.pdf");

        using HttpResponseMessage response = await client.PostAsync(url, form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verifies that validating a non-PDF file returns the InvalidFileFormat error code.
    /// </summary>
    [Fact]
    public async Task WhenValidatedFileIsNotPdfThenErrorCodeIsInvalidFileFormat()
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.pdf");

        using HttpResponseMessage response = await client.PostAsync("/api/pdf/validate-zugferd", form, TestContext.Current.CancellationToken);
        PdfFileValidationResponse? body = await response.Content.ReadFromJsonAsync<PdfFileValidationResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(ErrorCode.InvalidFileFormat, body?.ErrorCode);
    }

    /// <summary>
    /// Verifies that extracting XML from a non-PDF file returns the InvalidFileFormat error code.
    /// </summary>
    [Fact]
    public async Task WhenExtractedFileIsNotPdfThenErrorCodeIsInvalidFileFormat()
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.pdf");

        using HttpResponseMessage response = await client.PostAsync("/api/pdf/extract-zugferd-xml", form, TestContext.Current.CancellationToken);
        ExtractXmlFromPdfResponse? body = await response.Content.ReadFromJsonAsync<ExtractXmlFromPdfResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(ErrorCode.InvalidFileFormat, body?.ErrorCode);
    }

    /// <summary>
    /// Verifies that a request that is not multipart/form-data is not processed successfully.
    /// </summary>
    /// <param name="url">The PDF endpoint URL under test.</param>
    [Theory]
    [InlineData("/api/pdf/validate-zugferd")]
    [InlineData("/api/pdf/extract-zugferd-xml")]
    public async Task WhenRequestIsNotMultipartThenDoesNotSucceed(string url)
    {
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("%PDF-1.7", System.Text.Encoding.UTF8, "application/pdf");

        using HttpResponseMessage response = await client.PostAsync(url, content, TestContext.Current.CancellationToken);

        Assert.False(response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Verifies that validating a valid ZUGFeRD PDF returns 200 OK.
    /// </summary>
    [Fact]
    public async Task WhenZugferdPdfIsValidThenValidateReturnsOk()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.ZugferdPdfPath);

        using HttpResponseMessage response = await client.PostAsync("/api/pdf/validate-zugferd", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that validating a valid ZUGFeRD PDF reports it as valid.
    /// </summary>
    [Fact]
    public async Task WhenZugferdPdfIsValidThenValidateReportsIsValid()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.ZugferdPdfPath);

        using HttpResponseMessage response = await client.PostAsync("/api/pdf/validate-zugferd", form, TestContext.Current.CancellationToken);
        PdfFileValidationResponse? body = await response.Content.ReadFromJsonAsync<PdfFileValidationResponse>(TestContext.Current.CancellationToken);

        Assert.True(body?.IsValid);
    }

    /// <summary>
    /// Verifies that the XML embedded in a valid ZUGFeRD PDF is extracted.
    /// </summary>
    [Fact]
    public async Task WhenZugferdPdfIsValidThenExtractReturnsXml()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.ZugferdPdfPath);

        using HttpResponseMessage response = await client.PostAsync("/api/pdf/extract-zugferd-xml", form, TestContext.Current.CancellationToken);
        ExtractXmlFromPdfResponse? body = await response.Content.ReadFromJsonAsync<ExtractXmlFromPdfResponse>(TestContext.Current.CancellationToken);

        Assert.False(string.IsNullOrWhiteSpace(body?.Xml));
    }
}
