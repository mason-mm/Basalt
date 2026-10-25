#include "Core/Logger.h"

int main(int argc, char** argv)
{
    Basalt::Logger::Init();
    Basalt::Logger::Shutdown();

    return 0;
}
