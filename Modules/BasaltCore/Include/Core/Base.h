#pragma once

/// @file Base.h
/// @brief Defines common export and bit-mask macros used by BasaltCore.

#if defined(_WIN32) || defined(_WIN64)
	#if defined(BS_CORE_EXPORTS)
		#define BASALT_API __declspec(dllexport)
	#else
		#define BASALT_API __declspec(dllimport)
	#endif
#else
	#define BASALT_API __attribute__((visibility("default")))
#endif

/// @brief Creates a bit mask with bit @p x set.
/// @param x The zero-based bit position to set.
#define BS_BIT(x) (1 << (x))
