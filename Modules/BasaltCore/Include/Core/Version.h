#pragma once

#include <format>

#define B_VERSION_MAJOR 0
#define B_VERSION_MINOR 1
#define B_VERSION_PATCH 0
#define B_VERSION_STRING std::format("{}.{}.{}", B_VERSION_MAJOR, B_VERSION_MINOR, B_VERSION_PATCH)
