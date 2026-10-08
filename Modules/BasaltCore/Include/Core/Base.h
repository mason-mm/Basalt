#pragma once

#if defined(_WIN32) || defined(_WIN64)
	#if defined(BS_CORE_EXPORTS)
		#define BASALT_API __declspec(dllexport)
	#else
		#define BASALT_API __declspec(dllexport)
	#endif
#else
	#define BASALT_API __attribute__((visibility("default")))
#endif

#define BS_BIT(x) (1 << (x))
