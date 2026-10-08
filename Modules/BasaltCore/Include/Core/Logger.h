#pragma once

/// @file Logger.h
/// @brief Holds the logger class and logging macros for the engine.

#include "Base.h"
#include <string>
#include <format>
#include <memory>
#include <cstdint>

/// @brief Holds all classes and data types part of the core BASALT_API.
namespace Basalt
{
	/// @brief Holds all of the log levels for the Logger class (Represented by bits).
	enum class LogLevel : uint8_t
	{
		Trace = BS_BIT(0), ///< For information related to debugging.
		Info  = BS_BIT(1), ///< For regular information related to the flow of the application.
		Warn  = BS_BIT(2), ///< For things that might cause errors and needs attention.
		Error = BS_BIT(3), ///< For errors that still allow the application to keep running.
		Fatal = BS_BIT(4), ///< For errors that must stop the applications progress and results in an abort.
		All   = 0xFF       ///< Enabled all log levels/filters.
	};

	/// @brief Handles formatting and outputing log messages to the console.
	class BASALT_API Logger
	{
	public:
		/// @brief Initializes the Logger.
		static void Init();

		/// @brief Shuts down the Logger.
		static void Shutdown();

		/// @brief Configure log filtering bitmask.
		/// @param levelFlags: The flags to change the filter for.
		static void SetLevelFilter(uint8_t levelFlags);

		/// @brief Changes whether or not the logger should output at a certain log level.
		/// @param level: The log level to mute or unmute.
		static bool ShouldLog(LogLevel level);

		/// @brief Logs raw text to the console.
		/// @note Be careful when using this outside of the Logger class.
		/// @param level: The log level to use.
		/// @param category: The category for the log, such as a class or struct.
		/// @param message: The string message to be outputted.
		static void LogRaw(LogLevel level, const std::string& category, const std::string& message);

		/// @brief Formats a message and outputs it using LogRaw.
		/// @param level: The log level to use.
		/// @param category: The category for the log, such as a class or struct.
		template<typename... Args>
		static void Log(LogLevel level, const std::string& category, std::format_string<Args...> fmt, Args&&... args)
		{
			if (!ShouldLog(level)) return;
			std::string formattedMsg = std::format(fmt, std::forward<Args>(args)...);
			LogRaw(level, category, formattedMsg);
		}

	private:
		/// @brief The current level filters
		static uint8_t s_LevelFilter;
	};
} // namespace Basalt

// ----------------------------------------------------------------------------------------------------
// Logging macros
// ----------------------------------------------------------------------------------------------------

/* Engine Logs **/
#define BS_CORE_TRACE(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Trace, "CORE", __VA_ARGS__)
#define BS_CORE_INFO(...)  ::Basalt::Logger::Log(::Basalt::LogLevel::Info,  "CORE", __VA_ARGS__)
#define BS_CORE_WARN(...)  ::Basalt::Logger::Log(::Basalt::LogLevel::Warn,  "CORE", __VA_ARGS__)
#define BS_CORE_ERROR(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Error, "CORE", __VA_ARGS__)
#define BS_CORE_FATAL(...) ::Basalt::Logger::Log(::Basalt::LogLevel::Fatal, "CORE", __VA_ARGS__)

/* App/Game Logs **/
#define BS_TRACE(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Trace, "APP",  __VA_ARGS__)
#define BS_INFO(...)       ::Basalt::Logger::Log(::Basalt::LogLevel::Info,  "APP",  __VA_ARGS__)
#define BS_WARN(...)       ::Basalt::Logger::Log(::Basalt::LogLevel::Warn,  "APP",  __VA_ARGS__)
#define BS_ERROR(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Error, "APP",  __VA_ARGS__)
#define BS_FATAL(...)      ::Basalt::Logger::Log(::Basalt::LogLevel::Fatal, "APP",  __VA_ARGS__)
