namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Represents the result of a Java runtime information query.
/// </summary>
/// <param name="IsAvailable">Indicates whether Java is available on the system.</param>
/// <param name="Properties">A dictionary containing Java system properties.</param>
public sealed record JavaInfoResult(bool IsAvailable, IDictionary<string, string> Properties)
{
    /// <summary>
    /// Gets the Java runtime version from the system properties.
    /// </summary>
    /// <value>
    /// The runtime version from "java.runtime.version" property if available,
    /// otherwise falls back to "java.version" property, or null if neither exists.
    /// </value>
    public string? RuntimeVersion
        => Properties.TryGetValue("java.runtime.version", out string? runtime)
            ? runtime
            : Properties.TryGetValue("java.version", out string? version)
                ? version
                : null;
}
