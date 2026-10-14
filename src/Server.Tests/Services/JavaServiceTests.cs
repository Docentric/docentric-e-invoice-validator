using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

public class JavaServiceTests
{
    private readonly JavaService _service = new(NullLogger<JavaService>.Instance);

    [Fact]
    public async Task WhenJavaIsInstalledThenGetJavaInfoReturnsAvailableWithProperties()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        JavaInfoResult result = await _service.GetJavaInfoAsync(TestContext.Current.CancellationToken);

        Assert.True(result.IsAvailable);
        Assert.NotEmpty(result.Properties);
    }

    [Fact]
    public async Task WhenJavaIsInstalledThenGetJavaInfoContainsJavaVersionProperty()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        JavaInfoResult result = await _service.GetJavaInfoAsync(TestContext.Current.CancellationToken);

        Assert.True(result.Properties.ContainsKey("java.version"));
    }

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
