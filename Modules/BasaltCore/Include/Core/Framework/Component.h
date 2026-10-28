#pragma once

/// @file Component.h
/// @brief Declares the base component class.

#include "Core/Base.h"

/// @brief Contains the public classes and data types provided by BasaltCore.
namespace Basalt
{
	// Forward declarations.
	class Actor;

	/// @brief Base class for behavior attached to an @ref Actor.
	///
	/// Components are owned by their actor and receive lifecycle callbacks from
	/// it. Components are not copyable or movable.
	class BASALT_API Component
	{
	public:
		/// @brief Creates a component owned by an actor.
		/// @param owner The actor to which the component is attached.
		Component(Actor* owner) : m_Owner(owner) {}
		virtual ~Component() = default;
		
		// Prevent copying.
		Component(const Component&) = delete;
		Component& operator=(const Component&) = delete;
		Component(Component&&) = delete;
		Component& operator=(Component&&) = delete;

		/// @brief Called after all engine systems have initialized.
		///
		/// The default implementation does nothing.
		virtual void BeginPlay() {}

		/// @brief Updates the component once per frame.
		/// @param deltaTime The elapsed time since the previous frame, in seconds.
		///
		/// The default implementation does nothing.
		virtual void Tick(float deltaTime) {}

		/// @brief Gets the actor that owns the component.
		/// @returns The owning @ref Actor, or `nullptr` when no owner was supplied.
		Actor* GetOwner() const { return m_Owner; }

	protected:
		/// @brief The actor that owns this component.
		Actor* m_Owner = nullptr;
	};
}
