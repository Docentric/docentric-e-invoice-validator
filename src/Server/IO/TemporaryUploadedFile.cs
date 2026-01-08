using Docentric.EInvoice.Validator.RestServer.Contracts;

namespace Docentric.EInvoice.Validator.RestServer.IO;

/// <summary>
/// Stores an uploaded file as a temporary file on disk.
/// The file is deleted when the instance is disposed.
/// </summary>
public sealed class TemporaryUploadedFile : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Full path to the temporary file.
    /// </summary>
    public string FilePath { get; }

    private bool _disposed;

    private TemporaryUploadedFile(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>
    /// Creates a temporary file from the uploaded request and returns a disposable wrapper.
    /// </summary>
    public static async Task<TemporaryUploadedFile> CreateAsync(FileUploadRequest request, CancellationToken cancellationToken)
    {
        if (request?.File == null)
            throw new ArgumentNullException(nameof(request.File), "The form in POST request did not contain a file.");

        if (request.File.Length == 0)
            throw new ArgumentException("Uploaded file is empty.", nameof(request.File));

        if (string.IsNullOrWhiteSpace(request.File.FileName))
            throw new ArgumentNullException(nameof(request.File.FileName), "File uploaded is missing a filename.");

        // Keep the original extension if present
        string extension = Path.GetExtension(request.File.FileName);
        string tempFilePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}{extension}"
        );

        await using FileStream targetStream = File.Create(tempFilePath);
        await request.File.CopyToAsync(targetStream, cancellationToken);

        return new TemporaryUploadedFile(tempFilePath);
    }

    /// <summary>
    /// Deletes the temporary file.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch
        {
            // Swallow exceptions: cleanup best-effort
        }
    }

    /// <summary>
    /// Async dispose support.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}

