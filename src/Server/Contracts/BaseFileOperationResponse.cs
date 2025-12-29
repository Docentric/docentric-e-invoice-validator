using System.Text.Json.Serialization;

namespace Docentric.EInvoice.Validator.RestServer.Contracts;

/// <summary>
/// Represents the base response for file operations performed by the ZuGFeRD/Factur-X/UBL validator.
/// This class provides standard success/error information that can be extended by specific operation responses.
/// </summary>
public class BaseFileOperationResponse
{
    /// <summary>
    /// Indicates whether the extraction was successful.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success => ErrorCode == ErrorCode.Success;

    /// <summary>
    /// The error code associated with the extraction result.
    /// A value of <see cref="Contracts.ErrorCode.Success"/> indicates no error.
    /// </summary>
    [JsonPropertyName("errorCode")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ErrorCode ErrorCode { get; set; }

    private string _errorMessage = string.Empty;
    /// <summary>
    /// The error message describing any extraction failures or issues.
    /// </summary>
    [JsonPropertyName("errorMessage")]
    public string ErrorMessage
    {
        get => string.IsNullOrWhiteSpace(_errorMessage) && ErrorCode != ErrorCode.Success
            ? ErrorCode.GetFriendlyMessage()
            : _errorMessage;
        set => _errorMessage = value;
    }

    /// <summary>
    /// Additional diagnostics message
    /// </summary>
    [JsonPropertyName("diagnosticsErrorMessage")]
    public string? DiagnosticsErrorMessage { get; set; } = null;
}
