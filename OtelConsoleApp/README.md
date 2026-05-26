# OtelConsoleApp

.NET console app demonstrating OpenTelemetry — logs, traces, and metrics exported to both the **Aspire standalone dashboard** and **New Relic** via OTLP/gRPC.

## What it does

Runs a `BackgroundService` (`DemoWorker`) that simulates order processing every 3 seconds:

- **Traces** — nested spans: `ProcessOrder` → `ValidateOrder` + `FulfillOrder`. Every 7th order throws to demo error spans and exception recording.
- **Metrics** — `orders.processed` (counter), `orders.processing_duration` (histogram), `orders.active` (up-down counter), plus .NET runtime metrics.
- **Logs** — structured `ILogger` messages correlated to active traces.

All three signals are exported to both destinations simultaneously.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Aspire CLI](https://aspire.dev/dashboard/standalone/) for the standalone dashboard
- A New Relic account with an ingest license key

## Configuration

### API key (user secrets — recommended)

The New Relic API key is read from [.NET user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) so it never lands in source control.

```bash
dotnet user-secrets set "NewRelic:ApiKey" "<your-ingest-license-key>" --project OtelConsoleApp
```

User secrets are loaded automatically when `DOTNET_ENVIRONMENT` is `Development`. The included `Properties/launchSettings.json` sets this for `dotnet run` and IDE launches. If running via `dotnet exec` directly, set the variable manually.

### Endpoints and other settings

Edit `appsettings.json` for endpoint overrides:

```json
{
  "NewRelic": {
    "ApiKey": "",
    "OtlpEndpoint": "https://otlp.nr-data.net:4317"
  },
  "Aspire": {
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

Or use environment variables (double-underscore as separator):

```
NewRelic__ApiKey=<key>
NewRelic__OtlpEndpoint=https://otlp.nr-data.net:4317
Aspire__OtlpEndpoint=http://localhost:4317
```

## Running the Aspire standalone dashboard

See [Aspire standalone dashboard docs](https://aspire.dev/dashboard/standalone/) for installation. Once the CLI is installed:

```bash
aspire run dashboard
```

| Endpoint | Default | Purpose |
|----------|---------|---------|
| Dashboard UI | http://localhost:18888 | View traces, metrics, logs |
| OTLP gRPC | `http://localhost:4317` | Send telemetry from this app |
| OTLP HTTP | `http://localhost:4318` | Alternative HTTP/protobuf transport |

The `Aspire:OtlpEndpoint` in `appsettings.json` must point to the gRPC endpoint (`http://localhost:4317`).

## Running the app

```bash
dotnet run --project OtelConsoleApp
```

Press `Ctrl+C` to stop.

> **Note:** Metrics may take up to a minute to appear in the Aspire dashboard. Traces and logs show up immediately.

## Project structure

| File | Purpose |
|------|---------|
| `Program.cs` | Host setup; dual OTLP export wired for all three signals |
| `Telemetry.cs` | Shared `ActivitySource` and `Meter` instances |
| `DemoWorker.cs` | Hosted service generating demo telemetry |
| `appsettings.json` | Endpoint and API key configuration |
| `Properties/launchSettings.json` | Sets `DOTNET_ENVIRONMENT=Development` for `dotnet run` / IDE |

## Packages

| Package | Version |
|---------|---------|
| `Microsoft.Extensions.Hosting` | 10.0.8 |
| `OpenTelemetry.Extensions.Hosting` | 1.15.3 |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.15.3 |
| `OpenTelemetry.Instrumentation.Runtime` | 1.15.1 |

## New Relic OTLP endpoints

| Region | Endpoint |
|--------|---------|
| US | `https://otlp.nr-data.net:4317` (default) |
| EU | `https://otlp.eu01.nr-data.net:4317` |

Set `NewRelic:OtlpEndpoint` in `appsettings.json` or via the `NewRelic__OtlpEndpoint` environment variable.
