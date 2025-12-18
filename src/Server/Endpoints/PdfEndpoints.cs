using System.Xml.Linq;

using Docentric.ZuGFeRD.Validator.RestServer.Contracts;
using Docentric.ZuGFeRD.Validator.RestServer.IO;
using Docentric.ZuGFeRD.Validator.RestServer.Services;

using Microsoft.AspNetCore.Mvc;

namespace Docentric.ZuGFeRD.Validator.RestServer.Endpoints;

/// <summary>
/// Provides extension methods for mapping PDF-related API endpoints to an ASP.NET Core application's routing
/// configuration.
/// </summary>
public static class PdfEndpoints
{
    /// <summary>
    /// Maps PDF-related endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> so that additional calls can be chained.</returns>
    public static IEndpointRouteBuilder MapPdfEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/pdf")
            .WithTags("ZuGFeRD PDF");

        group.MapPost("/validate-zugferd", ValidateZuGFeRDPdfHandler)
            .Accepts<FileUploadRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<PdfFileValidationResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<PdfFileValidationResponse>(StatusCodes.Status400BadRequest, "application/json");

        group.MapPost("/extract-zugferd-xml", ExtractZuGFeRDXmlHandler)
            .Accepts<FileUploadRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<ExtractXmlFromPdfResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ExtractXmlFromPdfResponse>(StatusCodes.Status400BadRequest, "application/json");

        return endpoints;
    }

    /// <summary>
    /// Handles validation of ZuGFeRD PDF files using the Mustang CLI validator.
    /// </summary>
    /// <param name="request">The HTTP request containing the uploaded file.</param>
    /// <param name="mustangCliService">The Mustang CLI service for executing commands.</param>
    /// <param name="uploadedFile">The file upload request with the PDF file to validate.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a <see cref="PdfFileValidationResponse"/> with validation results,
    /// including whether the file is valid, signature status, and a detailed validation report.
    /// </returns>
    /// <remarks>
    /// This endpoint validates ZuGFeRD PDF files by invoking the Mustang CLI tool as an external Java process.
    /// The validation report is returned as XML format within the response.
    /// </remarks>
    public static async Task<IResult> ValidateZuGFeRDPdfHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new PdfFileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsPdfAsync(uploadedFile.File))
            return Results.BadRequest(new PdfFileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid PDF file."
            });

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile);

            MustangCliResult mustangCliResult = await mustangCliService.ValidateAsync(temporaryFile.FilePath);

            if (mustangCliResult.ProcessStartFailed)
            {
                return Results.InternalServerError(new PdfFileValidationResponse
                {
                    ErrorCode = ErrorCode.InternalServerError,
                    ErrorMessage = mustangCliResult.ErrorMessage
                });
            }

            string status = "unknown";
            try
            {
                if (!string.IsNullOrWhiteSpace(mustangCliResult.StandardOutput))
                {
                    var mustangCliXmlResult = XDocument.Parse(mustangCliResult.StandardOutput);
                    XElement? summary = mustangCliXmlResult.Root?.Descendants("summary").FirstOrDefault();
                    if (summary?.Attribute("status") is { } attributeValue)
                        status = attributeValue.Value;
                }
            }
            catch { /* keep status=unknown on parse issues */ }

            int statusCode = StatusCodes.Status400BadRequest;
            if (mustangCliResult.ExitCode == (int)ErrorCode.Success)
                statusCode = StatusCodes.Status200OK;

            return Results.Json(new PdfFileValidationResponse
            {
                ErrorCode = (ErrorCode)mustangCliResult.ExitCode,
                IsValid = status == "valid",
                IsSignatureValid = mustangCliResult.StandardOutput.Contains("<signature>valid</signature>"),
                ValidationReport = mustangCliResult.StandardOutput,
                DiagnosticsErrorMessage = mustangCliResult.ExitCode != (int)ErrorCode.Success ? mustangCliResult.StandardError : null
            }, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(new PdfFileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = $"Failed to process the uploaded file. Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handles extraction of embedded ZuGFeRD XML invoice data from PDF files using the Mustang CLI tool.
    /// </summary>
    /// <param name="request">The HTTP request containing the uploaded file.</param>
    /// <param name="mustangCliService">The Mustang CLI service for executing commands.</param>
    /// <param name="uploadedFile">The file upload request with the PDF file to extract XML from.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing an <see cref="ExtractXmlFromPdfResponse"/> with the extracted XML content
    /// if successful, or an error message if extraction fails.
    /// </returns>
    /// <remarks>
    /// This endpoint extracts the embedded ZuGFeRD XML invoice data from a PDF file by invoking the Mustang CLI tool
    /// as an external Java process. The extracted XML file is automatically cleaned up after reading.
    /// </remarks>
    public static async Task<IResult> ExtractZuGFeRDXmlHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new ExtractXmlFromPdfResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsPdfAsync(uploadedFile.File))
            return Results.BadRequest(new ExtractXmlFromPdfResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid PDF file."
            });

        string? outputXml = null;

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile);

            outputXml = Path.ChangeExtension(temporaryFile.FilePath, ".xml");

            MustangCliResult mustangCliResult = await mustangCliService.ExtractXmlAsync(temporaryFile.FilePath, outputXml);

            if (mustangCliResult.ProcessStartFailed)
            {
                return Results.BadRequest(new ExtractXmlFromPdfResponse
                {
                    ErrorCode = ErrorCode.InternalServerError,
                    ErrorMessage = mustangCliResult.ErrorMessage
                });
            }

            int statusCode = StatusCodes.Status400BadRequest;
            if (mustangCliResult.ExitCode == 0)
                statusCode = StatusCodes.Status200OK;

            return Results.Json(new ExtractXmlFromPdfResponse
            {
                ErrorCode = (ErrorCode)mustangCliResult.ExitCode,
                DiagnosticsErrorMessage = mustangCliResult.ExitCode != (int)ErrorCode.Success ? mustangCliResult.StandardError : null,
                Xml = mustangCliResult.ExitCode == 0 && outputXml != null && File.Exists(outputXml)
                    ? await File.ReadAllTextAsync(outputXml)
                    : null,
            }, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(new ExtractXmlFromPdfResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = $"Failed to process the uploaded file. Error: {ex.Message}"
            });
        }
        finally
        {
            if (outputXml != null && File.Exists(outputXml))
            {
                try
                {
                    File.Delete(outputXml);
                }
                catch
                {
                    // Ignore any errors during cleanup
                }
            }
        }
    }
}
