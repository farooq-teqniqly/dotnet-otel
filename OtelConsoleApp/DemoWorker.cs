using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OtelConsoleApp;

/// <summary>
/// Hosted service that simulates order processing to demonstrate OpenTelemetry
/// logs, traces, and metrics. Processes one order every three seconds until cancelled.
/// Every seventh order throws intentionally to produce error spans.
/// </summary>
internal sealed class DemoWorker : BackgroundService
{
    /// <summary>Tracks the number of orders currently in-flight.</summary>
    private static readonly UpDownCounter<int> ActiveOrders =
        Telemetry.Meter.CreateUpDownCounter<int>(
            "orders.active",
            "orders",
            "Currently active orders"
        );

    /// <summary>Counts completed orders, tagged by <c>status</c> (success | error).</summary>
    private static readonly Counter<long> OrdersProcessed = Telemetry.Meter.CreateCounter<long>(
        "orders.processed",
        "orders",
        "Total orders processed"
    );

    /// <summary>Records end-to-end order processing time in milliseconds, tagged by <c>order.type</c>.</summary>
    private static readonly Histogram<double> ProcessingDuration =
        Telemetry.Meter.CreateHistogram<double>(
            "orders.processing_duration",
            "ms",
            "Order processing duration"
        );
    private readonly ILogger<DemoWorker> _logger;

    /// <summary>Initializes a new instance of <see cref="DemoWorker"/>.</summary>
    /// <param name="logger">Logger for structured log output.</param>
    public DemoWorker(ILogger<DemoWorker> logger) => _logger = logger;

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("DemoWorker starting. Press Ctrl+C to stop");

        var orderId = 0;
        while (!ct.IsCancellationRequested)
        {
            orderId++;
            await ProcessOrderAsync(orderId, ct);
            await Task.Delay(TimeSpan.FromSeconds(3), ct).ConfigureAwait(false);
        }

        _logger.LogInformation("DemoWorker stopped");
    }

    /// <summary>
    /// Fulfills the order inside a child <c>FulfillOrder</c> span.
    /// Simulates downstream latency with a random delay.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task FulfillOrderAsync(int orderId, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("FulfillOrder");
        activity?.SetTag("order.id", orderId);

        await Task.Delay(Random.Shared.Next(50, 150), ct).ConfigureAwait(false);
        _logger.LogDebug("Order {OrderId} fulfilled", orderId);
    }

    /// <summary>
    /// Runs the full order pipeline (validate → fulfill) inside a root <c>ProcessOrder</c> span.
    /// Records outcome metrics and logs regardless of success or failure.
    /// </summary>
    /// <param name="orderId">Monotonically increasing order identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task ProcessOrderAsync(int orderId, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", orderId);
        activity?.SetTag("order.type", orderId % 2 == 0 ? "express" : "standard");

        var sw = Stopwatch.StartNew();
        ActiveOrders.Add(1);

        _logger.LogInformation(
            "Processing order {OrderId} (type={OrderType})",
            orderId,
            orderId % 2 == 0 ? "express" : "standard"
        );

        try
        {
            await ValidateOrderAsync(orderId, ct);
            await FulfillOrderAsync(orderId, ct);

            activity?.SetStatus(ActivityStatusCode.Ok);
            OrdersProcessed.Add(1, new TagList { { "status", "success" } });
            _logger.LogInformation(
                "Order {OrderId} completed in {ElapsedMs}ms",
                orderId,
                sw.ElapsedMilliseconds
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            OrdersProcessed.Add(1, new TagList { { "status", "error" } });
            _logger.LogError(
                ex,
                "Order {OrderId} failed after {ElapsedMs}ms",
                orderId,
                sw.ElapsedMilliseconds
            );
        }
        finally
        {
            sw.Stop();
            ActiveOrders.Add(-1);
            ProcessingDuration.Record(
                sw.Elapsed.TotalMilliseconds,
                new TagList { { "order.type", orderId % 2 == 0 ? "express" : "standard" } }
            );
        }
    }

    /// <summary>
    /// Validates the order inside a child <c>ValidateOrder</c> span.
    /// Throws <see cref="InvalidOperationException"/> for every seventh order to simulate failures.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    private async Task ValidateOrderAsync(int orderId, CancellationToken ct)
    {
        using var activity = Telemetry.ActivitySource.StartActivity("ValidateOrder");
        activity?.SetTag("order.id", orderId);

        await Task.Delay(Random.Shared.Next(10, 50), ct).ConfigureAwait(false);

        if (orderId % 7 == 0)
            throw new InvalidOperationException(
                $"Order {orderId} failed validation: inventory unavailable"
            );
    }
}
