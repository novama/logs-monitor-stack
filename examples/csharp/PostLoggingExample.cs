using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public static class PostLoggingExample
{
    public static void RunExample()
    {
        var lokiHandler = new LokiHandler(
            "http://localhost:3100/loki/api/v1/push", "dev", "csharp", "my-host"
            );

        lokiHandler.LogInfo("This is an informational message posted from C#");
        lokiHandler.LogWarning("This is a warning message posted from C#");
        lokiHandler.LogError("This is an error message posted from C#");
    }
}


class LokiHandler
{
    private readonly string _url;
    private readonly string _environment;
    private readonly string _application;
    private readonly string _host;
    private readonly string _tenant;
    private static readonly HttpClient _httpClient = new HttpClient();

    public LokiHandler(string url, string environment, string application, string host)
    {
        _url = url;
        _environment = environment;
        _application = application;
        _host = host;
        _tenant = "tenant1";
    }

    public void LogInfo(string message)
    {
        WriteAsync($"INFO | {message}").GetAwaiter().GetResult();
    }

    public void LogWarning(string message)
    {
        WriteAsync($"WARN | {message}").GetAwaiter().GetResult();
    }

    public void LogError(string message)
    {
        WriteAsync($"ERROR | {message}").GetAwaiter().GetResult();
    }

    private async Task WriteAsync(string message)
    {
        // Get the current time in nanoseconds since the epoch
        string currentTimeEpochNs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000000 + ""; 

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
                        host = _host
                    },
                    values = new[]
                    {
                        new[] { currentTimeEpochNs, message }
                    }
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        content.Headers.Add("X-Scope-OrgID", _tenant);

        try
        {
            var response = await _httpClient.PostAsync(_url, content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Successfully sent log to Loki: {(int)response.StatusCode}");
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Failed to send log to Loki: {e.Message}");
            Console.WriteLine($"Response status code: {(int)e.StatusCode}");
        }
    }
}