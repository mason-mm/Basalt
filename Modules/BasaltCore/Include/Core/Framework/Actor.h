#pragma once

/// @file Actor.h
/// @brief Declares the actor base class and its component-management API.

#include "Core/Base.h"
#include "Core/Framework/Component.h"
#include <vector>
#include <memory>
#include <string>
#include <type_traits>
#include <utility>

/// @brief Contains the public classes and data types provided by BasaltCore.
namespace Basalt
{
	#if defined(_MSC_VER)
	#pragma warning(push)
	#pragma warning(disable: 4251)
	#endif

	/// @brief Base class for an object that can be placed in the world.
	///
	/// An actor owns its @ref Component instances and forwards lifecycle calls to
	/// them. Actors are not copyable or movable.
	class BASALT_API Actor
	{
	public:
		/// @brief Creates an actor with the specified name.
		/// @param name The name assigned to the actor.
		Actor(const std::string& name = "NewActor");
		virtual ~Actor() = default;

		// Prevent copying.
		Actor(const Actor&) = delete;
		Actor& operator=(const Actor&) = delete;
		Actor(Actor&&) = delete;
		Actor& operator=(Actor&&) = delete;

		/// @brief Called after all engine systems have initialized.
		///
		/// The default implementation calls @ref Component::BeginPlay for each
		/// component owned by this actor.
		virtual void BeginPlay();

		/// @brief Updates the actor and its components once per frame.
		/// @param deltaTime The elapsed time since the previous frame, in seconds.
		///
		/// The default implementation calls @ref Component::Tick for each
		/// component owned by this actor.
		virtual void Tick(float deltaTime);

		/// @brief Creates and attaches a component to the actor.
		/// @tparam T The @ref Component-derived type to create.
		/// @tparam Args The additional constructor argument types for @p T.
		/// @param args The arguments forwarded to the constructor of @p T.
		/// @returns A pointer to the newly attached component.
		template<typename T, typename... Args>
		T* AddComponent(Args&&... args)
		{
			static_assert(std::is_base_of_v<Component, T>, "T must derive from Basalt::Component.");
			auto component = std::make_unique<T>(this, std::forward<Args>(args)...);
			T* ptr = component.get();
			m_Components.push_back(std::move(component));
			return ptr;
		}

		/// @brief Finds the first component of the requested type.
		/// @tparam T The @ref Component-derived type to find.
		/// @returns A pointer to the matching component, or `nullptr` if none exists.
		template<typename T>
		T* GetComponent() const
		{
			static_assert(std::is_base_of_v<Component, T>, "T must derive from Basalt::Component.");
			for (const auto& comp : m_Components)
			{
				if (T* casted = dynamic_cast<T*>(comp.get()))
				{
					return casted;
				}
			}
			return nullptr;
		}

		/// @brief Gets the actor's name.
		/// @returns A reference to the actor's name.
		const std::string& GetName() const { return m_Name; }

		/// @brief Sets the actor's name.
		/// @param name The new name of the actor.
		void SetName(const std::string& name) { m_Name = name; }

	protected:
		/// @brief The actor's name.
		std::string m_Name;

		/// @brief The components owned by the actor.
		std::vector<std::unique_ptr<Component>> m_Components;
	};

	#if defined(_MSC_VER)
	#pragma warning(pop)
	#endif
}
