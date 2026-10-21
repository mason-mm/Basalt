#include "Platform/EditorApplication.h"

namespace BasaltEditor
{
	bool EditorApplication::Init(const char* name)
	{
		Basalt::Application::Init(name);
		return true;
	}

	void EditorApplication::Shutdown()
	{
		Basalt::Application::Shutdown();
	}

	void EditorApplication::Tick()
	{
	}
}
