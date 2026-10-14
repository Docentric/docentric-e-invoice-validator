using Docentric.EInvoice.Validator.RestServer.Contracts;
using Docentric.EInvoice.Validator.RestServer.Services;

using Microsoft.Extensions.Logging.Abstractions;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

/// <summary>
/// Tests MustangCliService against the Mustang CLI jar deployed next to the tests. Tests that start Java are skipped when Java is not available.
/// </summary>
public class MustangCliServiceTests
{
    private readonly MustangCliService _service = new(NullLogger<MustangCliService>.Instance);

    private static string CreateTempFile(string extension, string content)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + extension);
        File.WriteAllText(path, content);
        return path;
    }

    /// <summary>
    /// Verifies that the version of the deployed Mustang CLI jar is returned.
    /// </summary>
    [Fact]
    public void WhenJarIsDeployedThenGetMustangCliVersionReturnsVersion()
    {
        string? version = _service.GetMustangCliVersion();

        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    /// <summary>
    /// Verifies that Mustang CLI is reported as available when Java is installed.
    /// </summary>
    [Fact]
    public async Task WhenJavaIsInstalledThenCheckAvailabilityReturnsTrue()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        bool result = await _service.CheckAvailabilityAsync(TestContext.Current.CancellationToken);

        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a canceled availability check reports Mustang CLI as not available.
    /// </summary>
    [Fact]
    public async Task WhenCancellationIsRequestedThenCheckAvailabilityReturnsFalse()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        bool result = await _service.CheckAvailabilityAsync(cts.Token);

        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a canceled validation returns the ProcessingTimeout exit code.
    /// </summary>
    [Fact]
    public async Task WhenCancellationIsRequestedThenValidateReturnsProcessingTimeout()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        MustangCliResult result = await _service.ValidateAsync("missing.pdf", cts.Token);

        Assert.Equal((int)ErrorCode.ProcessingTimeout, result.ExitCode);
    }

    /// <summary>
    /// Verifies that validating a missing file starts the process and returns diagnostic output.
    /// </summary>
    [Fact]
    public async Task WhenSourceFileDoesNotExistThenValidateDoesNotSucceed()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        string missingPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");

        MustangCliResult result = await _service.ValidateAsync(missingPath, TestContext.Current.CancellationToken);

        Assert.False(result.ProcessStartFailed);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardOutput + result.StandardError));
    }

    /// <summary>
    /// Verifies that extracting XML from an invalid PDF still starts the Mustang CLI process.
    /// </summary>
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

    /// <summary>
    /// Verifies that a valid Factur-X invoice is validated successfully.
    /// </summary>
    [Fact]
    public async Task WhenFacturXXmlIsValidThenValidateSucceeds()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        MustangCliResult result = await _service.ValidateAsync(SampleFiles.FacturXXmlPath, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Verifies that a valid ZUGFeRD PDF is validated successfully.
    /// </summary>
    [Fact]
    public async Task WhenZugferdPdfIsValidThenValidateSucceeds()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);

        MustangCliResult result = await _service.ValidateAsync(SampleFiles.ZugferdPdfPath, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    /// <summary>
    /// Verifies that the XML embedded in a valid ZUGFeRD PDF is extracted to the output file.
    /// </summary>
    [Fact]
    public async Task WhenZugferdPdfIsValidThenExtractXmlCreatesOutputFile()
    {
        Assert.SkipUnless(JavaTestHelper.IsJavaInstalled, JavaTestHelper.SkipReason);
        string output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".xml");

        try
        {
            await _service.ExtractXmlAsync(SampleFiles.ZugferdPdfPath, output, TestContext.Current.CancellationToken);

            Assert.True(new FileInfo(output).Length > 0);
        }
        finally
        {
            File.Delete(output);
        }
    }
}
