using System.Text.Json.Serialization;

namespace Docentric.EInvoice.Validator.RestServer.Contracts;

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

    /// <summary>
    /// Indicates whether the PDF part of the hybrid document is valid (e.g. PDF/A-3 conformant).
    /// Note: under ZuGFeRD rule BR-FX-DE-03, PDF/A compliance errors are treated as warnings (not fatal)
    /// when both buyer and seller are in Germany, so <see cref="FileValidationResponse.IsValid"/> may be
    /// <c>true</c> even when this value is <c>false</c>.
    /// </summary>
    [JsonPropertyName("isPdfValid")]
    public bool IsPdfValid { get; set; } = false;

    /// <summary>
    /// Indicates whether the embedded XML invoice data is valid according to the applicable standard.
    /// </summary>
    [JsonPropertyName("isXmlValid")]
    public bool IsXmlValid { get; set; } = false;
}
