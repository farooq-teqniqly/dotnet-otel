using Microsoft.Extensions.Logging;

namespace OtelConsoleApp;

internal sealed partial class DemoWorker
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Order {OrderId} completed in {ElapsedMs}ms"
    )]
    private static partial void LogOrderCompleted(ILogger logger, int orderId, long elapsedMs);

    [LoggerMessage(Level = LogLevel.Error, Message = "Order {OrderId} failed after {ElapsedMs}ms")]
    private static partial void LogOrderFailed(
        ILogger logger,
        Exception ex,
        int orderId,
        long elapsedMs
    );

    [LoggerMessage(Level = LogLevel.Debug, Message = "Order {OrderId} fulfilled")]
    private static partial void LogOrderFulfilled(ILogger logger, int orderId);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Processing order {OrderId} (type={OrderType})"
    )]
    private static partial void LogProcessingOrder(ILogger logger, int orderId, string orderType);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "DemoWorker starting. Press Ctrl+C to stop"
    )]
    private static partial void LogWorkerStarting(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "DemoWorker stopped")]
    private static partial void LogWorkerStopped(ILogger logger);
}
