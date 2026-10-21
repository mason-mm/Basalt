#pragma once

#include "Core/Framework/Application.h"

/// @file EditorApplication.h
/// @brief Holds the EditorApplication class.

/// @brief Namespace for all classes in the BasaltEditor.
namespace BasaltEditor
{
	/// @brief Main editor class
	class EditorApplication : public Basalt::Application
	{
	public:
		bool Init(const char* name) override;
		void Shutdown() override;
		void Tick() override;
	};
}
