using System.Xml.Linq;

using Docentric.EInvoice.Validator.RestServer.Configuration;
using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.IO;
using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.AspNetCore.Mvc;

namespace Docentric.EInvoice.Validator.RestServer.Endpoints;

/// <summary>
/// Provides extension methods for mapping XML-related API endpoints to an ASP.NET Core application's routing
/// configuration.
/// </summary>
public static class XmlEndpoints
{
    /// <summary>
    /// Maps XML-related endpoints to the specified endpoint route builder.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder"/> to add the routes to.</param>
    /// <returns>The <see cref="IEndpointRouteBuilder"/> so that additional calls can be chained.</returns>
    public static IEndpointRouteBuilder MapXmlEndpoints(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/xml")
            .WithTags("Factur-X or UBL XML");

        group.MapPost("/validate", ValidateXmlHandler)
            .Accepts<FileUploadRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<FileValidationResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<FileValidationResponse>(StatusCodes.Status400BadRequest, "application/json")
            .WithRequestTimeout(Constants.RequestTimeouts.LongRunningPolicy);

        group.MapPost("/convert-to-pdf", ConvertXmlToPdfHandler)
            .Accepts<FileUploadRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<ConvertXmlToPdfResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ConvertXmlToPdfResponse>(StatusCodes.Status400BadRequest, "application/json")
            .WithRequestTimeout(Constants.RequestTimeouts.LongRunningPolicy);

        return endpoints;
    }

    /// <summary>
    /// Handles validation of Factur-X or UBL XML files using the Mustang CLI validator.
    /// </summary>
    /// <param name="request">The HTTP request containing the uploaded file.</param>
    /// <param name="mustangCliService">The Mustang CLI service for executing commands.</param>
    /// <param name="uploadedFile">The file upload request with the XML file to validate.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a <see cref="FileValidationResponse"/> with validation results,
    /// including whether the file is valid and a detailed validation report.
    /// </returns>
    /// <remarks>
    /// This endpoint validates Factur-X or UBL XML files by invoking the Mustang CLI tool as an external Java process.
    /// The validation report is returned as XML format within the response.
    /// </remarks>
    private static async Task<IResult> ValidateXmlHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new FileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsXmlAsync(uploadedFile.File))
            return Results.BadRequest(new FileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid XML file."
            });

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile);

            MustangCliResult mustangCliResult = await mustangCliService.ValidateAsync(temporaryFile.FilePath);

            if (mustangCliResult.ProcessStartFailed)
            {
                return Results.BadRequest(new FileValidationResponse
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

            return Results.Json(new FileValidationResponse
            {
                ErrorCode = (ErrorCode)mustangCliResult.ExitCode,
                IsValid = status == "valid",
                ValidationReport = mustangCliResult.StandardOutput,
                DiagnosticsErrorMessage = mustangCliResult.ExitCode != (int)ErrorCode.Success ? mustangCliResult.StandardError : null
            }, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new FileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = $"Failed to process the uploaded file. Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Handles conversion of Factur-X or UBL XML files to PDF documents using the Mustang CLI tool.
    /// </summary>
    /// <param name="request">The HTTP request containing the uploaded file.</param>
    /// <param name="mustangCliService">The Mustang CLI service for executing commands.</param>
    /// <param name="uploadedFile">The file upload request with the XML file to convert.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a <see cref="ConvertXmlToPdfResponse"/> with the generated PDF content
    /// if successful, or an error message if conversion fails.
    /// </returns>
    /// <remarks>
    /// This endpoint converts Factur-X or UBL XML invoice data to a PDF document by invoking the Mustang CLI tool
    /// as an external Java process. The generated PDF file is automatically cleaned up after reading.
    /// </remarks>
    private static async Task<IResult> ConvertXmlToPdfHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile)
    {
        if (!request.HasFormContentType)
            return Results.BadRequest(new ConvertXmlToPdfResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsXmlAsync(uploadedFile.File))
            return Results.BadRequest(new ConvertXmlToPdfResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid XML file."
            });

        string? outputPdf = null;

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile);

            outputPdf = Path.ChangeExtension(temporaryFile.FilePath, ".pdf");

            MustangCliResult mustangCliResult = await mustangCliService.ConvertXmlToPdfAsync(temporaryFile.FilePath, outputPdf);

            if (mustangCliResult.ProcessStartFailed)
            {
                return Results.BadRequest(new ConvertXmlToPdfResponse
                {
                    ErrorCode = ErrorCode.InternalServerError,
                    ErrorMessage = mustangCliResult.ErrorMessage
                });
            }

            int statusCode = StatusCodes.Status400BadRequest;
            if (mustangCliResult.ExitCode == 0)
                statusCode = StatusCodes.Status200OK;

            return Results.Json(new ConvertXmlToPdfResponse
            {
                ErrorCode = (ErrorCode)mustangCliResult.ExitCode,
                DiagnosticsErrorMessage = mustangCliResult.ExitCode != (int)ErrorCode.Success ? mustangCliResult.StandardError : null,
                Pdf = mustangCliResult.ExitCode == 0 && File.Exists(outputPdf)
                    ? await File.ReadAllBytesAsync(outputPdf)
                    : null,
            }, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new ConvertXmlToPdfResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = $"Failed to process the uploaded file. Error: {ex.Message}"
            });
        }
        finally
        {
            if (outputPdf != null && File.Exists(outputPdf))
            {
                try
                {
                    File.Delete(outputPdf);
                }
                catch
                {
                    // Ignore any errors during cleanup
                }
            }
        }
    }
}
