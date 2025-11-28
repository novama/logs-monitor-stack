"""
Loki Logging Example Script (Direct API Integration)

This script demonstrates how to directly POST to the Loki Push Logs API by configuring it as a 
logging handler for the Loguru library. It includes:
- A custom log handler (`LokiHandler`) to format and send logs directly to the Loki Push Logs API.
- Support for multi-tenancy and optional basic authentication.
- Structured logging with log levels and metadata for enhanced querying in Loki.

Usage:
1. Install the required libraries using pip:
    pip install -r requirements.txt

2. Run the script to send example logs to the configured Loki server:
    python post_logging_example.py

3. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
"""

import json
import sys
import time

import requests
from loguru import logger


class LokiHandler:
    def __init__(self, url, environment, application, host, loki_tenant=None, loki_user=None, loki_password=None):
        self.url = url
        self.environment = environment
        self.application = application
        self.host = host
        self.loki_tenant = loki_tenant
        self.loki_user = loki_user
        self.loki_password = loki_password

    def write(self, record):
        loki_push_endpoint = "/loki/api/v1/push"
        # Sanitize the given loki url
        if self.url.endswith("/"):
            self.url = self.url.rstrip("/")
        # If given url does not end with the loki API push endpoint, append it
        if not self.url.endswith(loki_push_endpoint):
            self.url = f"{self.url}{loki_push_endpoint}"

        # Prepare headers and payload for Loki
        headers = {
            "Content-Type": "application/json"
        }
        # If loki_tenant is used, add it to headers (only if not None, not empty, not blank)
        if self.loki_tenant is not None and str(self.loki_tenant).strip() != "":
            headers["X-Scope-OrgID"] = self.loki_tenant

        # If loki_user and loki_password are provided, use them for basic authentication
        auth = None
        if self.loki_user is not None and self.loki_password is not None:
            auth = (self.loki_user, self.loki_password)

        # Extract log level and message from Loguru record dict
        log_level = record['level'].name.lower()
        log_message = record.get('message', '')
        # Format the log message to include timestamp and level
        log_message_formatted = f"{record['time'].strftime('%Y-%m-%d %H:%M:%S.%f')[:-3]} [{log_level.upper()}] {log_message}"

        # Get the current time in nanoseconds since the epoch
        current_time_epoch_ns = f"{time.time_ns()}"

        # Construct the payload
        payload = {
            "streams": [
                {
                    "stream": {
                        "application": self.application,
                        "environment": self.environment,
                        "host": self.host,
                        "level": log_level
                    },
                    "values": [
                        [
                            current_time_epoch_ns,
                            log_message_formatted
                        ]
                    ]
                }
            ]
        }

        # Prints payload in console for debugging
        print(payload)

        # Send the log to Loki
        response = None
        try:
            response = requests.post(
                self.url,
                headers=headers,
                data=json.dumps(payload),
                auth=auth  # Pass auth tuple if provided, else None
            )
            response.raise_for_status()  # Raise an HTTPError if the HTTP request returned an unsuccessful status code
            print(f"Successfully sent log to Loki: {response.status_code}")
        except requests.exceptions.RequestException as e:
            print(f"Failed to send log to Loki: {e}")
            if response is not None:
                print(f"Response status code: {response.status_code}")
                print(f"Response text: {response.text}")


# =========================================
# Script Entry Point
# =========================================
# This script demonstrates how to send logs to a Loki server using the Push Logs API.
# It sends four example log messages (DEBUG, INFO, WARNING, ERROR) to the configured Loki server.
# Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
# =========================================

# Define Loki server URL
loki_url = "http://localhost:3100"
# Optional: Define Loki's tenant if multi-tenancy is used
# For our log-monitor-stack setup, we use "tenant1"
tenant = "tenant1"
# Optional: Define Loki's user and password if authentication is required
# For our log-monitor-stack setup, no authentication is needed
user = None
# Important: In Grafana Cloud, an API key is used as the password
password = None

# Create an instance of the LokiHandler (our custom implementation class) with appropriate parameters
loki_handler = LokiHandler(loki_url, "dev", "python", "my-computer", tenant, user, password)

# Configure Loguru to use the custom format for console output
# Remove the default handler
logger.remove()
# Define a custom format for the console output with colorized level icons
custom_format = "<level>{time:YYYY-MM-DD HH:mm:ss.SSS} [{level}] {message}</level>"
# Add a new handler with the custom format and ensure colorization is enabled
logger.add(sys.stdout, format=custom_format, colorize=True)

# Add the LokiHandler to the loguru logger using a lambda to pass the record dict
logger.add(lambda msg: loki_handler.write(msg.record), level="DEBUG")

# Post some log messages
logger.info("This is an informational message posted from Python")
logger.warning("This is a warning message posted from Python")
logger.error("This is an error message posted from Python")
logger.debug("This is a debug message posted from Python")

print("Execution complete. Check Loki server for logs.")
