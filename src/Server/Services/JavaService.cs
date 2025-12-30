using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Provides services for querying Java runtime information on the system.
/// </summary>
public sealed partial class JavaService(ILogger<JavaService> logger)
{
    private const int JavaProcessTimeoutMs = 10_000;

    /// <summary>
    /// Retrieves information about the Java runtime installed on the system.
    /// </summary>
    /// <returns>
    /// A <see cref="JavaInfoResult"/> indicating whether Java is available and containing
    /// any discovered Java system properties.
    /// </returns>
    public async Task<JavaInfoResult> GetJavaInfoAsync()
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        ProcessStartInfo processStartInfo = new()
        {
            FileName = "java",
            Arguments = "-XshowSettings:properties -version",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(JavaProcessTimeoutMs));

        using var process = Process.Start(processStartInfo);

        if (process == null)
        {
            return new JavaInfoResult(IsAvailable: false, properties);
        }

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();

        try
        {
            await process.WaitForExitAsync(cts.Token);

            string allOutput = stdout + Environment.NewLine + stderr;
            properties = ParseJavaProperties(allOutput);

            // If we got here, java is at least callable
            return new JavaInfoResult(IsAvailable: true, properties);
        }
        catch (OperationCanceledException)
        {
            if (process?.HasExited == false)
                process?.Kill(entireProcessTree: true);

            return new JavaInfoResult(IsAvailable: false, properties);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Failed to execute Java process.");
            // Any failure => not available, empty properties
            return new JavaInfoResult(IsAvailable: false, properties);
        }
    }

    /// <summary>
    /// Parses Java system properties from the output of 'java -XshowSettings:properties -version'.
    /// </summary>
    /// <param name="output">The raw output text from the Java process.</param>
    /// <returns>A dictionary of property names and their values.</returns>
    private static Dictionary<string, string> ParseJavaProperties(string output)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StringReader(output);
        string? line;

        // Matches lines like:
        //   java.runtime.version = 25.0.0.36
        //   'java.home' = 'C:\Program Files\Java\...'
        Regex regex = JavaPropertyLineRegex();

        while ((line = reader.ReadLine()) is not null)
        {
            Match match = regex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string key = match.Groups[1].Value.Trim();
            string value = match.Groups[2].Value.Trim();

            if (key.Length == 0)
            {
                continue;
            }

            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Gets a compiled regular expression for matching Java property lines.
    /// </summary>
    /// <returns>A regex that captures property name and value from Java output lines.</returns>
    [GeneratedRegex(@"^\s*'?([\w\.\-]+)'?\s*=\s*'?(.+?)'?\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex JavaPropertyLineRegex();
}
