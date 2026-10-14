using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

public class MustangCliServiceTests
{
    private readonly MustangCliService _service = new(NullLogger<MustangCliService>.Instance);

    private static string CreateTempFile(string extension, string content)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void WhenJarIsDeployedThenGetMustangCliVersionReturnsVersion()
    {
        string? version = _service.GetMustangCliVersion();

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    [Fact]
    public async Task WhenJavaIsInstalledThenCheckAvailabilityReturnsTrue()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        bool result = await _service.CheckAvailabilityAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    [Fact]
    public async Task WhenCancellationIsRequestedThenCheckAvailabilityReturnsFalse()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        bool result = await _service.CheckAvailabilityAsync(cts.Token);

        Assert.False(result);
    }

    [Fact]
    public async Task WhenCancellationIsRequestedThenValidateReturnsProcessingTimeout()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        MustangCliResult result = await _service.ValidateAsync("missing.pdf", cts.Token);

        Assert.Equal((int)ErrorCode.ProcessingTimeout, result.ExitCode);
    }

    [Fact]
    public async Task WhenSourceFileDoesNotExistThenValidateDoesNotSucceed()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");

        MustangCliResult result = await _service.ValidateAsync(missingPath, TestContext.Current.CancellationToken);

        Assert.False(result.ProcessStartFailed);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput + result.StandardError));
    }

    [Fact]
    public async Task WhenPdfIsInvalidThenExtractXmlStillStartsProcess()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        string source = CreateTempFile(".pdf", "not a pdf");
        string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

        try
        {
            MustangCliResult result = await _service.ExtractXmlAsync(source, output, TestContext.Current.CancellationToken);

            Assert.Null(result.ErrorMessage);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }
}
