#pragma once

/// @file LuaEngine.h
/// @brief Declares the Lua runtime integration API.

#include <string>

namespace Basalt
{
	/// @brief Provides lifecycle and script-execution operations for Lua.
	///
	/// The engine registers its invocation bridge with
	/// @ref LuaScriptManager during initialization so that
	/// @ref LuaScriptComponent can forward actor events to Lua.
	class LuaEngine
	{
	public:
		/// @brief Initializes the Lua engine and registers its script callback.
		static void Init();

		/// @brief Shuts down the Lua engine.
		static void Shutdown();

		/// @brief Executes Lua source code provided as a string.
		/// @param code The Lua source code to execute.
		/// @returns `true` when execution succeeds.
		/// @note The current implementation logs the request and returns `true`
		/// without executing the code.
		static bool ExecuteString(const std::string& code);

		/// @brief Executes a Lua script file.
		/// @param filepath The path to the Lua script file.
		/// @returns `true` when execution succeeds.
		/// @note The current implementation logs the request and returns `true`
		/// without reading or executing the file.
		static bool ExecuteFile(const std::string& filepath);
	};
}
