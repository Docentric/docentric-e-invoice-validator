using System.Text.Json.Serialization;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// The result of Factur-X or UBL XML to PDF conversion operation.
/// </summary>
public class ConvertXmlToPdfResponse : BaseFileOperationResponse
{
    /// <summary>
    /// The generated PDF file as a byte array.
    /// </summary>
    [JsonPropertyName("pdf")]
    public byte[]? Pdf { get; set; }
}
