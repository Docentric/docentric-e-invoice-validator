using System.Text.Json.Serialization;

namespace Docentric.EInvoice.Validator.RestServer.Contracts;

/// <summary>
/// Specifies the supported types of cryptographic checksums used for data integrity verification.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CheckSumType
{
    /// <summary>
    /// SHA-256 hash algorithm producing a 256-bit (32-byte) hash value.
    /// </summary>
    [JsonPropertyName("sha256")]
    SHA256 = 1,

    /// <summary>
    /// SHA-384 hash algorithm producing a 384-bit (48-byte) hash value.
    /// </summary>
    [JsonPropertyName("sha384")]
    SHA384 = 2,

    /// <summary>
    /// SHA-512 hash algorithm producing a 512-bit (64-byte) hash value.
    /// </summary>
    [JsonPropertyName("sha512")]
    SHA512 = 3
}
