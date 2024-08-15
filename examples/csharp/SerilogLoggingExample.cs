using Serilog;
using Serilog.Sinks.Grafana.Loki;

public class SerilogLoggingExample
{
    public static void RunExample()
    {

        // Configuration for Loki
        var lokiUrl = "http://localhost:3100";
        var lokiCredentials = new LokiCredentials
        {
            Login = "",
            Password = ""
        };
        var lokiLabels = new[] {
            new LokiLabel() { Key = "application", Value = "chsarp - serilog" },
            new LokiLabel() { Key = "environment", Value = "dev" },
            new LokiLabel() { Key = "host", Value = "my-host" }
        };

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                lokiUrl,
                lokiLabels,
                //credentials: lokiCredentials,
                textFormatter: new Serilog.Formatting.Json.JsonFormatter(),
                tenant: "tenant1"
                )
            .CreateLogger();

        // Log some test messages
        Log.Information("This is an informational message posted from C# using Serilog Sinks");
        Log.Warning("This is a warning message posted from C# using Serilog Sinks");
        Log.Error("This is an error message posted from C# using Serilog Sinks");

        // Flush and close the logger
        Log.CloseAndFlush();
    }
}
