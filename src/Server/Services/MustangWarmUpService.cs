namespace Docentric.EInvoice.Validator.RestServer.Services;

/// <summary>
/// Loads the JVM and the Mustang CLI jar into the OS file cache after startup, so the first real request is not slow.
/// The host cancels and awaits this service on shutdown, which lets the Java process cleanup finish before services are disposed.
/// </summary>
/// <param name="mustangCliService">The Mustang CLI service used to run the warm-up command.</param>
/// <param name="logger">The logger instance for logging information and errors.</param>
internal sealed class MustangWarmUpService(MustangCliService mustangCliService, ILogger<MustangWarmUpService> logger) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Best effort only: a warm-up failure must never affect the application.
        try
        {
            bool available = await mustangCliService.CheckAvailabilityAsync(stoppingToken);
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Mustang CLI warm-up finished (available: {Available}).", available);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The application is shutting down.
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Mustang CLI warm-up failed.");
        }
    }
}
