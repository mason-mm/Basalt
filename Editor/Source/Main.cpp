#include "Core/Logger.h"

int main(int argc, char** argv)
{
	Basalt::Logger::Init();

	// Test logging

    BS_CORE_TRACE("Testing Core Trace: {0}", 42);
    BS_CORE_INFO("Engine Version: {0}.{1}.{2}", 1, 0, 0);
    BS_CORE_WARN("Texture '{0}' not found, using fallback pattern.", "T_Brick_Diffuse.png");
    BS_CORE_ERROR("Failed to allocate RDI buffer of size: {0} bytes", 1024 * 1024 * 64);

    BS_INFO("Game Logic Tick Rate: {0} FPS", 120.0f);

    BS_CORE_INFO("--- Applying Filter: Hiding TRACE and WARN ---");
    Basalt::Logger::SetLevelFilter(
        static_cast<uint8_t>(Basalt::LogLevel::Info) |
        static_cast<uint8_t>(Basalt::LogLevel::Error) |
        static_cast<uint8_t>(Basalt::LogLevel::Fatal)
    );

    BS_CORE_TRACE("This TRACE log should NOT appear.");
    BS_CORE_WARN("This WARN log should NOT appear.");
    BS_CORE_ERROR("This ERROR log SHOULD still appear!");

    Basalt::Logger::Shutdown();
    return 0;
}
