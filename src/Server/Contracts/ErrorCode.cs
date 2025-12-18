using System.ComponentModel;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// Defines error codes used during ZuGFeRD/Factur-X validation.
/// Error codes 1-27 correspond to Mustang CLI validator error types.
/// Error codes 1000+ are custom application errors.
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Operation has failed.
    /// </summary>
    [Description("Operation has failed.")]
    ExecutionError = -1,

    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    [Description("Operation completed successfully.")]
    Success = 0,

    // Mustang CLI Error Codes (1-27)
    
    /// <summary>
    /// File not found.
    /// </summary>
    [Description("File not found")]
    FileNotFound = 1,

    /// <summary>
    /// Additional data schema validation fails.
    /// </summary>
    [Description("Additional data schema validation failed")]
    AdditionalDataSchemaValidationFailed = 2,

    /// <summary>
    /// XML data not found in the file.
    /// </summary>
    [Description("XML data not found")]
    XmlDataNotFound = 3,

    /// <summary>
    /// Schematron rule failed.
    /// </summary>
    [Description("Schematron rule failed")]
    SchematronRuleFailed = 4,

    /// <summary>
    /// File too small to be valid.
    /// </summary>
    [Description("File too small")]
    FileTooSmall = 5,

    /// <summary>
    /// VeraPDF exception occurred during PDF validation.
    /// </summary>
    [Description("VeraPDF exception")]
    VeraPdfException = 6,

    /// <summary>
    /// IOException occurred while processing PDF.
    /// </summary>
    [Description("IOException while processing PDF")]
    IoExceptionPdf = 7,

    /// <summary>
    /// File does not look like PDF nor XML (contains neither %PDF nor &lt;?xml).
    /// </summary>
    [Description("File does not appear to be PDF or XML")]
    InvalidFileFormat = 8,

    /// <summary>
    /// IOException occurred while processing XML.
    /// </summary>
    [Description("IOException while processing XML")]
    IoExceptionXml = 9,

    /// <summary>
    /// Arithmetical unreproducability - Mustang calculates different sums and/or totals.
    /// </summary>
    [Description("Calculated sums/totals do not match (arithmetical error)")]
    ArithmeticalError = 10,

    /// <summary>
    /// XMP Metadata: ConformanceLevel not found.
    /// </summary>
    [Description("XMP Metadata: ConformanceLevel not found")]
    XmpConformanceLevelNotFound = 11,

    /// <summary>
    /// XMP Metadata: ConformanceLevel contains invalid value.
    /// </summary>
    [Description("XMP Metadata: ConformanceLevel contains invalid value")]
    XmpConformanceLevelInvalid = 12,

    /// <summary>
    /// XMP Metadata: DocumentType not found.
    /// </summary>
    [Description("XMP Metadata: DocumentType not found")]
    XmpDocumentTypeNotFound = 13,

    /// <summary>
    /// XMP Metadata: DocumentType invalid.
    /// </summary>
    [Description("XMP Metadata: DocumentType invalid")]
    XmpDocumentTypeInvalid = 14,

    /// <summary>
    /// XMP Metadata: Version not found.
    /// </summary>
    [Description("XMP Metadata: Version not found")]
    XmpVersionNotFound = 15,

    /// <summary>
    /// XMP Metadata: Version contains invalid value.
    /// </summary>
    [Description("XMP Metadata: Version contains invalid value")]
    XmpVersionInvalid = 16,

    /// <summary>
    /// Context error - parts may be valid but not in this combination.
    /// </summary>
    [Description("Context error - parts may be valid but not in this combination")]
    ContextError = 17,

    /// <summary>
    /// Schema validation failed.
    /// </summary>
    [Description("Schema validation failed")]
    SchemaValidationFailed = 18,

    /// <summary>
    /// XMP Metadata: DocumentFileName contains invalid value.
    /// </summary>
    [Description("XMP Metadata: DocumentFileName contains invalid value")]
    XmpDocumentFileNameInvalid = 19,

    /// <summary>
    /// Not a PDF file.
    /// </summary>
    [Description("Not a PDF file")]
    NotAPdf = 20,

    /// <summary>
    /// XMP Metadata: DocumentFileName not found.
    /// </summary>
    [Description("XMP Metadata: DocumentFileName not found")]
    XmpDocumentFileNameNotFound = 21,

    /// <summary>
    /// Generic XML validation exception.
    /// </summary>
    [Description("Generic XML validation exception")]
    GenericXmlValidationException = 22,

    /// <summary>
    /// Not a PDF/A-3 file.
    /// </summary>
    [Description("Not a PDF/A-3 file")]
    NotAPdfA3 = 23,

    /// <summary>
    /// Issues in CEN EN16931 Schematron Check.
    /// </summary>
    [Description("Issues in CEN EN16931 Schematron Check")]
    En16931SchematronCheckFailed = 24,

    /// <summary>
    /// Unsupported profile type.
    /// </summary>
    [Description("Unsupported profile type")]
    UnsupportedProfileType = 25,

    /// <summary>
    /// No rules matched, XML too minimal.
    /// </summary>
    [Description("No rules matched - XML may be too minimal")]
    NoRulesMatched = 26,

    /// <summary>
    /// XRechnung schematron validation failed.
    /// </summary>
    [Description("XRechnung schematron validation failed")]
    XRechnungSchematronValidationFailed = 27,

    // Custom Application Error Codes (1000+)

    /// <summary>
    /// Unknown or unspecified error occurred.
    /// </summary>
    [Description("Unknown error occurred")]
    UnknownError = 1000,

    /// <summary>
    /// Invalid request or missing required parameters.
    /// </summary>
    [Description("Invalid request or missing required parameters")]
    InvalidRequest = 1001,

    /// <summary>
    /// Processing timeout occurred.
    /// </summary>
    [Description("Processing timeout")]
    ProcessingTimeout = 1002,

    /// <summary>
    /// Internal server error during validation.
    /// </summary>
    [Description("Internal server error")]
    InternalServerError = 1003
}

/// <summary>
/// Extension methods for the <see cref="ErrorCode"/> enum.
/// </summary>
public static class ErrorCodeExtensions
{
    /// <summary>
    /// Gets a friendly, human-readable error message for the specified error code.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>A friendly error message describing the error.</returns>
    public static string GetFriendlyMessage(this ErrorCode errorCode)
    {
        var field = errorCode.GetType().GetField(errorCode.ToString());
        if (field != null)
        {
            var attribute = (DescriptionAttribute?)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            if (attribute != null)
            {
                return attribute.Description;
            }
        }
        return errorCode.ToString();
    }

    /// <summary>
    /// Determines whether the error code represents a Mustang CLI validation error (codes 1-27).
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns><c>true</c> if the error code is from Mustang CLI; otherwise, <c>false</c>.</returns>
    public static bool IsMustangError(this ErrorCode errorCode)
    {
        int code = (int)errorCode;
        return code >= 1 && code <= 27;
    }

    /// <summary>
    /// Determines whether the error code represents a custom application error (codes 1000+).
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns><c>true</c> if the error code is a custom application error; otherwise, <c>false</c>.</returns>
    public static bool IsCustomError(this ErrorCode errorCode)
    {
        int code = (int)errorCode;
        return code >= 1000;
    }
}
