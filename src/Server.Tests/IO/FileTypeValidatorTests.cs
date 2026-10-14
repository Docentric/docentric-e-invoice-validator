using System.Text;

using Docentric.EInvoice.Validator.RestServer.IO;

using Microsoft.AspNetCore.Http;

namespace Docentric.EInvoice.Validator.RestServer.Tests.IO;

/// <summary>
/// Tests the magic-byte file type detection of FileTypeValidator.
/// </summary>
public class FileTypeValidatorTests
{
    private static FormFile CreateFile(byte[] content) =>
        new(new MemoryStream(content), 0, content.Length, "file", "test.bin");

    private static FormFile CreateFile(string content) => CreateFile(Encoding.UTF8.GetBytes(content));

    /// <summary>
    /// Verifies that content starting with the PDF signature is recognized as PDF.
    /// </summary>
    [Fact]
    public async Task WhenContentStartsWithPdfSignatureThenIsPdfReturnsTrue()
    {
        FormFile file = CreateFile("%PDF-1.7 content");

        bool result = await FileTypeValidator.IsPdfAsync(file, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that content shorter than the PDF signature is not recognized as PDF.
    /// </summary>
    [Fact]
    public async Task WhenContentIsShorterThanPdfSignatureThenIsPdfReturnsFalse()
    {
        FormFile file = CreateFile("%PDF");

        bool result = await FileTypeValidator.IsPdfAsync(file, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that empty content is not recognized as PDF.
    /// </summary>
    [Fact]
    public async Task WhenContentIsEmptyThenIsPdfReturnsFalse()
    {
        FormFile file = CreateFile([]);

        bool result = await FileTypeValidator.IsPdfAsync(file, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that non-PDF content is not recognized as PDF.
    /// </summary>
    [Fact]
    public async Task WhenContentIsNotPdfThenIsPdfReturnsFalse()
    {
        FormFile file = CreateFile("<?xml version=\"1.0\"?><a/>");

        bool result = await FileTypeValidator.IsPdfAsync(file, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that PDF detection observes a canceled token.
    /// </summary>
    [Fact]
    public async Task WhenCancellationIsRequestedThenIsPdfThrows()
    {
        FormFile file = CreateFile("%PDF-1.7 content");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileTypeValidator.IsPdfAsync(file, cts.Token));
    }

    /// <summary>
    /// Verifies that content that looks like XML is recognized as XML.
    /// </summary>
    /// <param name="content">The file content under test.</param>
    [Theory]
    [InlineData("<?xml version=\"1.0\"?><a/>")]
    [InlineData("<root/>")]
    [InlineData("<ns:root/>")]
    [InlineData("<_root/>")]
    [InlineData("< root/>")]
    public async Task WhenContentIsXmlThenIsXmlReturnsTrue(string content)
    {
        FormFile file = CreateFile(content);

        bool result = await FileTypeValidator.IsXmlAsync(file, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that XML starting with a UTF-8 byte order mark is recognized as XML.
    /// </summary>
    [Fact]
    public async Task WhenXmlHasUtf8BomThenIsXmlReturnsTrue()
    {
        byte[] content = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("<?xml version=\"1.0\"?><a/>")];
        FormFile file = CreateFile(content);

        bool result = await FileTypeValidator.IsXmlAsync(file, TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that empty content is not recognized as XML.
    /// </summary>
    [Fact]
    public async Task WhenContentIsEmptyThenIsXmlReturnsFalse()
    {
        FormFile file = CreateFile([]);

        bool result = await FileTypeValidator.IsXmlAsync(file, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that content that does not look like XML is not recognized as XML.
    /// </summary>
    /// <param name="content">The file content under test.</param>
    [Theory]
    [InlineData("%PDF-1.7 content")]
    [InlineData("plain text")]
    [InlineData("<1root/>")]
    [InlineData("<")]
    [InlineData("<   ")]
    public async Task WhenContentIsNotXmlThenIsXmlReturnsFalse(string content)
    {
        FormFile file = CreateFile(content);

        bool result = await FileTypeValidator.IsXmlAsync(file, TestContext.Current.CancellationToken);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that XML detection observes a canceled token.
    /// </summary>
    [Fact]
    public async Task WhenCancellationIsRequestedThenIsXmlThrows()
    {
        FormFile file = CreateFile("<root/>");
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => FileTypeValidator.IsXmlAsync(file, cts.Token));
    }
}
