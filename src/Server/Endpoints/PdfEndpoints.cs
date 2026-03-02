using System.Xml.Linq;

using Docentric.EInvoice.Validator.RestServer.Configuration;
using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.IO;
using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.AspNetCore.Mvc;

namespace Docentric.EInvoice.Validator.RestServer.Endpoints;

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
            .Produces<PdfFileValidationResponse>(StatusCodes.Status400BadRequest, "application/json")
            .Produces(StatusCodes.Status408RequestTimeout)
            .WithRequestTimeout(Constants.RequestTimeouts.LongRunningPolicy);

        group.MapPost("/extract-zugferd-xml", ExtractZuGFeRDXmlHandler)
            .Accepts<FileUploadRequest>("multipart/form-data")
            .DisableAntiforgery()
            .Produces<ExtractXmlFromPdfResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ExtractXmlFromPdfResponse>(StatusCodes.Status400BadRequest, "application/json")
            .Produces(StatusCodes.Status408RequestTimeout)
            .WithRequestTimeout(Constants.RequestTimeouts.LongRunningPolicy);

        return endpoints;
    }

    /// <summary>
    /// Handles validation of ZuGFeRD PDF files using the Mustang CLI validator.
    /// </summary>
    /// <param name="request">The HTTP request containing the uploaded file.</param>
    /// <param name="mustangCliService">The Mustang CLI service for executing commands.</param>
    /// <param name="uploadedFile">The file upload request with the PDF file to validate.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing a <see cref="PdfFileValidationResponse"/> with validation results,
    /// including whether the file is valid, signature status, and a detailed validation report.
    /// </returns>
    /// <remarks>
    /// This endpoint validates ZuGFeRD PDF files by invoking the Mustang CLI tool as an external Java process.
    /// The validation report is returned as XML format within the response.
    /// </remarks>
    private static async Task<IResult> ValidateZuGFeRDPdfHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger(nameof(ValidateZuGFeRDPdfHandler));
        if (!request.HasFormContentType)
            return Results.BadRequest(new PdfFileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsPdfAsync(uploadedFile.File, cancellationToken))
            return Results.BadRequest(new PdfFileValidationResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid PDF file."
            });

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile, cancellationToken);

            MustangCliResult mustangCliResult = await mustangCliService.ValidateAsync(temporaryFile.FilePath, cancellationToken);

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
            catch (Exception ex)
            {
                /* keep status=unknown on parse issues */
                logger.LogError(ex, "Failed to parse Mustang CLI XML output");
            }

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
            logger.LogError(ex, "Failed to validate ZuGFeRD PDF file");
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
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>
    /// An <see cref="IResult"/> containing an <see cref="ExtractXmlFromPdfResponse"/> with the extracted XML content
    /// if successful, or an error message if extraction fails.
    /// </returns>
    /// <remarks>
    /// This endpoint extracts the embedded ZuGFeRD XML invoice data from a PDF file by invoking the Mustang CLI tool
    /// as an external Java process. The extracted XML file is automatically cleaned up after reading.
    /// </remarks>
    private static async Task<IResult> ExtractZuGFeRDXmlHandler(HttpRequest request, MustangCliService mustangCliService, [FromForm] FileUploadRequest uploadedFile, ILoggerFactory loggerFactory, CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger(nameof(ExtractZuGFeRDXmlHandler));
        if (!request.HasFormContentType)
            return Results.BadRequest(new ExtractXmlFromPdfResponse
            {
                ErrorCode = ErrorCode.InvalidRequest,
                ErrorMessage = "The request content type must be 'multipart/form-data'."
            });

        if (!await FileTypeValidator.IsPdfAsync(uploadedFile.File, cancellationToken))
            return Results.BadRequest(new ExtractXmlFromPdfResponse
            {
                ErrorCode = ErrorCode.InvalidFileFormat,
                ErrorMessage = "The uploaded file is not a valid PDF file."
            });

        string? outputXml = null;

        try
        {
            await using TemporaryUploadedFile temporaryFile = await TemporaryUploadedFile.CreateAsync(uploadedFile, cancellationToken);

            outputXml = Path.ChangeExtension(temporaryFile.FilePath, ".xml");

            MustangCliResult mustangCliResult = await mustangCliService.ExtractXmlAsync(temporaryFile.FilePath, outputXml, cancellationToken);

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
                Xml = mustangCliResult.ExitCode == 0 && File.Exists(outputXml)
                    ? await File.ReadAllTextAsync(outputXml, cancellationToken)
                    : null,
            }, statusCode: statusCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to extract XML from ZuGFeRD PDF file");
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
                catch(Exception ex)
                {
                    logger.LogError(ex, "Error during cleanup of temporary XML file on the path: {Path}.", outputXml);
                    // Ignore any errors during cleanup
                }
            }
        }
    }
}
