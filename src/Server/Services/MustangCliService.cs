using System.Diagnostics;

using Docentric.EInvoice.Validator.RestServer.Contracts;

namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Provides methods for executing Mustang CLI commands to validate and extract ZuGFeRD/Factur-X data.
/// </summary>
/// <param name="logger">The logger instance for logging information and errors.</param>
public sealed class MustangCliService(ILogger<MustangCliService> logger)
{
    /// <summary>
    /// The version of Mustang CLI being used.
    /// </summary>
    public const string MustangCliVersion = "2.21.0";
    private const string MustangJarFile = $"Mustang-CLI-{MustangCliVersion}.jar";
    private const string JavaMaxMemory = "-Xmx1G";
    private const string FileEncoding = "-Dfile.encoding=UTF-8";

    /// <summary>
    /// Validates a ZuGFeRD/Factur-X PDF file.
    /// </summary>
    /// <param name="sourceFilePath">The path to the PDF file to validate.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>A <see cref="MustangCliResult"/> containing validation results.</returns>
    public Task<MustangCliResult> ValidateAsync(string sourceFilePath, CancellationToken cancellationToken)
        => ExecuteAsync("validate", ["--source", sourceFilePath], cancellationToken);

    /// <summary>
    /// Extracts embedded XML from a ZuGFeRD/Factur-X PDF file.
    /// </summary>
    /// <param name="sourceFilePath">The path to the PDF file.</param>
    /// <param name="outputFilePath">The path where the extracted XML should be saved.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>A <see cref="MustangCliResult"/> containing extraction results.</returns>
    public Task<MustangCliResult> ExtractXmlAsync(string sourceFilePath, string outputFilePath, CancellationToken cancellationToken)
        => ExecuteAsync("extract", ["--source", sourceFilePath, "--out", outputFilePath], cancellationToken);

    /// <summary>
    /// Converts Factur-X or UBL XML file to a PDF document.
    /// </summary>
    /// <param name="sourceFilePath">The path to the XML file.</param>
    /// <param name="outputFilePath">The path where the generated PDF should be saved.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>A <see cref="MustangCliResult"/> containing conversion results.</returns>
    public Task<MustangCliResult> ConvertXmlToPdfAsync(string sourceFilePath, string outputFilePath, CancellationToken cancellationToken)
        => ExecuteAsync("pdf", ["--source", sourceFilePath, "--out", outputFilePath], cancellationToken);


    /// <summary>
    /// Checks if Mustang CLI is available by executing the help command.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>True if Mustang CLI executed successfully, false otherwise.</returns>
    public async Task<bool> CheckAvailabilityAsync(CancellationToken cancellationToken)
    {
        try
        {
            MustangCliResult result = await ExecuteAsync("help", [], cancellationToken);
            return result.IsSuccess;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Executes a Mustang CLI command with the specified action and arguments.
    /// </summary>
    /// <param name="action">The Mustang CLI action to perform (e.g., "validate", "extract", "help").</param>
    /// <param name="additionalArguments">Additional command-line arguments to pass to Mustang CLI.</param>
    /// <param name="cancellationToken">The cancellation token for managing task cancellation.</param>
    /// <returns>A <see cref="MustangCliResult"/> containing the exit code, stdout, and stderr.</returns>
    private async Task<MustangCliResult> ExecuteAsync(string action, string[] additionalArguments, CancellationToken cancellationToken)
    {
        ProcessStartInfo processStartInfo = new()
        {
            FileName = "java",
            ArgumentList = { JavaMaxMemory, FileEncoding, "-jar", MustangJarFile },
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory
        };

        // Add common Mustang CLI flags
        processStartInfo.ArgumentList.Add("--no-notices");
        processStartInfo.ArgumentList.Add("--disable-file-logging");

        // Add action
        processStartInfo.ArgumentList.Add("--action");
        processStartInfo.ArgumentList.Add(action);

        // Add additional arguments
        foreach (string arg in additionalArguments)
        {
            processStartInfo.ArgumentList.Add(arg);
        }

        using var process = Process.Start(processStartInfo);

        if (process == null)
        {
            return MustangCliResult.Failed($"Failed to start Java process with Mustang CLI (action: {action}).");
        }

        string stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        string stderr = await process.StandardError.ReadToEndAsync(cancellationToken);

        try
        {
            await process.WaitForExitAsync(cancellationToken);

            return new MustangCliResult
            {
                ExitCode = process.ExitCode,
                StandardOutput = stdout,
                StandardError = stderr
            };
        }
        catch (OperationCanceledException oce)
        {
            logger.LogError(oce, "Operation was cancelled.");
            if (process.HasExited == false)
                process.Kill(entireProcessTree: true);

            return new MustangCliResult
            {
                ExitCode = (int)ErrorCode.ProcessingTimeout,
                StandardOutput = stdout,
                StandardError = stderr
            };
        }
    }
}
