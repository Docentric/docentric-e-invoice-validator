using System.Text.Json.Serialization;

namespace Docentric.EInvoice.Validator.RestServer.Contracts;

/// <summary>
/// The result of a ZuGFeRD PDF XML extraction operation.
/// </summary>
public class ExtractXmlFromPdfResponse : BaseFileOperationResponse
{
    /// <summary>
    /// The extracted XML content.
    /// </summary>
    [JsonPropertyName("xml")]
    public string? Xml { get; set; } = null;
}
