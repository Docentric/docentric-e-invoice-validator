using System.Text.Json.Serialization;

namespace Docentric.ZuGFeRD.Validator.RestServer.Contracts;

/// <summary>
/// Server health status information.
/// </summary>
public class ServerHealthResponse
{
    /// <summary>
    /// Overall server health status. True when all required dependencies are available.
    /// </summary>
    [JsonPropertyName("healthy")]
    public bool Healthy => JavaPresent && MustangCliPresent;

    /// <summary>
    /// Indicates whether Java runtime is available.
    /// </summary>
    [JsonPropertyName("javaPresent")]
    public bool JavaPresent { get; set; }

    /// <summary>
    /// Java runtime version.
    /// </summary>
    [JsonPropertyName("javaVersion")]
    public string? JavaVersion { get; set; }

    /// <summary>
    /// Indicates whether Mustang CLI tool is available.
    /// </summary>
    [JsonPropertyName("mustangCliPresent")]
    public bool MustangCliPresent { get; set; }

    /// <summary>
    /// Mustang CLI tool version.
    /// </summary>
    [JsonPropertyName("mustangCliVersion")]
    public string? MustangCliVersion { get; set; }
}
