namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Represents the result of a Mustang CLI command execution.
/// </summary>
public sealed class MustangCliResult
{
    /// <summary>
    /// The exit code returned by the Mustang CLI process.
    /// </summary>
    public int ExitCode { get; init; }

    /// <summary>
    /// The standard output from the Mustang CLI process.
    /// </summary>
    public string StandardOutput { get; init; } = string.Empty;

    /// <summary>
    /// The standard error from the Mustang CLI process.
    /// </summary>
    public string StandardError { get; init; } = string.Empty;

    /// <summary>
    /// Indicates whether the command executed successfully (exit code 0).
    /// </summary>
    public bool IsSuccess => ExitCode == 0;

    /// <summary>
    /// Error message if the process failed to start.
    /// </summary>
    public string ErrorMessage { get; init; } = null!;

    /// <summary>
    /// Indicates whether the process failed to start.
    /// </summary>
    public bool ProcessStartFailed => ErrorMessage != null;

    /// <summary>
    /// Creates a failed result when the process couldn't be started.
    /// </summary>
    internal static MustangCliResult Failed(string errorMessage) => new()
    {
        ExitCode = -1,
        ErrorMessage = errorMessage
    };
}
