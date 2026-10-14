using System.Text;

using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.IO;

using Microsoft.AspNetCore.Http;

namespace Docentric.EInvoice.Validator.RestServer.Tests.IO;

public class TemporaryUploadedFileTests
{
    private static FileUploadRequest CreateRequest(string content, string fileName)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(content);
        return new FileUploadRequest { File = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName) };
    }

    [Fact]
    public async Task WhenFileIsValidThenCreateAsyncWritesContentToDisk()
    {
        await using TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        string content = await File.ReadAllTextAsync(file.FilePath, TestContext.Current.CancellationToken);

        Assert.Equal("hello", content);
    }

    [Fact]
    public async Task WhenFileHasExtensionThenCreateAsyncKeepsExtension()
    {
        await using TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        Assert.EndsWith(".xml", file.FilePath);
    }

    [Fact]
    public async Task WhenDisposedThenFileIsDeleted()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        file.Dispose();

        Assert.False(File.Exists(file.FilePath));
    }

    [Fact]
    public async Task WhenDisposedAsyncThenFileIsDeleted()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        await file.DisposeAsync();

        Assert.False(File.Exists(file.FilePath));
    }

    [Fact]
    public async Task WhenDisposedTwiceThenNoExceptionIsThrown()
    {
        TemporaryUploadedFile file = await TemporaryUploadedFile.CreateAsync(
            CreateRequest("hello", "invoice.xml"), TestContext.Current.CancellationToken);

        file.Dispose();
        Exception? exception = Record.Exception(file.Dispose);

        Assert.Null(exception);
    }

    [Fact]
    public async Task WhenFileIsNullThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = new();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenFileIsEmptyThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = CreateRequest(string.Empty, "invoice.xml");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WhenFileNameIsBlankThenCreateAsyncThrowsArgumentException()
    {
        FileUploadRequest request = CreateRequest("hello", " ");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            TemporaryUploadedFile.CreateAsync(request, TestContext.Current.CancellationToken));
    }
}
