#include "Core/GameFramework/LuaScriptComponent.h"
#include "Core/Logger.h"
#include "Core/Scripting/LuaScriptManager.h"

namespace Basalt
{
	LuaScriptComponent::LuaScriptComponent(Actor* owner, const std::string& scriptPath)
		: Component(owner), m_ScriptPath(scriptPath)
	{
		BS_CORE_TRACE("Constructed Lua script component for script: {0}", m_ScriptPath.empty() ? "<none>" : m_ScriptPath);
	}

	void LuaScriptComponent::BeginPlay()
	{
		BS_CORE_TRACE("Beginning Lua script component play for script: {0}", m_ScriptPath.empty() ? "<none>" : m_ScriptPath);
		if (!m_ScriptPath.empty())
		{
			LuaScriptManager::CallFunction(m_ScriptPath, "BeginPlay", m_Owner, 0.0f);
		}
	}

	void LuaScriptComponent::Tick(float deltaTime)
	{
		BS_CORE_TRACE("Ticking Lua script component for script: {0} with delta time {1}", m_ScriptPath.empty() ? "<none>" : m_ScriptPath, deltaTime);
		if (!m_ScriptPath.empty())
		{
			LuaScriptManager::CallFunction(m_ScriptPath, "Tick", m_Owner, deltaTime);
		}
	}

	void LuaScriptComponent::TriggerCustomEvent(const std::string& eventName)
	{
		BS_CORE_TRACE("Triggering Lua event '{0}' for script: {1}", eventName, m_ScriptPath.empty() ? "<none>" : m_ScriptPath);
		if (!m_ScriptPath.empty())
		{
			LuaScriptManager::CallFunction(m_ScriptPath, eventName, m_Owner, 0.0f);
		}
	}
}
