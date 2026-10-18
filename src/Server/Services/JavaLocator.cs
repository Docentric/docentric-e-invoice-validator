namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Selects the Java executable that the validator launches.
/// </summary>
/// <remarks>
/// The selection must match the Java install scripts in <c>scripts/java</c>: the executable in
/// <c>JAVA_HOME</c> is used when it exists, otherwise <c>java</c> is resolved from <c>PATH</c>.
/// This keeps an older runtime earlier on <c>PATH</c> from being used after the scripts install a newer one.
/// </remarks>
public static class JavaLocator
{
    private const string JavaHomeVariable = "JAVA_HOME";
    private const string PathExecutable = "java";

    /// <summary>
    /// Gets the Java executable to launch, based on the <c>JAVA_HOME</c> environment variable.
    /// </summary>
    /// <returns>The full path to the Java executable in <c>JAVA_HOME</c>, or <c>java</c> when it is not available.</returns>
    public static string GetExecutable() => GetExecutable(Environment.GetEnvironmentVariable(JavaHomeVariable));

    /// <summary>
    /// Gets the Java executable to launch for the given Java home directory.
    /// </summary>
    /// <param name="javaHome">The value of <c>JAVA_HOME</c>. It can be <see langword="null"/> or empty.</param>
    /// <returns>The full path to the Java executable in <paramref name="javaHome"/>, or <c>java</c> when it does not exist.</returns>
    public static string GetExecutable(string? javaHome)
    {
        if (string.IsNullOrWhiteSpace(javaHome))
        {
            return PathExecutable;
        }

        string executableName = OperatingSystem.IsWindows() ? "java.exe" : PathExecutable;
        string candidate = Path.Combine(javaHome, "bin", executableName);

        return File.Exists(candidate) ? candidate : PathExecutable;
    }
}
