using System.Xml.Linq;

namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Overall and per-section statuses extracted from a Mustang CLI validation report.
/// Each status is the raw <c>status</c> attribute value (e.g. "valid", "invalid") or "unknown" when not reported.
/// </summary>
/// <param name="Overall">Status of the top-level summary.</param>
/// <param name="Pdf">Status of the PDF section summary.</param>
/// <param name="Xml">Status of the embedded XML section summary.</param>
public sealed record MustangValidationStatuses(string Overall, string Pdf, string Xml)
{
    /// <summary>Statuses used when the report is missing or cannot be interpreted.</summary>
    public static MustangValidationStatuses Unknown { get; } = new(UnknownStatus, UnknownStatus, UnknownStatus);

    internal const string UnknownStatus = "unknown";
}

/// <summary>
/// Parses the XML validation report produced by the Mustang CLI.
/// </summary>
public static class MustangValidationReportParser
{
    /// <summary>
    /// Extracts the overall, PDF and XML statuses from the report.
    /// </summary>
    /// <param name="reportXml">The XML report written by the Mustang CLI.</param>
    /// <returns>The parsed statuses; sections missing from the report are "unknown".</returns>
    /// <exception cref="System.Xml.XmlException">Thrown when <paramref name="reportXml"/> is not well-formed XML.</exception>
    public static MustangValidationStatuses Parse(string? reportXml)
    {
        if (string.IsNullOrWhiteSpace(reportXml))
            return MustangValidationStatuses.Unknown;

        XElement? root = XDocument.Parse(reportXml).Root;

        // Use direct child Elements() instead of Descendants() to target the top-level <summary status="..."/>
        // which represents the overall validation result aggregating both <pdf> and <xml> sub-results.
        // See: https://www.mustangproject.org/commandline/#validate
        // Under ZuGFeRD rule BR-FX-DE-03, PDF/A failures are warnings (not fatal) for German invoices,
        // so the overall status may be "valid" even when the PDF section reports "invalid".
        return new MustangValidationStatuses(
            GetSummaryStatus(root),
            GetSummaryStatus(root?.Element("pdf")),
            GetSummaryStatus(root?.Element("xml")));
    }

    private static string GetSummaryStatus(XElement? parent) =>
        parent?.Elements("summary").FirstOrDefault()?.Attribute("status")?.Value ?? MustangValidationStatuses.UnknownStatus;
}
