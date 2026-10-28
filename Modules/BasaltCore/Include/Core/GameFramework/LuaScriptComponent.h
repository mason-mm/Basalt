#pragma once

/// @file LuaScriptComponent.h
/// @brief Declares the component that forwards actor events to a Lua script.

#include "Core/Base.h"
#include "Core/Framework/Component.h"
#include <string>

/// @brief Contains the public classes and data types provided by BasaltCore.
namespace Basalt
{
	// Forward declarations.
	class LuaEngine;

	#if defined(_MSC_VER)
	#pragma warning(push)
	#pragma warning(disable: 4251)
	#endif

	/// @brief Component for attaching a Lua script to an @ref Actor.
	///
	/// The component forwards @ref Component::BeginPlay, @ref Component::Tick,
	/// and custom events to the registered @ref LuaScriptManager callback.
	class BASALT_API LuaScriptComponent : public Component
	{
	public:
		/// @brief Creates a Lua script component.
		/// @param owner The actor that owns the component.
		/// @param scriptPath The path to the Lua script, or an empty string to
		/// disable script callbacks.
		LuaScriptComponent(Actor* owner, const std::string& scriptPath = "");
		virtual ~LuaScriptComponent() override = default;

		/// @brief Invokes the script's `BeginPlay` function.
		///
		/// No callback is made when the script path is empty.
		virtual void BeginPlay() override;

		/// @brief Invokes the script's `Tick` function.
		/// @param deltaTime The elapsed time since the previous frame, in seconds.
		///
		/// No callback is made when the script path is empty.
		virtual void Tick(float deltaTime) override;

		/// @brief Invokes a custom function in the script.
		/// @param eventName The name of the Lua function to invoke.
		///
		/// No callback is made when the script path is empty.
		virtual void TriggerCustomEvent(const std::string& eventName);

		/// @brief Gets the path of the Lua script.
		/// @returns A reference to the configured script path.
		const std::string& GetScriptPath() const { return m_ScriptPath; }

		/// @brief Sets the path to the Lua script.
		/// @param path The new script path. An empty path disables callbacks.
		void SetScriptPath(const std::string& path) { m_ScriptPath = path; }

	private:
		/// @brief The path to the configured Lua script.
		std::string m_ScriptPath;
	};

	#if defined(_MSC_VER)
	#pragma warning(pop)
	#endif
}
