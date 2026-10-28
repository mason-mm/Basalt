// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

#include "Core/Framework/Actor.h"
#include "Core/Logger.h"

namespace Basalt
{
	Actor::Actor(const std::string& name)
		: m_Name(name)
	{
		BS_CORE_TRACE("Constructed actor: {0}", m_Name);
	}

	void Actor::BeginPlay()
	{
		BS_CORE_TRACE("Beginning play for actor: {0} ({1} components)", m_Name, m_Components.size());
		// Call BeginPlay for all components.
		for (auto& component : m_Components)
		{
			if (component)
			{
				component->BeginPlay();
			}
		}
	}

	void Actor::Tick(float deltaTime)
	{
		BS_CORE_TRACE("Ticking actor: {0} with delta time {1}", m_Name, deltaTime);
		// Call Tick for all components.
		for (auto& component : m_Components)
		{
			if (component)
			{
				component->Tick(deltaTime);
			}
		}
	}
}
