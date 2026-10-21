#include "Core/Framework/Application.h"
#include "Core/Logger.h"
#include "Core/Version.h"

namespace Basalt
{
	bool Application::Init(const char* name)
	{
		m_IsRunning = true;
		m_Name = name;
		BS_CORE_INFO("Application '{0}' initialized on Basalt version {1}.", name, B_VERSION_STRING);

		return true;
	}

	void Application::Shutdown()
	{
		m_IsRunning = false;
		BS_CORE_INFO("Application '{0}' shutdown.", m_Name);
	}
}
