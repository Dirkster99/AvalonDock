/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock
{
	/// <summary>
	/// Implements a class that provides mathematical helper methods.
	/// </summary>
	internal static class MathHelper
	{
		/// <summary>
		/// Ensures that <paramref name="min"/> is greater <paramref name="max"/> via Exception (if not)
		/// and returns a valid value inside the given bounds.
		/// 
		/// That is, (<paramref name="min"/> or <paramref name="max"/> is returned if
		/// <paramref name="value"/> is out of bounds, <paramref name="value"/> is returned otherwise.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		public static double MinMax(double value, double min, double max)
		{
			if (min > max) throw new ArgumentException("The minimum should not be greater then the maximum", nameof(min));
			if (value < min) return min;
			return value > max ? max : value;
		}

		/// <summary>
		/// Throw an exception if <paramref name="value"/> is smaller than 0.
		/// </summary>
		/// <param name="value"></param>
		public static void AssertIsPositiveOrZero(double value)
		{
			if (value < 0.0) throw new ArgumentException("Invalid value, must be a positive number or equal to zero", nameof(value));
		}
	}
}
