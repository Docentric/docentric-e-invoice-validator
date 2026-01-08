# ErrorCode Usage Examples

This document provides examples of how to use the `ErrorCode` enum in the ZuGFeRD Validator REST API.

## Overview

The `ErrorCode` enum contains validation error codes from two sources:
- **Mustang CLI errors (1-27)**: Standard validation errors from the Mustang validator
- **Custom application errors (1000+)**: Application-specific errors

## API Response Format

When validation fails, the API returns a `FileValidationResponse` with an error code:

```json
{
  "success": false,
  "errorCode": "FileNotFound",
  "errorMessage": "File not found",
  "validationReport": null
}
```

The `errorCode` field is serialized as a string (e.g., "FileNotFound") for better API usability.

## C# Usage Examples

### Getting a Friendly Error Message

```csharp
using Docentric.EInvoice.Validator.RestServer.Contracts;

var errorCode = ErrorCode.FileNotFound;
string friendlyMessage = errorCode.GetFriendlyMessage();
// Output: "File not found"
```

### Checking Error Type

```csharp
var errorCode = ErrorCode.SchematronRuleFailed;

if (errorCode.IsMustangError())
{
    Console.WriteLine("This is a Mustang CLI validation error");
}

if (errorCode.IsCustomError())
{
    Console.WriteLine("This is a custom application error");
}
```

### Creating a Validation Response

```csharp
var response = new FileValidationResponse
{
    Success = false,
    ErrorCode = ErrorCode.XmlDataNotFound,
    ErrorMessage = ErrorCode.XmlDataNotFound.GetFriendlyMessage(),
    ValidationReport = null
};
```

## Common Error Codes

### File-Related Errors
- `FileNotFound` (1): The specified file could not be found
- `FileTooSmall` (5): File is too small to be a valid ZuGFeRD/Factur-X document
- `InvalidFileFormat` (8): File doesn't appear to be PDF or XML

### PDF-Related Errors
- `NotAPdf` (20): File is not a valid PDF
- `NotAPdfA3` (23): PDF is not in PDF/A-3 format (required for ZuGFeRD)
- `VeraPdfException` (6): PDF validation failed

### XML/Schema Errors
- `XmlDataNotFound` (3): No embedded XML found in PDF
- `SchemaValidationFailed` (18): XML doesn't conform to schema
- `SchematronRuleFailed` (4): Business rule validation failed

### Validation Errors
- `ArithmeticalError` (10): Calculated totals don't match document totals
- `En16931SchematronCheckFailed` (24): EN16931 standard validation failed
- `XRechnungSchematronValidationFailed` (27): XRechnung validation failed

### XMP Metadata Errors
- `XmpConformanceLevelNotFound` (11): Required conformance level not found
- `XmpVersionNotFound` (15): Required version not found
- `XmpDocumentTypeNotFound` (13): Required document type not found

### Custom Application Errors
- `UnknownError` (1000): Unspecified error
- `InvalidRequest` (1001): Invalid request parameters
- `ProcessingTimeout` (1002): Processing took too long
- `InternalServerError` (1003): Server error during processing

## JSON/Form Deserialization

When sending requests to the API, error codes can be specified as:

### JSON Request
```json
{
  "errorCode": "FileNotFound"
}
```

Or as numeric values:
```json
{
  "errorCode": 1
}
```

Both formats are supported thanks to `JsonStringEnumConverter`.

### HTML Form
```html
<select name="errorCode">
  <option value="0">None</option>
  <option value="1">FileNotFound</option>
  <option value="3">XmlDataNotFound</option>
  <!-- ... -->
</select>
```

## Error Code Reference Table

| Code | Name | Description |
|------|------|-------------|
| 0 | None | No error |
| 1 | FileNotFound | File not found |
| 2 | AdditionalDataSchemaValidationFailed | Additional data schema validation failed |
| 3 | XmlDataNotFound | XML data not found |
| 4 | SchematronRuleFailed | Schematron rule failed |
| 5 | FileTooSmall | File too small |
| 6 | VeraPdfException | VeraPDF exception |
| 7 | IoExceptionPdf | IOException while processing PDF |
| 8 | InvalidFileFormat | File does not appear to be PDF or XML |
| 9 | IoExceptionXml | IOException while processing XML |
| 10 | ArithmeticalError | Calculated sums/totals do not match |
| 11-27 | [Various XMP/Validation Errors] | See code documentation |
| 1000+ | [Custom Application Errors] | Application-specific errors |

For the complete list of error codes and their descriptions, refer to the `ErrorCode` enum in the source code.
