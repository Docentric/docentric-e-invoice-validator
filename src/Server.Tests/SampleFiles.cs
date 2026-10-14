namespace Docentric.EInvoice.Validator.RestServer.Tests;

internal static class SampleFiles
{
    /// <summary>
    /// Path to a valid Factur-X XML invoice copied next to the test binaries.
    /// </summary>
    public static string FacturXXmlPath { get; } = GetPath("factur-x e-invoice.xml");

    /// <summary>
    /// Path to a valid ZUGFeRD PDF invoice copied next to the test binaries.
    /// </summary>
    public static string ZugferdPdfPath { get; } = GetPath("ZuGFeRD e-Invoice.pdf");

    private static string GetPath(string fileName) => Path.Combine(AppContext.BaseDirectory, "Samples", fileName);
}
