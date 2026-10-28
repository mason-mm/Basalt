#include "LuaEngine/LuaEngine.h"
#include "Core/Logger.h"
#include "Core/Scripting/LuaScriptManager.h"

namespace Basalt
{
	void LuaEngine::Init()
	{
		LuaScriptManager::RegisterInvokeCallback([](const std::string& path, const std::string& functionName, Actor* owner, float deltaTime)
		{
			BS_CORE_TRACE("Lua callback received for function '{0}' from script: {1}", functionName, path);
			// Lua function invocation will be implemented here.
		});

		BS_CORE_INFO("Basalt Lua engine initialized.");
	}

	void LuaEngine::Shutdown()
	{
		BS_CORE_INFO("Basalt Lua engine shut down.");
	}

	bool LuaEngine::ExecuteString(const std::string& code)
	{
		BS_CORE_TRACE("Executing Lua string: {0}", code);
		return true;
	}

	bool LuaEngine::ExecuteFile(const std::string& filepath)
	{
		BS_CORE_TRACE("Executing Lua script file: {0}", filepath);
		return true;
	}
}
