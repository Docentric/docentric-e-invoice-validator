using System.Diagnostics;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

internal static class JavaTestHelper
{
    public const string SkipReason = "Java is not installed or not available on PATH.";

    public static bool IsJavaInstalled { get; } = DetectJava();

    private static bool DetectJava()
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = "java",
                Arguments = "-version",
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using Process? process = Process.Start(startInfo);
            if (process is null)
                return false;

            if (!process.WaitForExit(TimeSpan.FromSeconds(10)))
            {
                process.Kill(entireProcessTree: true);
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
