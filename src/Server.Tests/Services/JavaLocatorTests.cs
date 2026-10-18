using Docentric.EInvoice.Validator.RestServer.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

/// <summary>
/// Tests the selection of the Java executable.
/// </summary>
public class JavaLocatorTests : IDisposable
{
    private readonly string _javaHome = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    /// <summary>
    /// Removes the temporary Java home directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_javaHome))
        {
            Directory.Delete(_javaHome, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that java from PATH is used when JAVA_HOME is not set or is blank.
    /// </summary>
    /// <param name="javaHome">The JAVA_HOME value.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WhenJavaHomeIsNotSetThenPathExecutableIsUsed(string? javaHome)
    {
        Assert.Equal("java", JavaLocator.GetExecutable(javaHome));
    }

    /// <summary>
    /// Verifies that java from PATH is used when JAVA_HOME has no java executable.
    /// </summary>
    [Fact]
    public void WhenJavaHomeHasNoExecutableThenPathExecutableIsUsed()
    {
        Directory.CreateDirectory(Path.Combine(_javaHome, "bin"));

        Assert.Equal("java", JavaLocator.GetExecutable(_javaHome));
    }

    /// <summary>
    /// Verifies that the executable in JAVA_HOME is preferred over java on PATH.
    /// </summary>
    [Fact]
    public void WhenJavaHomeHasExecutableThenItIsUsed()
    {
        string executableName = OperatingSystem.IsWindows() ? "java.exe" : "java";
        string expected = Path.Combine(_javaHome, "bin", executableName);
        Directory.CreateDirectory(Path.GetDirectoryName(expected)!);
        File.WriteAllText(expected, string.Empty);

        Assert.Equal(expected, JavaLocator.GetExecutable(_javaHome));
    }
}
