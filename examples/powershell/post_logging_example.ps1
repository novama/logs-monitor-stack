# Requires -Version 5.1
<#
   Loki Logging Example Script (Direct API Integration)

   This script demonstrates how to directly POST to the Loki Push Logs API by configuring it as a 
   logging handler. It includes:
   - A custom log handler to format and send logs directly to the Loki Push Logs API.
   - Support for multi-tenancy and optional basic authentication.
   - Structured logging with log levels and metadata for enhanced querying in Loki.

   Usage:
   1. Run the script to send example logs to the configured Loki server:
      .\post_logging_example.ps1

   2. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
#>

# Define Loki server URL
$lokiUrl = "http://localhost:3100"
# Optional: Define Loki's tenant if multi-tenancy is used
# For our log-monitor-stack setup, we use "tenant1"
$tenant = "tenant1"
# Optional: Define Loki's user and password if authentication is required
# For our log-monitor-stack setup, no authentication is needed
$user = $null
$password = $null

# Define environment, application, and host metadata
$environment = "dev"
$application = "powershell"
$hostname = "my-computer"

# Function to send logs to Loki
function Send-LogToLoki {
    param (
        [string]$Level,
        [string]$Message
    )

    $lokiPushEndpoint = "/loki/api/v1/push"
    # Sanitize the given Loki URL
    if ($lokiUrl.EndsWith("/")) {
        $lokiUrl = $lokiUrl.TrimEnd("/")
    }
    if (-not $lokiUrl.EndsWith($lokiPushEndpoint)) {
        $lokiUrl = "$lokiUrl$lokiPushEndpoint"
    }

    # Prepare headers and payload for Loki
    $headers = @{ "Content-Type" = "application/json" }
    if (-not [string]::IsNullOrWhiteSpace($tenant)) {
        $headers["X-Scope-OrgID"] = $tenant
    }

    $auth = $null
    if (-not [string]::IsNullOrWhiteSpace($user) -and -not [string]::IsNullOrWhiteSpace($password)) {
        $auth = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($user):$($password)"))
        $headers["Authorization"] = $auth
    }

    # Get the current time in nanoseconds since the epoch as a plain integer string
    $currentTimeEpochNs = [math]::Floor((Get-Date).ToUniversalTime().Subtract([datetime]"1970-01-01").TotalSeconds * 1e9).ToString("F0")

    # Extract milliseconds from the nanoseconds value
    $milliseconds = ($currentTimeEpochNs / 1e6) % 1000

    # Format the log message using the same origin time value
    $logTime = (Get-Date).ToUniversalTime().AddMilliseconds($milliseconds)
    $logMessageFormatted = "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}" -f $logTime, $Level.ToUpper(), $Message

    # Manually construct the JSON payload
    $payload = @"
{
    "streams": [
        {
            "stream": {
                "application": "$application",
                "environment": "$environment",
                "host": "$hostname",
                "level": "$($Level.ToLower())"
            },
            "values": [
                ["$currentTimeEpochNs", "$logMessageFormatted"]
            ]
        }
    ]
}
"@

    # Send the log to Loki
    try {
        $response = Invoke-RestMethod -Uri $lokiUrl -Method Post -Headers $headers -Body $payload
        # Write-Host "Successfully sent log to Loki." -ForegroundColor Green
        # Display log message in the console
        # Enhance the console message to display different colors based on the log level
        switch ($Level.ToLower()) {
            "info" { Write-Host $logMessageFormatted -ForegroundColor Green }
            "warning" { Write-Host $logMessageFormatted -ForegroundColor Yellow }
            "error" { Write-Host $logMessageFormatted -ForegroundColor Red }
            "debug" { Write-Host $logMessageFormatted -ForegroundColor Cyan }
            default { Write-Host $logMessageFormatted }
        }

    } catch {
        Write-Host "Failed to send log to Loki: $_" -ForegroundColor Red
        Write-Output "Payload: $payload" # Debugging output
    }
}

# Post some log messages
Send-LogToLoki -Level "info" -Message "This is an informational message posted from PowerShell"
Send-LogToLoki -Level "warning" -Message "This is a warning message posted from PowerShell"
Send-LogToLoki -Level "error" -Message "This is an error message posted from PowerShell"
Send-LogToLoki -Level "debug" -Message "This is a debug message posted from PowerShell"

Write-Host "Execution complete. Check Loki server for logs."