using System.Text.Json.Serialization;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// The result of a ZuGFeRD file validation operation.
/// </summary>
public sealed class PdfFileValidationResponse : FileValidationResponse
{
    /// <summary>
    /// Indicates whether the digital signature of the file is valid.
    /// </summary>
    [JsonPropertyName("isSignatureValid")]
    public bool IsSignatureValid { get; set; } = false;
}
