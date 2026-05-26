using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OtelConsoleApp;

var builder = Host.CreateApplicationBuilder(args);

var newRelicApiKey = builder.Configuration["NewRelic:ApiKey"] ?? string.Empty;
var newRelicEndpoint =
    builder.Configuration["NewRelic:OtlpEndpoint"] ?? "https://otlp.nr-data.net:4317";
var aspireEndpoint = builder.Configuration["Aspire:OtlpEndpoint"] ?? "http://localhost:4317";

void ConfigureAspire(OtlpExporterOptions o)
{
    o.Endpoint = new Uri(aspireEndpoint);
    o.Protocol = OtlpExportProtocol.Grpc;
}

void ConfigureNewRelic(OtlpExporterOptions o)
{
    o.Endpoint = new Uri(newRelicEndpoint);
    o.Protocol = OtlpExportProtocol.Grpc;
    o.Headers = $"api-key={newRelicApiKey}";
}

builder
    .Services.AddOpenTelemetry()
    .ConfigureResource(r =>
        r.AddService(Telemetry.ServiceName, serviceVersion: Telemetry.ServiceVersion)
            .AddAttributes(
                new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                }
            )
    )
    .WithTracing(t =>
        t.AddSource(Telemetry.ServiceName)
            .AddOtlpExporter(ConfigureAspire)
            .AddOtlpExporter(ConfigureNewRelic)
    )
    .WithMetrics(m =>
        m.AddMeter(Telemetry.ServiceName)
            .AddRuntimeInstrumentation()
            .AddOtlpExporter(ConfigureAspire)
            .AddOtlpExporter(ConfigureNewRelic)
    )
    .WithLogging(l => l.AddOtlpExporter(ConfigureAspire).AddOtlpExporter(ConfigureNewRelic));

builder.Services.Configure<OpenTelemetry.Logs.OpenTelemetryLoggerOptions>(o =>
{
    o.IncludeFormattedMessage = true;
    o.IncludeScopes = true;
});

builder.Services.AddHostedService<DemoWorker>();

await builder.Build().RunAsync();
