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

    private static FileUploadRequest CreateRequest(FailingFormFile file) => new() { File = file };

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

    /// <summary>
    /// Verifies that a failure while copying the upload propagates and the partially written file is deleted.
    /// </summary>
    [Fact]
    public async Task WhenCopyFailsThenExceptionPropagatesAndPartialFileIsDeleted()
    {
        FailingFormFile formFile = new(new InvalidOperationException("copy failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            TemporaryUploadedFile.CreateAsync(CreateRequest(formFile), TestContext.Current.CancellationToken));

        Assert.False(File.Exists(formFile.TargetPath));
    }

    /// <summary>
    /// Verifies that a cancellation while copying the upload propagates and the partially written file is deleted.
    /// </summary>
    [Fact]
    public async Task WhenCopyIsCanceledThenCancellationPropagatesAndPartialFileIsDeleted()
    {
        FailingFormFile formFile = new(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            TemporaryUploadedFile.CreateAsync(CreateRequest(formFile), TestContext.Current.CancellationToken));

        Assert.False(File.Exists(formFile.TargetPath));
    }

    /// <summary>
    /// Form file test double that writes some bytes to the target stream and then throws.
    /// </summary>
    private sealed class FailingFormFile(Exception exception) : IFormFile
    {
        public string? TargetPath { get; private set; }

        public string ContentType => "application/xml";

        public string ContentDisposition => string.Empty;

        public IHeaderDictionary Headers => new HeaderDictionary();

        public long Length => 5;

        public string Name => "file";

        public string FileName => "invoice.xml";

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            TargetPath = ((FileStream)target).Name;
            await target.WriteAsync("hello"u8.ToArray(), cancellationToken);
            await target.FlushAsync(cancellationToken);
            throw exception;
        }

        public void CopyTo(Stream target) => throw new NotSupportedException();

        public Stream OpenReadStream() => throw new NotSupportedException();
    }
}
