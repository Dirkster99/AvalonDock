using System;

namespace AvalonDock
{
	/// <summary>
	/// Provides helper members for math Helper.
	/// </summary>
	internal static class MathHelper
	{
		/// <summary>
		/// Executes the min Max operation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="min">The min.</param>
		/// <param name="max">The max.</param>
		/// <returns>The result of the operation.</returns>
		public static double MinMax(double value, double min, double max)
		{
			if (min > max) throw new ArgumentException("The minimum should not be greater then the maximum", nameof(min));
			if (value < min) return min;
			return value > max ? max : value;
		}

		/// <summary>
		/// Executes the assert Is Positive Or Zero operation.
		/// </summary>
		/// <param name="value">The value.</param>
		public static void AssertIsPositiveOrZero(double value)
		{
			if (value < 0.0) throw new ArgumentException("Invalid value, must be a positive number or equal to zero", nameof(value));
		}
	}
}