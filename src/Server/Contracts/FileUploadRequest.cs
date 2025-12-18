using Microsoft.AspNetCore.Mvc;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// Request to upload a file with optional checksum verification.
/// </summary>
public sealed class FileUploadRequest
{
    /// <summary>
    /// The file to be uploaded.
    /// </summary>
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = default!;
}
