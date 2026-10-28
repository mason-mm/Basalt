// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

#include "Core/Logger.h"
#include "Core/Framework/Actor.h"
#include "Core/Framework/Component.h"
#include "Core/GameFramework/LuaScriptComponent.h"
#include "Core/Scripting/LuaScriptManager.h"
#include "LuaEngine/LuaEngine.h"
#include <string>

namespace
{
	struct ScriptCallState
	{
		int callCount = 0;
		std::string scriptPath;
		std::string functionName;
		Basalt::Actor* owner = nullptr;
		float deltaTime = 0.0f;
	};

	ScriptCallState g_ScriptCallState;

	void RecordScriptCall(const std::string& scriptPath, const std::string& functionName, Basalt::Actor* owner, float deltaTime)
	{
		++g_ScriptCallState.callCount;
		g_ScriptCallState.scriptPath = scriptPath;
		g_ScriptCallState.functionName = functionName;
		g_ScriptCallState.owner = owner;
		g_ScriptCallState.deltaTime = deltaTime;
	}

	class TestComponent : public Basalt::Component
	{
	public:
		explicit TestComponent(Basalt::Actor* owner)
			: Component(owner)
		{
		}

		void BeginPlay() override
		{
			++beginPlayCount;
		}

		void Tick(float deltaTime) override
		{
			++tickCount;
			lastDeltaTime = deltaTime;
		}

		int beginPlayCount = 0;
		int tickCount = 0;
		float lastDeltaTime = 0.0f;
	};

	bool Check(bool condition, const char* description)
	{
		if (condition)
		{
			BS_CORE_INFO("[PASS] {0}", description);
			return true;
		}

		BS_CORE_ERROR("[FAIL] {0}", description);
		return false;
	}

	bool TestActorAndComponentLifecycle()
	{
		BS_CORE_INFO("Running actor and component lifecycle tests.");
		Basalt::Actor actor("TestActor");
		TestComponent* component = actor.AddComponent<TestComponent>();
		bool passed = true;

		passed &= Check(actor.GetName() == "TestActor", "Actor stores its name.");
		passed &= Check(actor.GetComponent<TestComponent>() == component, "Actor returns its component by type.");
		passed &= Check(component->GetOwner() == &actor, "Component stores its owning actor.");

		actor.BeginPlay();
		actor.Tick(0.016f);
		passed &= Check(component->beginPlayCount == 1, "Actor forwards BeginPlay to its components.");
		passed &= Check(component->tickCount == 1 && component->lastDeltaTime == 0.016f, "Actor forwards Tick to its components.");

		return passed;
	}

	bool TestLoggerFiltering()
	{
		BS_CORE_INFO("Running logger filtering tests.");
		Basalt::Logger::SetLevelFilter(static_cast<uint8_t>(Basalt::LogLevel::Trace));
		const bool passed = Basalt::Logger::ShouldLog(Basalt::LogLevel::Trace)
			&& !Basalt::Logger::ShouldLog(Basalt::LogLevel::Info);
		Basalt::Logger::SetLevelFilter(static_cast<uint8_t>(Basalt::LogLevel::All));
		return Check(passed, "Logger filters enabled log levels.");
	}

	bool TestLuaScriptComponent()
	{
		BS_CORE_INFO("Running Lua script component tests.");
		g_ScriptCallState = {};
		Basalt::LuaScriptManager::RegisterInvokeCallback(RecordScriptCall);

		Basalt::Actor actor("LuaTestActor");
		Basalt::LuaScriptComponent* scriptComponent = actor.AddComponent<Basalt::LuaScriptComponent>("Tests/Test.lua");
		scriptComponent->BeginPlay();
		scriptComponent->Tick(0.016f);
		scriptComponent->TriggerCustomEvent("OnTestEvent");

		bool passed = true;
		passed &= Check(g_ScriptCallState.callCount == 3, "Lua component forwards lifecycle and custom events.");
		passed &= Check(g_ScriptCallState.scriptPath == "Tests/Test.lua", "Lua callback receives the script path.");
		passed &= Check(g_ScriptCallState.functionName == "OnTestEvent", "Lua callback receives the function name.");
		passed &= Check(g_ScriptCallState.owner == &actor, "Lua callback receives the owning actor.");
		passed &= Check(g_ScriptCallState.deltaTime == 0.0f, "Custom Lua events use a zero delta time.");

		Basalt::LuaScriptComponent emptyScriptComponent(&actor);
		emptyScriptComponent.BeginPlay();
		passed &= Check(g_ScriptCallState.callCount == 3, "An empty script path does not invoke callbacks.");
		return passed;
	}

	bool TestLuaEngine()
	{
		BS_CORE_INFO("Running Lua engine tests.");
		const bool passed = Basalt::LuaEngine::ExecuteString("return 1")
			&& Basalt::LuaEngine::ExecuteFile("Tests/Test.lua");
		return Check(passed, "Lua engine execution methods report success.");
	}

	int RunTests()
	{
		BS_CORE_INFO("Starting engine unit tests.");
		const bool passed = TestActorAndComponentLifecycle()
			&& TestLoggerFiltering()
			&& TestLuaScriptComponent()
			&& TestLuaEngine();
		BS_CORE_INFO("Engine unit tests {0}.", passed ? "passed" : "failed");
		return passed ? 0 : 1;
	}
}

int main(int argc, char* argv[])
{
	Basalt::Logger::Init();
	Basalt::LuaEngine::Init();
	const int testResult = RunTests();
	Basalt::LuaEngine::Init();

	BS_CORE_INFO("Starting Simulation...");

	{
		auto playerActor = std::make_unique<Basalt::Actor>("HeroPlayer");

		auto luaScriptComp = playerActor->AddComponent<Basalt::LuaScriptComponent>("Scripts/PlayerHud.lua");

		playerActor->BeginPlay();

		float simulatedDeltaTime = 0.016f;
		for (int frame = 0; frame < 6; frame++)
		{
			BS_CORE_TRACE("-- Frame {0} --", frame);
			playerActor->Tick(simulatedDeltaTime);
		}
	}

	BS_CORE_INFO("Ending Simulation...");

	Basalt::LuaEngine::Shutdown();
	Basalt::Logger::Shutdown();
	return testResult;
}
