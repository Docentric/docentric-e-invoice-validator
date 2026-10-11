using System.Xml;

using Docentric.EInvoice.Validator.RestServer.Services;

namespace Docentric.EInvoice.Validator.RestServer.Tests.Services;

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

    [Theory]
    [InlineData("valid", "valid", "valid")]
    [InlineData("invalid", "valid", "invalid")]
    [InlineData("invalid", "invalid", "valid")]
    [InlineData("invalid", "invalid", "invalid")]
    public void WhenAllSectionsReportedThenStatusesAreReturned(string overall, string pdf, string xml)
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report(overall, pdf, xml));

        Assert.Equal(new MustangValidationStatuses(overall, pdf, xml), result);
    }

    [Fact]
    public void WhenPdfInvalidButOverallValidThenPdfIsInvalidAndOverallIsValid()
    {
        // BR-FX-DE-03: PDF/A errors are warnings for German invoices.
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "invalid", "valid"));

        Assert.Equal(new MustangValidationStatuses("valid", "invalid", "valid"), result);
    }

    [Fact]
    public void WhenPdfSectionMissingThenPdfStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", null, "valid"));

        Assert.Equal("unknown", result.Pdf);
    }

    [Fact]
    public void WhenXmlSectionMissingThenXmlStatusIsUnknown()
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(Report("valid", "valid", null));

        Assert.Equal("unknown", result.Xml);
    }

    [Fact]
    public void WhenOnlyNestedSummaryExistsThenOverallStatusIsUnknown()
    {
        const string report = """<validation><pdf><nested><summary status="valid"/></nested></pdf></validation>""";

        MustangValidationStatuses result = MustangValidationReportParser.Parse(report);

        Assert.Equal("unknown", result.Overall);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WhenReportIsNullOrWhitespaceThenAllStatusesAreUnknown(string? report)
    {
        MustangValidationStatuses result = MustangValidationReportParser.Parse(report);

        Assert.Equal(MustangValidationStatuses.Unknown, result);
    }

    [Fact]
    public void WhenReportIsMalformedThenXmlExceptionIsThrown() => Assert.ThrowsAny<XmlException>(() => MustangValidationReportParser.Parse("<validation>"));
}
