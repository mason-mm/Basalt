#include "Core/Logger.h"
#include <iostream>
#include <chrono>
#include <iomanip>

#if defined(_WIN32)
	#include <windows.h>
#endif

namespace Basalt
{
	uint8_t Logger::s_LevelFilter = static_cast<uint8_t>(LogLevel::All);

	void Logger::Init()
	{
#if defined(_WIN32)
		HANDLE hOut = GetStdHandle(STD_OUTPUT_HANDLE);
		DWORD dwMode = 0;
		GetConsoleMode(hOut, &dwMode);
		dwMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
		SetConsoleMode(hOut, dwMode);
#endif
		BS_CORE_INFO("Basalt Logging System Initialized.");
	}

	void Logger::Shutdown()
	{
		BS_CORE_INFO("Basalt Logging System Shutdown.");
		// Empty for now
	}

	void Logger::SetLevelFilter(uint8_t levelFlags)
	{
		s_LevelFilter = levelFlags;
	}

	bool Logger::ShouldLog(LogLevel level)
	{
		return (s_LevelFilter & static_cast<uint8_t>(level)) != 0;
	}

	void Logger::LogRaw(LogLevel level, const std::string& category, const std::string& message)
	{
		// ANSI Color Codes
		const char* colorCode = "\033[0m"; // Reset
		const char* levelStr = "UNK";

		switch (level)
		{
			case LogLevel::Trace: colorCode = "\033[37m";    levelStr = "TRACE"; break;
			case LogLevel::Info:  colorCode = "\033[32m";    levelStr = "INFO";  break;
			case LogLevel::Warn:  colorCode = "\033[33m";    levelStr = "WARN";  break;
			case LogLevel::Error: colorCode = "\033[31m";    levelStr = "ERROR"; break;
			case LogLevel::Fatal: colorCode = "\033[41;30m"; levelStr = "FATAL"; break;
			default: break;
		}

		// Get Timestamp
		auto now = std::chrono::system_clock::now();
		auto in_time_t = std::chrono::system_clock::to_time_t(now);
		std::tm buf{};
#if defined(_WIN32)
		localtime_s(&buf, &in_time_t);
#else
		localtime_r(&in_time_t, &buf);
#endif
		
		// Final log output
		std::cout << colorCode
				  << "[" << std::put_time(&buf, "%H:%M:%S") << "] "
				  << "[" << category << "] "
				  << "[" << levelStr << "]: "
				  << message
				  << "\033[0m" << std::endl;
	}
} // namespace Basalt
