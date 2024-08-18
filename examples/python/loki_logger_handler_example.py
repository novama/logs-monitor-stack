from loki_logger_handler.loki_logger_handler import LokiLoggerHandler
from loki_logger_handler.formatters.logger_formatter import LoggerFormatter
import logging
import logging.config
import logging.handlers
import os
import sys

# Set up logging
logger = logging.getLogger("custom_logger")


def set_log_handler_config_manually():
    # Create an instance of the custom handler
    custom_handler = LokiLoggerHandler(
        url="http://localhost:3100/loki/api/v1/push",
        labels={"application": "python", "environment": "dev", "host": "my-host"},
        labelKeys={},
        timeout=10,
        additional_headers={"X-Scope-OrgID": "tenant1"}
    )

    logger.setLevel(logging.INFO)
    logger.addHandler(custom_handler)


set_log_handler_config_manually()
logger.info("This is an informational message posted from Python using LokiLoggerHandler", extra={'custom_field': 'custom_value'})
logger.warning("This is a warning message posted from Python using LokiLoggerHandler", extra={'custom_field': 'custom_value'})
logger.error("This is an error message posted from Python using LokiLoggerHandler", extra={'custom_field': 'custom_value'})