#pragma once

#include "Core/Base.h"

///	@file Application.h
/// @brief Holds the abstract application class for things like the BasaltEditor and final Game.

/// @brief Holds all classes and data types part of the core BASALT_API.
namespace Basalt
{
	// @brief Base class for applications.
	class BASALT_API Application
	{
	public:
		virtual ~Application() = default;

		// Prevent copying and moving of application instances.
		Application(const Application&) = delete;
		Application& operator=(const Application&) = delete;
		Application(Application&&) = delete;
		Application& operator=(Application&&) = delete;

		/// @brief Initializes the application.
		/// @returns If the application initialized successfully.
		virtual bool Init(const char* name);

		/// @brief Shuts down the application.
		virtual void Shutdown();

		/// @brief Called every frame.
		virtual void Tick() = 0;

	public:
		bool CanTick() const { return m_CanTick; }
		void SetCanTick(bool canTick) { m_CanTick = canTick; }

		bool IsRunning() const { return m_IsRunning; }
		void SetIsRunning(bool isRunning) { m_IsRunning = isRunning; }

		const char* Name() const { return m_Name; }
		void SetName(const char* name) { m_Name = name; }

	protected:
		Application() = default;

		bool m_CanTick = true;
		bool m_IsRunning = false;
		const char* m_Name = "New Application";
	};
}
