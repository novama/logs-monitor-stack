/*
   Serilog Logging Example Script (Using recommended Serilog Sink for C#)

   This script demonstrates how to configure and use the Serilog logging library with the Grafana Loki sink to send logs to the Loki Push Logs API. It includes:
   - Setting up Serilog with the Loki sink for structured logging.
   - Support for multi-tenancy and optional basic authentication.
   - Structured logging with log levels, metadata, and custom fields for enhanced querying in Loki.

   Usage:
   1. Define the Loki server URL, tenant (if applicable), and authentication credentials.
   2. Configure the Serilog logger using the `GetSerilogConfiguration` method to send logs to Loki.
   3. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
*/

using System;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Grafana.Loki;

public class SerilogLoggingExample
{
    public static void RunExample()
    {
        // Define Loki server URL
        var lokiUrl = "http://localhost:3100";
        // Optional: Define lokiTenant if multi-tenancy is used
        // For our log-monitor-stack setup, we use "tenant1"
        var tenant = "tenant1";
        // Optional: Define lokiUser and lokiPassword if authentication is required
        // For our log-monitor-stack setup, no authentication is needed
        var user = string.Empty;
        // Important: In Grafana Cloud, an API key is used as the password
        var password = string.Empty;

        // Create a configured instance of Serilog logger with Loki sink
        var logger = GetSerilogConfiguration(lokiUrl, "dev", "csharp", "my-computer",
            tenant, user, password).CreateLogger();
        // Configure the global Serilog logger
        Log.Logger = logger;

        // Post some log messages
        Log.Information("This is an informational message posted from C# using Serilog Sinks");
        Log.Warning("This is a warning message posted from C# using Serilog Sinks");
        Log.Error("This is an error message posted from C# using Serilog Sinks");
        Log.Debug("This is a debug message posted from C# using Serilog Sinks");

        // Flush and close the logger
        Log.CloseAndFlush();

        Console.WriteLine("Execution complete. Check Loki server for logs.");
    }

    public static LoggerConfiguration GetSerilogConfiguration(string url, string environment, string application,
        string host, string lokiTenant = null, string lokiUser = null, string lokiPassword = null)
    {
        const string lokiPushEndpoint = "/loki/api/v1/push";
        // Sanitize the given loki url
        // Remove any occurrence of the push endpoint (case-insensitive) and then remove any trailing "/"
        // The Serilog sink will append the push endpoint automatically, so we need to ensure it's not duplicated
        if (!string.IsNullOrEmpty(url))
        {
            // Use Replace overload with StringComparison to remove every occurrence case-insensitively
            url = url.Replace(lokiPushEndpoint, string.Empty, StringComparison.OrdinalIgnoreCase);

            // Remove any trailing slashes that may remain after the replacements
            url = url.TrimEnd('/');
        }

        LokiCredentials lokiCredentials = null;
        // If lokiUser and lokiPassword are provided, use them for basic authentication
        if (!string.IsNullOrWhiteSpace(lokiUser) && !string.IsNullOrWhiteSpace(lokiPassword))
            lokiCredentials = new LokiCredentials
            {
                Login = lokiUser,
                Password = lokiPassword
            };

        // Define loki labels
        var lokiLabels = new[]
        {
            new LokiLabel { Key = "environment", Value = environment },
            new LokiLabel { Key = "application", Value = application },
            new LokiLabel { Key = "host", Value = host }
        };

        // Configure Serilog
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.GrafanaLoki(
                url,
                lokiLabels,
                credentials: lokiCredentials,
                textFormatter: new JsonFormatter(),
                tenant: lokiTenant
            );
        return loggerConfiguration;
    }
}