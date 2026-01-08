namespace Docentric.EInvoice.Validator.RestServer.Configuration;

/// <summary>
/// Contains application-wide constant values.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Application-level constants.
    /// </summary>
    public static class Application
    {
        /// <summary>
        /// The default title for the application.
        /// </summary>
        public const string DefaultTitle = "Docentric e-Invoice Document Validator";
    }

    /// <summary>
    /// OpenAPI specification constants.
    /// </summary>
    public static class OpenApi
    {
        /// <summary>
        /// The API version identifier.
        /// </summary>
        public const string Version = "v1";
        
        /// <summary>
        /// The URI suffix for the OpenAPI specification document.
        /// </summary>
        public const string V1UriSufix = $"/openapi/{Version}.json";
    }

    /// <summary>
    /// Request timeout configuration values.
    /// </summary>
    public static class RequestTimeouts
    {
        /// <summary>
        /// Default timeout in seconds.
        /// </summary>
        public const int DefaultInSeconds = 30;
              
        /// <summary>
        /// Timeout for long-running operations in seconds.
        /// </summary>
        public const int LongRunningInSeconds = 300;
        
        /// <summary>
        /// Policy name for long-running operations.
        /// </summary>
        public const string LongRunningPolicy = "LongRunning";
    }
}
