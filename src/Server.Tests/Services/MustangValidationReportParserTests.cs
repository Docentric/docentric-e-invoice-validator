using System.Xml;

using Docentric.EInvoice.Validator.RestServer.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

/// <summary>
/// Tests how MustangValidationReportParser extracts statuses from a Mustang validation report.
/// </summary>
public class MustangValidationReportParserTests
{
    private static string Report(string overall, string? pdf, string? xml, string extra = "") =>
        $"""
        <validation>
          <pdf>{(pdf is null ? "" : $"<summary status=\"{pdf}\"/>")}</pdf>
          <xml>{(xml is null ? "" : $"<summary status=\"{xml}\"/>")}</xml>
          {extra}
          <summary status="{overall}"/>
        </validation>
        """;

    /// <summary>
    /// Verifies that the overall, PDF and XML statuses are returned when all sections are reported.
    /// </summary>
    /// <param name="pdf">The PDF status in the report.</param>
    /// <param name="overall">The overall status in the report.</param>
    /// <param name="xml">The XML status in the report.</param>
    [Theory]
    [InlineData("valid", "valid", "valid")]
    [InlineData("invalid", "valid", "invalid")]
    [InlineData("invalid", "invalid", "valid")]
    [InlineData("invalid", "invalid", "invalid")]
    public void WhenAllSectionsReportedThenStatusesAreReturned(string overall, string pdf, string xml)
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report(overall, pdf, xml));

        Assert.Equal(new MustangValidationStatuses(overall, pdf, xml, "unknown"), result);
    }

    /// <summary>
    /// Verifies that an invalid PDF section does not change a valid overall status.
    /// </summary>
    [Fact]
    public void WhenPdfInvalidButOverallValidThenPdfIsInvalidAndOverallIsValid()
    {
        // BR-FX-DE-03: PDF/A errors are warnings for German invoices.
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "invalid", "valid"));

        Assert.Equal(new MustangValidationStatuses("valid", "invalid", "valid", "unknown"), result);
    }

    /// <summary>
    /// Verifies that a missing PDF section results in an unknown PDF status.
    /// </summary>
    [Fact]
    public void WhenPdfSectionMissingThenPdfStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", null, "valid"));

        Assert.Equal("unknown", result.Pdf);
    }

    /// <summary>
    /// Verifies that a missing XML section results in an unknown XML status.
    /// </summary>
    [Fact]
    public void WhenXmlSectionMissingThenXmlStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", null));

        Assert.Equal("unknown", result.Xml);
    }

    /// <summary>
    /// Verifies that a nested summary is not mistaken for the overall status.
    /// </summary>
    [Fact]
    public void WhenOnlyNestedSummaryExistsThenOverallStatusIsUnknown()
    {
        const string report = """<validation><pdf><nested><summary status="valid"/></nested></pdf></validation>""";

        MustangValidationStatuses result = MustangValidationReportParser.Parse(report);

        Assert.Equal("unknown", result.Overall);
    }

    /// <summary>
    /// Verifies that a null or whitespace report results in unknown statuses.
    /// </summary>
    /// <param name="report">The report text under test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WhenReportIsNullOrWhitespaceThenAllStatusesAreUnknown(string? report)
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(report);

        Assert.Equal(MustangValidationStatuses.Unknown, result);
    }

    /// <summary>
    /// Verifies that the signature status is returned when the signature element is present.
    /// </summary>
    /// <param name="signature">The signature status in the report.</param>
    [Theory]
    [InlineData("valid")]
    [InlineData("invalid")]
    public void WhenSignatureElementPresentThenSignatureStatusIsReturned(string signature)
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", "valid", $"<signature>{signature}</signature>"));

        Assert.Equal(signature, result.Signature);
    }

    /// <summary>
    /// Verifies that whitespace around the signature status is trimmed.
    /// </summary>
    [Fact]
    public void WhenSignatureElementHasSurroundingWhitespaceThenItIsTrimmed()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", "valid", "<signature> valid </signature>"));

        Assert.Equal("valid", result.Signature);
    }

    /// <summary>
    /// Verifies that a missing signature element results in an unknown signature status.
    /// </summary>
    [Fact]
    public void WhenSignatureElementMissingThenSignatureStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", "valid"));

        Assert.Equal("unknown", result.Signature);
    }

    /// <summary>
    /// Verifies that an empty signature element results in an unknown signature status.
    /// </summary>
    [Fact]
    public void WhenSignatureElementEmptyThenSignatureStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", "valid", "<signature/>"));

        Assert.Equal("unknown", result.Signature);
    }

    /// <summary>
    /// Verifies that a malformed report throws an XmlException.
    /// </summary>
    [Fact]
    public void WhenReportIsMalformedThenXmlExceptionIsThrown() => Assert.ThrowsAny<XmlException>(() => MustangValidationReportParser.Parse("<validation>"));
}
