using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OtelConsoleApp;

/// <summary>Shared OpenTelemetry instrumentation primitives for the application.</summary>
internal static class Telemetry
{
    /// <summary>The logical service name emitted on all telemetry signals.</summary>
    internal const string ServiceName = "OtelConsoleApp";

    /// <summary>The service version emitted on all telemetry signals.</summary>
    internal const string ServiceVersion = "1.0.0";

    /// <summary>
    /// <see cref="System.Diagnostics.ActivitySource"/> used to create traces.
    /// Register this source name with <c>AddSource</c> in the tracer provider.
    /// </summary>
    internal static readonly ActivitySource ActivitySource = new(ServiceName, ServiceVersion);

    /// <summary>
    /// <see cref="System.Diagnostics.Metrics.Meter"/> used to create metrics instruments.
    /// Register this meter name with <c>AddMeter</c> in the meter provider.
    /// </summary>
    internal static readonly Meter Meter = new(ServiceName, ServiceVersion);
}
