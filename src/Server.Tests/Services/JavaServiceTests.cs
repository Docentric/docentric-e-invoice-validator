using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

/// <summary>
/// Tests JavaService against the Java runtime installed on the machine. Tests are skipped when Java is not available.
/// </summary>
public class JavaServiceTests
{
    private readonly JavaService _service = new(NullLogger<JavaService>.Instance);

    /// <summary>
    /// Verifies that Java is reported as available together with its system properties.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenGetJavaInfoReturnsAvailableWithProperties()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        JavaInfoResult result = await _service.GetJavaInfoAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsAvailable);
        Assert.NotEmpty(result.Properties);
    }

    /// <summary>
    /// Verifies that the returned Java properties contain java.version.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenGetJavaInfoContainsJavaVersionProperty()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        JavaInfoResult result = await _service.GetJavaInfoAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Properties.ContainsKey("java.version"));
    }

    /// <summary>
    /// Verifies that a canceled request reports Java as not available.
    /// </summary>
    [Fact]
    public async Task WhenCancellationIsRequestedThenGetJavaInfoReturnsNotAvailable()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        JavaInfoResult result = await _service.GetJavaInfoAsync(cts.Token);

        Assert.False(result.IsAvailable);
    }
}
