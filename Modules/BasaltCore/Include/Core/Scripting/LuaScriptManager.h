// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

#pragma once

/// @file LuaScriptManager.h
/// @brief Declares the bridge between BasaltCore and BasaltLuaEngine.

#include "Core/Base.h"
#include "Core/Logger.h"
#include <string>

/// @brief Contains the public classes and data types provided by BasaltCore.
namespace Basalt
{
	// Forward declarations.
	class Actor;

	/// @brief Signature of the callback used to invoke a Lua function.
	/// @param scriptPath The path to the Lua script.
	/// @param functionName The name of the Lua function to invoke.
	/// @param owner The actor associated with the invocation.
	/// @param deltaTime The elapsed time since the previous frame, in seconds.
	using ScriptInvokeFn = void(*)(const std::string& scriptPath, const std::string& functionName, Actor* owner, float deltaTime);

	/// @brief Bridges script calls from BasaltCore to BasaltLuaEngine.
	///
	/// @ref LuaScriptComponent uses this class to forward lifecycle and custom
	/// events without making BasaltCore depend directly on the Lua runtime.
	class BASALT_API LuaScriptManager
	{
	public:
		/// @brief Registers the function used to invoke Lua callbacks.
		/// @param fn The callback function supplied by BasaltLuaEngine.
		/// @note This should be called by BasaltLuaEngine during initialization.
		static void RegisterInvokeCallback(ScriptInvokeFn fn)
		{
			BS_CORE_TRACE("Registering Lua script invocation callback.");
			s_InvokeCallback = fn;
		}

		/// @brief Invokes a Lua function through the registered callback.
		/// @param path The path to the Lua script.
		/// @param functionName The name of the Lua function to invoke.
		/// @param owner The actor associated with the invocation.
		/// @param deltaTime The elapsed time since the previous frame, in seconds.
		/// @note If no callback has been registered, this function does nothing.
		static void CallFunction(const std::string& path, const std::string& functionName, Actor* owner, float deltaTime = 0.0f)
		{
			BS_CORE_TRACE("Dispatching Lua function '{0}' from script: {1}", functionName, path);
			if (s_InvokeCallback)
			{
				s_InvokeCallback(path, functionName, owner, deltaTime);
			}
		}

	private:
		/// @brief The callback registered by BasaltLuaEngine.
		inline static ScriptInvokeFn s_InvokeCallback;
	};
}
