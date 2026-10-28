#pragma once

/// @file Logger.h
/// @brief Declares the logging API and logging macros used by the engine.

#include "Base.h"
#include <string>
#include <format>
#include <memory>
#include <cstdint>

/// @brief Contains the public classes and data types provided by BasaltCore.
namespace Basalt
{
	/// @brief Bit flags that identify the severity of a log message.
	enum class LogLevel : uint8_t
	{
		Trace = BS_BIT(0), ///< Diagnostic information intended for debugging.
		Info  = BS_BIT(1), ///< General information about application progress.
		Warn  = BS_BIT(2), ///< A condition that may require attention.
		Error = BS_BIT(3), ///< An error that does not necessarily stop the application.
		Fatal = BS_BIT(4), ///< A critical error that requires the application to abort.
		All   = 0xFF       ///< Enables all log levels.
	};

	/// @brief Formats and outputs log messages to the console.
	class BASALT_API Logger
	{
	public:
		/// @brief Initializes the logging system.
		static void Init();

		/// @brief Shuts down the logging system.
		static void Shutdown();

		/// @brief Configures the enabled log-level filter.
		/// @param levelFlags A bitmask composed of @ref LogLevel values.
		static void SetLevelFilter(uint8_t levelFlags);

		/// @brief Determines whether a log level is enabled by the current filter.
		/// @param level The log level to test.
		/// @returns `true` when messages at @p level should be output.
		static bool ShouldLog(LogLevel level);

		/// @brief Writes an unformatted message to the console.
		/// @note Callers should normally use @ref Log instead so that formatting and
		/// filtering are handled consistently.
		/// @param level The log level to use.
		/// @param category The message category, such as a class or subsystem name.
		/// @param message The message text to output.
		static void LogRaw(LogLevel level, const std::string& category, const std::string& message);

		/// @brief Formats and outputs a message through @ref LogRaw.
		/// @param level The log level to use.
		/// @param category The message category, such as a class or subsystem name.
		/// @param fmt The format string passed to `std::format`.
		/// @param args The values used to format @p fmt.
		template<typename... Args>
		static void Log(LogLevel level, const std::string& category, std::format_string<Args...> fmt, Args&&... args)
		{
			if (!ShouldLog(level)) return;
			std::string formattedMsg = std::format(fmt, std::forward<Args>(args)...);
			LogRaw(level, category, formattedMsg);
		}

	private:
		/// @brief The bitmask of currently enabled log levels.
		static uint8_t s_LevelFilter;
	};
} // namespace Basalt

// ----------------------------------------------------------------------------------------------------
// Logging macros
// ----------------------------------------------------------------------------------------------------

/// @brief Logs a trace message in the `CORE` category.
#define BS_CORE_TRACE(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Trace, "CORE", __VA_ARGS__)
/// @brief Logs an informational message in the `CORE` category.
#define BS_CORE_INFO(...)  ::Basalt::Logger::Log(::Basalt::LogLevel::Info,  "CORE", __VA_ARGS__)
/// @brief Logs a warning message in the `CORE` category.
#define BS_CORE_WARN(...)  ::Basalt::Logger::Log(::Basalt::LogLevel::Warn,  "CORE", __VA_ARGS__)
/// @brief Logs an error message in the `CORE` category.
#define BS_CORE_ERROR(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Error, "CORE", __VA_ARGS__)
/// @brief Logs a fatal message in the `CORE` category.
#define BS_CORE_FATAL(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Fatal, "CORE", __VA_ARGS__)

/// @brief Logs a trace message in the `APP` category.
#define BS_TRACE(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Trace, "APP",  __VA_ARGS__)
/// @brief Logs an informational message in the `APP` category.
#define BS_INFO(...)       ::Basalt::Logger::Log(::Basalt::LogLevel::Info,  "APP",  __VA_ARGS__)
/// @brief Logs a warning message in the `APP` category.
#define BS_WARN(...)       ::Basalt::Logger::Log(::Basalt::LogLevel::Warn,  "APP",  __VA_ARGS__)
/// @brief Logs an error message in the `APP` category.
#define BS_ERROR(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Error, "APP",  __VA_ARGS__)
/// @brief Logs a fatal message in the `APP` category.
#define BS_FATAL(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Fatal, "APP",  __VA_ARGS__)
