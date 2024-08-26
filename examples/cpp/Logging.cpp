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
            {"application", "example_app"},
            {"environment", "dev"},
            {"host", "localhost"},
            {"service", "example_service"}
        };

        // Create the LokiLoggerSink
        std::string loki_url = "http://localhost:3100/loki/api/v1/push";
        auto loki_sink = std::make_shared<loki_sink_mt>(loki_url, labels, "tenant1");

        // Create other sinks if needed (e.g., console sink)
        auto console_sink = std::make_shared<spdlog::sinks::stdout_color_sink_mt>();

        // Combine the sinks into a logger
        std::vector<spdlog::sink_ptr> sinks{ console_sink, loki_sink };
        auto logger = std::make_shared<spdlog::logger>("multi_sink", sinks.begin(), sinks.end());

        // Set the logger as the default
        spdlog::set_default_logger(logger);

        // Set log level (optional)
        logger->set_level(spdlog::level::info);
        logger->flush_on(spdlog::level::info);

        // Log some example messages to Loki
        spdlog::info("This is an informational message posted from C++");
        spdlog::warn("This is a warning message message posted from C++");
        spdlog::error("This is an error message message posted from C++");

    } catch (const std::exception& e) {
        std::cerr << "Exception occurred: " << e.what() << std::endl;
    } catch (...) {
        std::cerr << "Unknown exception occurred!" << std::endl;
    }

    return 0;
}


