/*
   Loki Logging Example Script (Direct API Integration with C#)

   This script demonstrates how to directly POST logs to the Loki Push Logs API by configuring a 
   custom log handler (`LokiHandler`) to format and send logs. It includes:
   - A custom log handler (`LokiHandler`) to format and send logs directly to the Loki Push Logs API.
   - Support for multi-tenancy and optional basic authentication.
   - Structured logging with log levels, metadata, and custom fields for enhanced querying in Loki.

   Usage:
   1. Define the Loki server URL, tenant (if applicable), and authentication credentials.
   2. Use the `LokiHandler` methods (LogInfo, LogWarning, LogError, LogDebug) to send logs to Loki.
   3. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
*/

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class PostLoggingExample
{
    public static void RunExample()
    {
        // Define Loki server URL
        var lokiUrl = "http://localhost:3100";
        // Optional: Define Loki's tenant if multi-tenancy is used
        // For our log-monitor-stack setup, we use "tenant1"
        var tenant = "tenant1";
        // Optional: Define Loki's user and password if authentication is required
        // For our log-monitor-stack setup, no authentication is needed
        var user = string.Empty;
        // Important: In Grafana Cloud, an API key is used as the password
        var password = string.Empty;

        // Create an instance of the LokiHandler (our custom implementation class) with appropriate parameters
        var lokiHandler = new LokiHandler(
            lokiUrl, "dev", "csharp", "my-computer",
            tenant, user, password
        );

        // Post some log messages
        lokiHandler.LogInfo("This is an informational message posted from C#");
        lokiHandler.LogWarning("This is a warning message posted from C#");
        lokiHandler.LogError("This is an error message posted from C#");
        lokiHandler.LogDebug("This is a debug message posted from C#");

        Console.WriteLine("Execution complete. Check Loki server for logs.");
    }
}

/// <summary>
///     Custom Loki log handler that formats and sends logs directly to the Loki Push Logs API.
/// </summary>
public class LokiHandler
{
    private static readonly HttpClient HttpClient = new();
    private readonly string _application;
    private readonly string _environment;
    private readonly string _host;
    private readonly string _lokiPassword;
    private readonly string _lokiTenant;
    private readonly string _lokiUser;
    private string _url;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LokiHandler" /> class.
    /// </summary>
    public LokiHandler(string url, string environment, string application, string host,
        string lokiTenant = null, string lokiUser = null, string lokiPassword = null)
    {
        _url = url;
        _environment = environment;
        _application = application;
        _host = host;
        _lokiTenant = lokiTenant;
        _lokiUser = lokiUser;
        _lokiPassword = lokiPassword;
    }

    public void LogDebug(string message)
    {
        WriteAsync("DEBUG", message).GetAwaiter().GetResult();
    }

    public void LogInfo(string message)
    {
        WriteAsync("INFO", message).GetAwaiter().GetResult();
    }

    public void LogWarning(string message)
    {
        WriteAsync("WARN", message).GetAwaiter().GetResult();
    }

    public void LogError(string message)
    {
        WriteAsync("ERROR", message).GetAwaiter().GetResult();
    }

    private async Task WriteAsync(string level, string message)
    {
        const string lokiPushEndpoint = "/loki/api/v1/push";
        // Sanitize the given loki url
        // Remove trailing "/" form the url if exists
        if (_url.EndsWith("/")) _url = _url[..^1];
        // If given url does not end with the loki API push endpoint, append it
        if (!_url.EndsWith(lokiPushEndpoint)) _url += lokiPushEndpoint;

        var logLevel = level.Trim().ToLower();
        // Get the current time in nanoseconds since the epoch
        var currentTimeEpochNs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000 + "";
        // Get the formatted timestamp to use in the log message
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logMessage = $"{timestamp} [{level}] {message}";

        var payload = new
        {
            streams = new[]
            {
                new
                {
                    stream = new
                    {
                        application = _application,
                        environment = _environment,
                        host = _host,
                        level = logLevel
                    },
                    values = new[]
                    {
                        new[] { currentTimeEpochNs, logMessage }
                    }
                }
            }
        };

        // Prints payload in console for debugging
        // Console.WriteLine("Loki Payload: " + JsonSerializer.Serialize(payload));

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        // Prepare headers and payload for Loki
        // If lokiTenant is used, add it to headers (only if not None, not empty, not blank)
        if (!string.IsNullOrWhiteSpace(_lokiTenant)) content.Headers.Add("X-Scope-OrgID", _lokiTenant);

        // If lokiUser and lokiPassword are provided, use them for basic authentication
        if (!string.IsNullOrWhiteSpace(_lokiUser) && !string.IsNullOrWhiteSpace(_lokiPassword))
        {
            var byteArray = Encoding.ASCII.GetBytes($"{_lokiUser}:{_lokiPassword}");
            HttpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        // Send the log to Loki
        HttpResponseMessage response = null;
        try
        {
            response = await HttpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();
            // Console.WriteLine($"Successfully sent log to Loki: {(int)response.StatusCode}");
            // Display log message in the console
            // Enhance the console message to display different colors based on the log level
            var color = level switch
            {
                "INFO" => ConsoleColor.Green,
                "WARN" => ConsoleColor.Yellow,
                "ERROR" => ConsoleColor.Red,
                "DEBUG" => ConsoleColor.Blue,
                _ => ConsoleColor.White
            };

            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(logMessage);
            Console.ForegroundColor = originalColor;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Failed to send log to Loki: {e.Message}");
            var statusCodeText = response is not null
                ? ((int)response.StatusCode).ToString()
                : e.StatusCode.HasValue
                    ? ((int)e.StatusCode).ToString()
                    : "unknown";
            Console.WriteLine($"Response status code: {statusCodeText}");
            if (response != null)
            {
                // Read and print the response content for more details
                var responseText = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response text: {responseText}");
            }
        }
    }
}