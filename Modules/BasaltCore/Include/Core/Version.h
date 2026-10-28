// Copyright (c) 2026 Mason Maday. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for full license information.

#pragma once

/// @file Version.h
/// @brief Defines the BasaltCore version components and formatted version string.

#include <format>

/// @brief The major BasaltCore version component.
#define B_VERSION_MAJOR 0

/// @brief The minor BasaltCore version component.
#define B_VERSION_MINOR 1

/// @brief The patch BasaltCore version component.
#define B_VERSION_PATCH 0

/// @brief Formats the BasaltCore version as `major.minor.patch`.
#define B_VERSION_STRING std::format("{}.{}.{}", B_VERSION_MAJOR, B_VERSION_MINOR, B_VERSION_PATCH)
