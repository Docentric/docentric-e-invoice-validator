using System.Text;

using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.IO;

using Microsoft.AspNetCore.Http;

namespace Docentric.EInvoice.Validator.RestServer.Tests.IO;

/// <summary>
/// Tests the temporary file handling of TemporaryUploadedFile.
/// </summary>
public class TemporaryUploadedFileTests
{
    private static FileUploadRequest CreateRequest(string content, string fileName)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        return new FileUploadRequest { File = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName) };
    }

    /// <summary>
    /// Verifies that the uploaded content is written to the temporary file.
    /// </summary>
    [Fact]
    public async Task WhenFileIsValidThenCreateAsyncWritesContentToDisk()
    {
        await using TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        string content = await File.ReadAllTextAsync(file.FilePath, TestContext.Current.CancellationToken);

        Assert.Equal("hello", content);
    }

    /// <summary>
    /// Verifies that the temporary file keeps the extension of the uploaded file.
    /// </summary>
    [Fact]
    public async Task WhenFileHasExtensionThenCreateAsyncKeepsExtension()
    {
        await using TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        Assert.EndsWith(".xml", file.FilePath);
    }

    /// <summary>
    /// Verifies that disposing the instance deletes the temporary file.
    /// </summary>
    [Fact]
    public async Task WhenDisposedThenFileIsDeleted()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        file.Dispose();

        Assert.False(File.Exists(file.FilePath));
    }

    /// <summary>
    /// Verifies that disposing the instance asynchronously deletes the temporary file.
    /// </summary>
    [Fact]
    public async Task WhenDisposedAsyncThenFileIsDeleted()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        await file.DisposeAsync();

        Assert.False(File.Exists(file.FilePath));
    }

    /// <summary>
    /// Verifies that disposing the instance more than once does not throw.
    /// </summary>
    [Fact]
    public async Task WhenDisposedTwiceThenNoExceptionIsThrown()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        file.Dispose();
        Exception? exception = Record.Exception(file.Dispose);

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that a request without a file is rejected.
    /// </summary>
    [Fact]
    public async Task WhenFileIsNullThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = new();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Verifies that an empty uploaded file is rejected.
    /// </summary>
    [Fact]
    public async Task WhenFileIsEmptyThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = CreateRequest(string.Empty, "invoice.xml");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }

    /// <summary>
    /// Verifies that an uploaded file without a file name is rejected.
    /// </summary>
    [Fact]
    public async Task WhenFileNameIsBlankThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = CreateRequest("hello", " ");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }
}
