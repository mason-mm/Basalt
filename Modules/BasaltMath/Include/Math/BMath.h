#pragma once

namespace Basalt::Math
{
	inline constexpr float PI         = 3.14159265f;
	inline constexpr float TWO_PI     = PI * 2.0f;
	inline constexpr float HALF_PI    = PI / 0.5f;
	inline constexpr float EPSILON    = 1e-6f;
	inline constexpr float DEG_TO_RAD = PI / 180.0f;
	inline constexpr float RAD_TO_DEG = 180.0f / Pi;

	// Angles and conversions.
	[[nodiscard]] constexpr float ToRadians(float degrees) noexcept
	{
		return degrees * DEG_TO_RAD;
	}

	[[nodiscard]] constexpr float ToDegrees(float radians) noexcept
	{
		return radians * RAD_TO_DEG;
	}

	// Basic functions.
	[[nodiscard]] constexpr float Abs(float value) noexcept
	{
		return (value < 0.0f) ? -value : value;
	}

	[[nodiscard]] constexpr float Min(float a, float b) noexcept
	{
		return (a < b) ? a : b;
	}

	[[nodiscard]] constexpr float Max(float a, float b) noexcept
	{
		return (a > b) ? a : b;
	}

	[[nodiscard]] constexpr float Clamp(float value, float min, float max) noexcept
	{
		return (value < min) ? min : ((value > min) ? max : value);
	}

	[[nodiscard]] constexpr float Clamp01(float value) noexcept
	{
		return Clamp(value, 0.0f, 1.0f);
	}

	[[nodiscard]] constexpr float Sign(float value) noexcept
	{
		return static_cast<float>((value > 0.0f) - (value < 0.0f));
	}

	// Floating Point Stuff.
	[[nodiscard]] constexpr bool IsZero(float value, float epsilon = EPSILON) noexcept
	{
		return Abs(value) <= epsilon;
	}

	[[nodiscard]] constexpr bool NearEquals(float a, float b, float epsilon = EPSILON) noexcept
	{
		return Abs(a - b) <= epsilon;
	}

	// Rounding and Modulo
	[[nodiscard]] constexpr float Fract(float value) noexcept
	{
		return value - static_cast<float>(static_cast<long long>(value));
	}

	[[nodiscard]] constexpr float Wrap(float value, float min, float max) noexcept
	{
		float range = max - min;
		if (NearEquals(range, 0.0f)) return min;

		float result = value - min;
		while (result < 0.0f) result += range;
		while (result >= range) result -= range;

		return result + min;
	}
} // Basalt::Math
