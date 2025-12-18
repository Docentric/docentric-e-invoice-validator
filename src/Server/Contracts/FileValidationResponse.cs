using System.Text.Json.Serialization;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// The result of a XML file validation operation.
/// </summary>
public class FileValidationResponse : BaseFileOperationResponse
{
    /// <summary>
    /// Indicates whether the file content is valid according to the ZuGFeRD/Factur-X standard.
    /// </summary>
    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; } = false;

    /// <summary>
    /// The detailed validation report containing the results of the validation process.
    /// </summary>
    [JsonPropertyName("validationReport")]
    public string ValidationReport { get; set; } = default!;
}
