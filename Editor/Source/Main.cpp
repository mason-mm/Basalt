// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

#include "Core/Logger.h"
#include "LuaEngine/LuaEngine.h"

int main(int argc, char* argv[])
{
	Basalt::Logger::Init();
	Basalt::LuaEngine::Init();

	// Test BBT Platform Detection.
#if defined(BASALT_PLATFORM_WIN64)
	BS_CORE_INFO("Detected Platform: Windows");
#elif defined(BASALT_PLATFORM_LINUX)
	BS_CORE_INFO("Detected Platform: Linux");
#elif defined(BASALT_PLATFORM_MAC)
	BS_CORE_INFO("Detected Platform: MacOS");
#endif

	Basalt::LuaEngine::Shutdown();
	Basalt::Logger::Shutdown();
	return 0;
}
