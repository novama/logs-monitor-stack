// Logging.cpp : Defines the entry point for the application.
//

#include "LokiLoggerSink.h"
#include <spdlog/spdlog.h>
#include <spdlog/sinks/stdout_color_sinks.h>
#include <memory>

int main() {
    try {
        // Define some example labels for Loki
        std::map<std::string, std::string> labels = {
            {"environment", "dev"},
            {"application", "cpp"},
            {"host", "my-computer"},
            {"service", "example_service"}
        };

        // Create the LokiLoggerSink
        std::string loki_url = "http://localhost:3100";
        // Optional: Define Loki's tenant if multi-tenancy is used
        // For our log-monitor-stack setup, we use "tenant1"
        std::string tenant = "tenant1";
        // Optional: Define Loki's user and password if authentication is required
        // For our log-monitor-stack setup, no authentication is needed
        std::string user = "";
        // Important: In Grafana Cloud, an API key is used as the password
        std::string password = "";

        // Create the Loki sink
        auto loki_sink = std::make_shared<loki_sink_mt>(loki_url, labels, tenant, user, password);

        // Create other sinks if needed (e.g., console sink)
        auto console_sink = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();

        // Set a custom pattern for the console sink with color codes
        console_sink->set_pattern("%^[%Y-%m-%d %H:%M:%S.%e] [%l] %v%$");

        // Combine the sinks into a logger
        std::vector<spdlog::sink_ptr> sinks{ console_sink, loki_sink };
        auto logger = std::make_shared<spdlog::logger>("multi_sink", sinks.begin(), sinks.end());

        // Set the logger as the default
        spdlog::set_default_logger(logger);

        // Set log level (optional)
        logger->set_level(spdlog::level::debug);
        logger->flush_on(spdlog::level::debug);

        // Post some log messages
        spdlog::info("This is an informational message posted from C++");
        spdlog::warn("This is a warning message message posted from C++");
        spdlog::error("This is an error message message posted from C++");
		spdlog::debug("This is a debug message message posted from C++");

		std::cout << "Execution complete. Check Loki server for logs." << std::endl;

    } catch (const std::exception& e) {
        std::cerr << "Exception occurred: " << e.what() << std::endl;
    } catch (...) {
        std::cerr << "Unknown exception occurred!" << std::endl;
    }

    return 0;
}


