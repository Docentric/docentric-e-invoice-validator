using System.Net;
using System.Net.Http.Json;

using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Tests.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Endpoints;

/// <summary>
/// Tests the XML endpoints of the REST server using requests that are rejected before Mustang CLI is invoked.
/// </summary>
/// <param name="factory">The in-memory server factory shared by the tests in this class.</param>
public class XmlEndpointsTests(RestServerFactory factory) : IClassFixture<RestServerFactory>
{
    /// <summary>
    /// Verifies that uploading a non-XML file to an XML endpoint returns 400 Bad Request.
    /// </summary>
    /// <param name="url">The XML endpoint URL under test.</param>
    [Theory]
    [InlineData("/api/xml/validate")]
    [InlineData("/api/xml/convert-to-pdf")]
    public async Task WhenUploadedFileIsNotXmlThenReturnsBadRequest(string url)
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.xml");

        using HttpResponseMessage response = await client.PostAsync(url, form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Verifies that validating a non-XML file returns the InvalidFileFormat error code.
    /// </summary>
    [Fact]
    public async Task WhenValidatedFileIsNotXmlThenErrorCodeIsInvalidFileFormat()
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.xml");

        using HttpResponseMessage response = await client.PostAsync("/api/xml/validate", form, TestContext.Current.CancellationToken);
        FileValidationResponse? body = await response.Content.ReadFromJsonAsync<FileValidationResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(ErrorCode.InvalidFileFormat, body?.ErrorCode);
    }

    /// <summary>
    /// Verifies that converting a non-XML file to PDF returns the InvalidFileFormat error code.
    /// </summary>
    [Fact]
    public async Task WhenConvertedFileIsNotXmlThenErrorCodeIsInvalidFileFormat()
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFile("plain text", "invoice.xml");

        using HttpResponseMessage response = await client.PostAsync("/api/xml/convert-to-pdf", form, TestContext.Current.CancellationToken);
        ConvertXmlToPdfResponse? body = await response.Content.ReadFromJsonAsync<ConvertXmlToPdfResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(ErrorCode.InvalidFileFormat, body?.ErrorCode);
    }

    /// <summary>
    /// Verifies that converting with an unsupported language returns the InvalidRequest error code.
    /// </summary>
    [Fact]
    public async Task WhenConvertLanguageIsUnsupportedThenErrorCodeIsInvalidRequest()
    {
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.FacturXXmlPath);

        using HttpResponseMessage response = await client.PostAsync("/api/xml/convert-to-pdf?language=xx", form, TestContext.Current.CancellationToken);
        ConvertXmlToPdfResponse? body = await response.Content.ReadFromJsonAsync<ConvertXmlToPdfResponse>(TestContext.Current.CancellationToken);

        Assert.Equal(ErrorCode.InvalidRequest, body?.ErrorCode);
    }

    /// <summary>
    /// Verifies that converting a valid Factur-X XML with an explicit supported language returns PDF content.
    /// </summary>
    [Fact]
    public async Task WhenConvertLanguageIsSpecifiedThenConvertToPdfReturnsPdfContent()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.FacturXXmlPath);

        using HttpResponseMessage response = await client.PostAsync("/api/xml/convert-to-pdf?language=de", form, TestContext.Current.CancellationToken);
        ConvertXmlToPdfResponse? body = await response.Content.ReadFromJsonAsync<ConvertXmlToPdfResponse>(TestContext.Current.CancellationToken);

        Assert.NotEmpty(body?.Pdf ?? []);
    }

    /// <summary>
    /// Verifies that a request that is not multipart/form-data is not processed successfully.
    /// </summary>
    /// <param name="url">The XML endpoint URL under test.</param>
    [Theory]
    [InlineData("/api/xml/validate")]
    [InlineData("/api/xml/convert-to-pdf")]
    public async Task WhenRequestIsNotMultipartThenDoesNotSucceed(string url)
    {
        using HttpClient client = factory.CreateApiClient();
        using StringContent content = new("<invoice/>", System.Text.Encoding.UTF8, "application/xml");

        using HttpResponseMessage response = await client.PostAsync(url, content, TestContext.Current.CancellationToken);

        Assert.False(response.IsSuccessStatusCode);
    }

    /// <summary>
    /// Verifies that validating a valid Factur-X invoice returns 200 OK.
    /// </summary>
    [Fact]
    public async Task WhenFacturXXmlIsValidThenValidateReturnsOk()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.FacturXXmlPath);

        using HttpResponseMessage response = await client.PostAsync("/api/xml/validate", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Verifies that validating a valid Factur-X invoice reports it as valid.
    /// </summary>
    [Fact]
    public async Task WhenFacturXXmlIsValidThenValidateReportsIsValid()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent form = MultipartContentFactory.CreateFromPath(SampleFiles.FacturXXmlPath);

        using HttpResponseMessage response = await client.PostAsync("/api/xml/validate", form, TestContext.Current.CancellationToken);
        FileValidationResponse? body = await response.Content.ReadFromJsonAsync<FileValidationResponse>(TestContext.Current.CancellationToken);

        Assert.True(body?.IsValid);
    }

    /// <summary>
    /// Verifies that XML extracted from a valid ZUGFeRD PDF can be converted back to a PDF.
    /// </summary>
    [Fact]
    public async Task WhenXmlIsExtractedFromZugferdPdfThenConvertToPdfReturnsPdfContent()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using HttpClient client = factory.CreateApiClient();
        using MultipartFormDataContent pdfForm = MultipartContentFactory.CreateFromPath(SampleFiles.ZugferdPdfPath);
        using HttpResponseMessage extractResponse = await client.PostAsync("/api/pdf/extract-zugferd-xml", pdfForm, TestContext.Current.CancellationToken);
        ExtractXmlFromPdfResponse? extracted = await extractResponse.Content.ReadFromJsonAsync<ExtractXmlFromPdfResponse>(TestContext.Current.CancellationToken);
        using MultipartFormDataContent xmlForm = MultipartContentFactory.CreateFile(extracted?.Xml ?? string.Empty, "extracted.xml");

        using HttpResponseMessage response = await client.PostAsync("/api/xml/convert-to-pdf", xmlForm, TestContext.Current.CancellationToken);
        ConvertXmlToPdfResponse? body = await response.Content.ReadFromJsonAsync<ConvertXmlToPdfResponse>(TestContext.Current.CancellationToken);

        Assert.NotEmpty(body?.Pdf ?? []);
    }
}
