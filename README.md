# Logs Monitor Stack

[![Docker](https://img.shields.io/badge/Docker-Ready-blue.svg)](https://www.docker.com/)
[![C++](https://img.shields.io/badge/C++-17%2B-green.svg)](https://isocpp.org/)
[![C#](https://img.shields.io/badge/C%23-.NET%209.0+-purple.svg)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.10+-blue.svg)](https://www.python.org/)
[![JavaScript](https://img.shields.io/badge/JavaScript-ES6+-yellow.svg)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
[![PowerShell](https://img.shields.io/badge/PowerShell-5.1+-blue.svg)](https://learn.microsoft.com/en-us/powershell/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Logs Monitor Stack is a versatile and efficient solution for monitoring and logging. It integrates Prometheus, Grafana, and Loki to collect, visualize, and manage metrics and logs from diverse applications, including Python scripts, C# programs, JavaScript, PowerShell scripts, etc. By leveraging Docker containers, the stack ensures seamless deployment and simplified management, making it ideal for modern development and operational workflows.

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
- [Configuration](#configuration)
- [Usage](#usage)
  - [Start the Docker containers](#start-the-docker-containers)
  - [Validate Loki Startup](#validate-loki-startup)
  - [Accessing and Configuring Loki in Grafana](#accessing-and-configuring-loki-in-grafana)
- [Logging Examples](#logging-examples)
  - [Python Logging](#python-logging)
  - [C# Logging](#c-logging)
  - [C++ Logging](#c-logging-1)
  - [JavaScript Logging](#javascript-logging)
  - [PowerShell Logging](#powershell-logging)
- [Troubleshooting](#troubleshooting)
  - [Loki Not Receiving Logs](#loki-not-receiving-logs)
  - [Grafana Dashboard Not Displaying Data](#grafana-dashboard-not-displaying-data)
  - [Service Health Checks](#service-health-checks)
- [Security Considerations](#security-considerations)
- [Scaling the Stack](#scaling-the-stack)
  - [Horizontal Scaling](#horizontal-scaling)
  - [Prometheus Scaling](#prometheus-scaling)
- [Backup and Restore](#backup-and-restore)
  - [Loki Data](#loki-data)
  - [Prometheus Data](#prometheus-data)
  - [Grafana Dashboards](#grafana-dashboards)
- [Updating the Stack](#updating-the-stack)
  - [Updating Docker Images](#updating-docker-images)
  - [Configuration Changes](#configuration-changes)
- [Resource Requirements](#resource-requirements)
  - [CPU \& Memory Usage](#cpu--memory-usage)
  - [Storage Requirements](#storage-requirements)
- [Metrics and Logs Visualization](#metrics-and-logs-visualization)
  - [Example Dashboards](#example-dashboards)
  - [Log Queries](#log-queries)
- [License](#license)
- [Deployment to Production](#deployment-to-production)
  - [Best Practices](#best-practices)
  - [CI/CD Integration](#cicd-integration)

## Features

- **Prometheus**: Monitoring and alerting toolkit for collecting and querying metrics.
- **Grafana**: Visualization and analytics platform.
- **Loki**: Log aggregation system designed to work with Grafana.
- **Promtail**: Agent for shipping logs to Loki.
- **Alloy**: Scraper for Docker metrics.
- **Minio**: High-performance object storage service.
- **Nginx**: Acts as a reverse proxy to route requests to the correct Loki service.
- **Flog**: Generates fake logs for testing.
- **Docker**: Containerization for easy deployment.

## Architecture Overview

The Logs Monitor Stack is composed of several interconnected components, each fulfilling a specific role:

- **Loki**: Serves as the centralized logging system. It is split into read, write, and backend components to handle log ingestion, querying, and storage.
- **Prometheus**: Collects and stores metrics, which can be visualized in Grafana.
- **Grafana**: Acts as the front-end for visualizing both metrics (from Prometheus) and logs (from Loki).
- **Promtail**: Collects logs from the local system and forwards them to Loki.
- **Alloy**: Scrapes Docker metrics and forwards them to Loki.
- **Minio**: Provides S3-compatible storage for Loki’s log data.
- **Nginx**: Routes HTTP requests to the appropriate Loki components, and publish Grafana on HTTP (80) or HTTPS (443).
- **Flog**: Generates test logs to simulate real-world logging scenarios.

## Prerequisites

- Ensure Docker and Docker Compose are installed on your machine.

## Setup

1. Clone the repository:

   ```sh
   git clone https://github.com/Orthotech1/log-monitor-stack.git
   cd log-monitor-stack/docker
   ```

2. Create the necessary directories for Loki and Minio data storage:

   ```sh
   mkdir -p ../.data/minio
   sudo mkdir -p /path/to/your/loki/data
   sudo chown -R 1000:1000 /path/to/your/loki/data
   ```

## Configuration

- **Alloy** configuration: [alloy/alloy-local-config.yml](./docker/config/alloy/alloy-local-config.yml)
- **Grafana** data sources configuration: [grafana/grafana-datasources.yml](./docker/config/grafana/grafana-datasources.yml)
- **Loki** configuration: [loki/loki-config.yml](./docker/config/loki/loki-config.yml)
- **nginx** configuration: [nginx/nginx-config.conf](./docker/config/nginx/nginx-config.conf)
- **Prometheus** configuration: [prometheus/prometheus-config.yml](./docker/config/prometheus/prometheus-config.yml)
- **Promtail** configuration: [promtail/promtail-config.yml](./docker/config/promtail/promtail-config.yml)

## Usage

### Start the Docker containers

   ```sh
   cd docker
   docker-compose up -d
   ```

### Validate Loki Startup

   Check the logs to ensure Loki starts correctly:

   ```sh
   docker-compose logs loki-backend
   ```

### Accessing and Configuring Loki in Grafana

1. **Access Grafana**:
   - Open your browser and go to `http://localhost:3000`.
   - Log in with the default credentials (`admin` / `admin`).

   ![Grafana Login](../misc/grafana-login.png)

   **Notes:**
   ***For testing purposes only***: You can modify your `docker-compose.yml` file and add the following environment variables for the `grafana` service and you will disable the login screen, activating anonymous access with administrator permissions:

   ```yml
   environment:
      - GF_AUTH_ANONYMOUS_ENABLED=true
      - GF_AUTH_ANONYMOUS_ORG_ROLE=Admin
   ```

   **Grafana Configuration:**
   Grafana stores its configuration in a local `grafana.ini` file. In Linux environments, your configuration file is located at `/etc/grafana/grafana.ini`

   You can override configuration settings with environment variables.
   To override an option:

   ```yml
   environment:
      - GF_<SectionName>_<KeyName>=<Value>
   ```

   For more information about this, please visit the [official documentation](https://grafana.com/docs/grafana/latest/setup-grafana/configure-grafana/#override-configuration-with-environment-variables)

2. **Add Loki as a Data Source**:
   This step is not necessary if you have used the `docker-compose.yml`, as the Loki datasource has been automatically added by configuration.

   If you need to manually connect Grafana to Loki:
   - In Grafana, go to **Configuration** (the gear icon) > **Data Sources**.
   - Click **Add data source**.
   - Select **Loki** from the list of available data sources.
   - Configure the data source with the following settings:
     - **URL**: `http://localhost:3100`
     - Leave other settings at their defaults.
   - Click **Save & Test** to verify the connection.

3. **Create a Dashboard and Explore Logs**:
   - Go to **Create** (the plus icon) > **New dashboard**.
   - Click **Add visualization**.
   - Select **Loki** as the data source.
   - Enter a log query to visualize logs. For example:

     ```logql
     {system_logs="varlogs"}
     ```

   - Click **Save** to save the panel to the dashboard.

## Logging Examples

### Python Logging

For examples of logging in Python, see the [Python logging examples](./examples/python).

There are two examples:

1. **Loguru Integration**: Demonstrates how to use the Loguru library to send logs to Loki.
2. **Direct API Integration**: Shows how to directly POST logs to the Loki Push Logs API.

Make sure you install the dependencies for the examples defined in the `requirements.txt` file.

```shell
pip install -r ./requirements.txt
python post_logging_example.py
```

### C# Logging

For examples of logging in C#, see the [C# logging examples](./examples/csharp).

There are two examples:

1. **Serilog Integration**: Demonstrates how to use Serilog to send logs to Loki.
2. **Direct API Integration**: Shows how to directly POST logs to the Loki Push Logs API using a custom `LokiHandler`.

```shell
# Example usage
# Open the solution in Visual Studio and run the project.
```

### C++ Logging

For an example of logging in C++, see the [C++ logging example](./examples/cpp).
Make sure you install the dependencies for the example defined in the `vcpkg.json` file.

```shell
vcpkg install
cmake .
make
./Logging
```

### JavaScript Logging

For an example of logging in JavaScript, see the [JavaScript logging example](./examples/javascript).
This example demonstrates how to use the `winston-loki` transport to send logs to Loki.

```shell
npm install
node winston_loki_example.js
```

### PowerShell Logging

For an example of logging in PowerShell, see the [PowerShell logging example](./examples/powershell).
This example demonstrates how to directly POST logs to the Loki Push Logs API.

```powershell
powershell.exe -File post_logging_example.ps1
```

## Troubleshooting

### Loki Not Receiving Logs

- **Promtail**: Ensure that Promtail is correctly pointing to the Loki endpoint and is running without errors.
- **Alloy**: Check that Alloy is correctly scraping Docker metrics and forwarding them to Loki.

### Grafana Dashboard Not Displaying Data

- Verify that the Loki data source is correctly configured and connected in Grafana.
- Check the logs of the Grafana container to ensure it's functioning properly.

### Service Health Checks

- **Loki Services**: Check the health status of the `loki-read`, `loki-write`, and `loki-backend` services.
- **Grafana**: Ensure Grafana’s health check returns a successful status.
- **Minio**: Verify that Minio is accessible and functioning properly.

## Security Considerations

- **Credentials Management**: Store sensitive data like `MINIO_ROOT_PASSWORD` securely, preferably using environment variables or Docker secrets.
- **Secure Networking**: Consider securing network communication between services using Docker networks or external firewalls.
- **Grafana Authentication**: If anonymous access is enabled, be aware of the security implications. Use strong passwords and consider enabling authentication in production environments.

## Scaling the Stack

### Horizontal Scaling

- **Loki**: Add more read and write nodes to distribute the load. Ensure that the `loki-config.yml` is updated accordingly.
- **Grafana**: Deploy Grafana in a high-availability setup with load balancing.

### Prometheus Scaling

- **Remote Storage**: Consider setting up remote storage for Prometheus metrics if the volume becomes too large.
- **Multiple Instances**: Deploy multiple Prometheus instances for high availability.

## Backup and Restore

### Loki Data

- **Minio**: Back up the Minio data directory (`/path/to/your/minio/data`) regularly.
- **S3-Compatible Storage**: Use an S3-compatible tool to back up the logs stored in Minio.

### Prometheus Data

- **Snapshots**: Create snapshots of Prometheus data periodically and store them in a secure location.

### Grafana Dashboards

- **Export/Import**: Use Grafana’s export/import feature to back up dashboards. Store these JSON files in version control.

## Updating the Stack

### Updating Docker Images

- Pull the latest versions of the Docker images and restart the containers:

  ```sh
  docker-compose pull
  docker-compose up -d
  ```

### Configuration Changes

- Safely apply configuration changes by editing the relevant files and restarting the affected services:

  ```sh
  docker-compose up -d <service_name>
  ```

## Resource Requirements

### CPU & Memory Usage

- **Loki**: Ensure that Loki has sufficient memory and CPU resources, especially if handling high volumes of logs.
- **Prometheus**: Allocate enough resources to Prometheus, as it can be resource-intensive.

### Storage Requirements

- **Minio**: Estimate storage needs based on log volume and allocate sufficient disk space.
- **Loki**: Ensure that the storage backend (Minio) is performant and has enough capacity for long-term log retention.

## Metrics and Logs Visualization

### Example Dashboards

- Create dashboards in Grafana that visualize key metrics and logs. Example dashboards could include:

- **System Health**: CPU, memory, and disk usage.
- **Log Analysis**: Error rates, log frequency, and anomaly detection.

### Log Queries

- **Error Monitoring**:

  ```logql
  {system_logs="varlogs"} |= "error"
  ```

- **Performance Metrics**:

  ```logql
  {system_logs="varlogs"} | duration > 100ms
  ```

## License

This project is licensed under the [MIT License](LICENSE).

## Deployment to Production

### Best Practices

- **Monitoring and Alerts**: Set up Prometheus alerts to monitor the health of your stack in production.
- **Security**: Ensure that all sensitive data is securely managed, and access controls are enforced.
- **Backup**: Regularly back up your data, especially Minio storage and Prometheus metrics.

### CI/CD Integration

- **Automated Deployment**: Use a CI/CD pipeline to automate the deployment of your stack. Include steps for pulling the latest Docker images, applying configuration changes, and restarting services.
