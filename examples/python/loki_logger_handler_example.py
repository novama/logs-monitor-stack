"""
Loki Logger Handler Example Script (Using recommended python package)

This script demonstrates how to configure and use the `LokiLoggerHandler` package from PyPI 
to send logs to the Loki Push Logs API. It includes:
- Setting up the `LokiLoggerHandler` with appropriate parameters.
- Attaching the handler to a Python logger.
- Sending structured logs with metadata and custom fields for enhanced querying in Loki.

Usage:
1. Install the required libraries using pip:
    pip install -r requirements.txt

2. Run the script to send example logs to the configured Loki server:
    python loki_logger_handler_example.py

3. Verify the logs in the Loki server or Grafana dashboard to ensure successful integration.
"""

import logging.handlers

# Import the LokiLoggerHandler from the loki_logger_handler package
# For more information, visit: https://github.com/xente/loki-logger-handler
from loki_logger_handler.loki_logger_handler import LokiLoggerHandler

# Set up logging
logger = logging.getLogger("loki_logger_handler_example")

# Function to create and configure the LokiLoggerHandler
def get_loki_logger_handler_configured(url, environment, application, host, loki_tenant=None, loki_user=None,
                                       loki_password=None):
    loki_push_endpoint = "/loki/api/v1/push"
    # Sanitize the given loki url
    if url.endswith("/"):
        url = url.rstrip("/")
    # If given url does not end with the loki API push endpoint, append it
    if not url.endswith(loki_push_endpoint):
        url = f"{url}{loki_push_endpoint}"

    # If loki_tenant is used, add it to headers (only if not None, not empty, not blank)
    headers = {}
    if loki_tenant:
        headers["X-Scope-OrgID"] = loki_tenant

    # If loki_user and loki_password are provided, use them for basic authentication
    auth = None
    if loki_user and loki_password:
        auth = (loki_user, loki_password)

    # Create an instance of the LokiLoggerHandler handler
    configured_handler = LokiLoggerHandler(
        url=url,
        labels={"application": application, "environment": environment, "host": host},
        label_keys={},
        timeout=10,
        additional_headers=headers,
        auth=auth
    )
    return configured_handler


# =========================================
# Script Entry Point
# =========================================
# This script demonstrates how to configure and use the LokiLoggerHandler package
# to send logs to a Loki server using the Push Logs API.
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

# Create an instance of the LokiLoggerHandler (pypi package) with appropriate parameters
loki_logger_handler = get_loki_logger_handler_configured(loki_url, "dev", "python", "my-computer", tenant, user,
                                                         password)
logger.setLevel(logging.DEBUG)
# Add the configured LokiLoggerHandler to the logger
logger.addHandler(loki_logger_handler)

# Add a console handler to the logger
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.DEBUG)
console_formatter = logging.Formatter('%(asctime)s.%(msecs)03d [%(levelname)s] %(message)s', datefmt='%Y-%m-%d %H:%M:%S')
console_handler.setFormatter(console_formatter)
logger.addHandler(console_handler)

# Post some log messages
logger.info("This is an informational message posted from Python using LokiLoggerHandler",
            extra={'custom_field': 'custom_value'})
logger.warning("This is a warning message posted from Python using LokiLoggerHandler",
               extra={'custom_field': 'custom_value'})
logger.error("This is an error message posted from Python using LokiLoggerHandler",
             extra={'custom_field': 'custom_value'})
logger.debug("This is a debug message posted from Python using LokiLoggerHandler",
             extra={'custom_field': 'custom_value'})

print("Execution complete. Check Loki server for logs.")