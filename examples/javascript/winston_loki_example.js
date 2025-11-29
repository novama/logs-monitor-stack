/*
   Winston Logging Example Script (Integration with Loki)

   This script demonstrates how to configure and use the Winston logging library with the `winston-loki` transport to send logs to the Loki Push Logs API. It includes:
   - Setting up Winston with the Loki transport for structured logging.
   - Support for multi-tenancy and optional basic authentication.
   - Structured logging with log levels, metadata, and custom fields for enhanced querying in Loki.

   Usage:
   1. Install the required libraries using npm:
      npm install winston winston-loki

   2. Run the script to send example logs to the configured Loki server:
      node winston_loki_example.js

   3. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
*/

const LokiTransport = require("winston-loki");

// Function to create and configure a Winston logger with Loki transport
function getWinstonLoggerConfigured(url, environment, application, host, tenant = undefined, user = undefined, password = undefined) {
  const {createLogger, format, transports} = require("winston");

  // Prepare headers and payload for Loki
  const headers = {
    "Content-Type": "application/json",
  };

  // If loki_tenant is used, add it to headers (only if not None, not empty, not blank)
  if (tenant && tenant.trim() !== "") {
    headers["X-Scope-OrgID"] = tenant;
  }

  // Configure Winston logger with Loki transport, file, and console
  return createLogger({
    // Default log format with timestamp and level
    format: format.combine(format.timestamp(), format.printf(({timestamp, level, message}) => {
      return `${timestamp} [${level.toUpperCase()}] ${message}`;
    })), transports: [
      new LokiTransport({
        host: url,
        labels: {
          application: application, environment: environment, host: host,
        },
        headers: headers,
        basicAuth: user && password ? `${user}:${password}` : undefined,
        json: true,
        batching: false,
        format: format.combine(
          // Formats timestamp to local time with milliseconds
          format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss.SSS' }),
          // Use different formats based on environment
          format.printf(({timestamp, level, message}) => {
            return `${timestamp} [${level.toUpperCase()}] ${message}`;
          })
        ),
        onConnectionError: (err) => console.error(err),
      }),
      new transports.Console({
        format: format.combine(
          // Formats timestamp to local time with milliseconds
          format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss.SSS' }),
          format.printf(({timestamp, level, message}) => {
            return `${timestamp} [${level.toUpperCase()}] ${message}`;
          }), // Adds color to the console output (not needed, but it's a nice touch)
          format.colorize({all: true})),
      }),
    ],
    defaultMeta: {
      loggerType: "winston", pid: process.pid,
    },
  });
}

// =========================================
// Script Entry Point
// =========================================
// This script demonstrates how to configure and use the Winston logging library
// with the `winston-loki` transport to send logs to a Loki server using the Push Logs API.
// It sends four example log messages (DEBUG, INFO, WARNING, ERROR) to the configured Loki server.
// Additionally, logs are written to the console and a file (logs/output.log) for validation.
// Verify the logs in the Loki server, console, and output file to ensure successful integration.
// =========================================

// Define Loki server URL
const lokiUrl = "http://localhost:3100";
// Optional: Define Loki's tenant if multi-tenancy is used
// For our log-monitor-stack setup, we use "tenant1"
const tenant = "tenant1";
// Optional: Define Loki's user and password if authentication is required
// For our log-monitor-stack setup, no authentication is needed
const user = undefined;
// Important: In Grafana Cloud, an API key is used as the password
const password = undefined;

// Create and configure the Winston logger
const logger = getWinstonLoggerConfigured(lokiUrl, "dev", "javascript", "my-computer", tenant, user, password);
logger.level = "debug";

// Post some log messages
logger.info("This is an informational message posted from JavaScript using winston-loki");
logger.warn("This is a warning message posted from JavaScript using winston-loki");
logger.error("This is an error message posted from JavaScript using winston-loki");
logger.debug("This is a debug message posted from JavaScript using winston-loki");

console.log("Execution complete. Check Loki server, console, and logs/output.log for logs.");
