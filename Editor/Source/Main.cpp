#include "Platform/EditorApplication.h"
#include "Core/Logger.h"
#include <iostream>

int main(int argc, char** argv)
{
    Basalt::Logger::Init();

    BasaltEditor::EditorApplication application;

    if (application.Init("Basalt Editor"))
    {
        application.SetIsRunning(true);

        // Example loop
        // Broken right after user pressed enter
        while (application.IsRunning())
        {
            application.Tick();

            std::cin.get();
            break;
        }

        application.Shutdown();
    }

    Basalt::Logger::Shutdown();

    return 0;
}