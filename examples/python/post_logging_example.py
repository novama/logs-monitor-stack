from loguru import logger
import requests
import json
import time

class LokiHandler:
    def __init__(self, url, environment, application, host):
        self.url = url
        self.environment = environment
        self.application = application
        self.host = host
        self.tenant = "tenant1"

    def write(self, message):
        # Get the current time in nanoseconds since the epoch
        current_time_epoch_ns = f"{time.time_ns()}"
        headers = {
            "Content-Type": "application/json",
            "X-Scope-OrgID": self.tenant
        }
        payload = {
            "streams": [
                {
                    "stream": {
                        "application": self.application,
                        "environment": self.environment,
                        "host": self.host
                    },
                    "values": [
                        [
                            current_time_epoch_ns,
                            message
                        ]
                    ]
                }
            ]
        }
        print(payload)
        try:
            response = requests.post(self.url, headers=headers, data=json.dumps(payload))
            response.raise_for_status()  # Raise an HTTPError if the HTTP request returned an unsuccessful status code
            print(f"Successfully sent log to Loki: {response.status_code}")
        except requests.exceptions.RequestException as e:
            print(f"Failed to send log to Loki: {e}")
            if response is not None:
                print(f"Response status code: {response.status_code}")
                print(f"Response text: {response.text}")

loki_handler = LokiHandler(
    "http://localhost:3100/loki/api/v1/push", "dev", "python", "my-host"
    )
logger.add(loki_handler.write, level="INFO")

logger.info("This is an informational message posted from Python")
logger.warning("This is a warning message posted from Python")
logger.error("This is an error message posted from Python")
