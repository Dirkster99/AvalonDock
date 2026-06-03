/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// Conditional to use more aggressive fail-fast behaviors when debugging.
#define DEV_DEBUG

// This file contains general utilities to aid in development.
// It is distinct from unit test Assert classes.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
	using System;
	using System.Diagnostics;
	using System.Threading;

	/// <summary>
	/// Provides helper members for assert.
	/// </summary>
	internal static class Assert
	{
		/// <summary>
		/// Executes the break operation.
		/// </summary>
		private static void _Break()
		{
#if DEV_DEBUG
			Debugger.Break();
#else
            Debug.Assert(false);
#endif
		}

		/// <summary>
		/// Represents the method that handles evaluate Function.
		/// </summary>
		public delegate void EvaluateFunction();

		/// <summary>
		/// Represents the method that handles implication Function.
		/// </summary>
		/// <returns>The delegate result.</returns>
		public delegate bool ImplicationFunction();

		/// <summary>
		/// Executes the evaluate operation.
		/// </summary>
		/// <param name="argument">The argument.</param>
		[Conditional("DEBUG")]
		public static void Evaluate(EvaluateFunction argument)
		{
			IsNotNull(argument);
			argument();
		}

		/// <summary>
		/// Executes the equals operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="expected">The expected.</param>
		/// <param name="actual">The actual.</param>
		[Obsolete("Use Assert.AreEqual instead of Assert.Equals", false)]
		[Conditional("DEBUG")]
		public static void Equals<T>(T expected, T actual)
		{
			AreEqual(expected, actual);
		}

		/// <summary>
		/// Executes the are Equal operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="expected">The expected.</param>
		/// <param name="actual">The actual.</param>
		[Conditional("DEBUG")]
		public static void AreEqual<T>(T expected, T actual)
		{
			if (expected == null)
			{
				// Two nulls are considered equal, regardless of type semantics.
				if (actual != null && !actual.Equals(expected)) _Break();
			}
			else if (!expected.Equals(actual))
			{
				_Break();
			}
		}

		/// <summary>
		/// Executes the are Not Equal operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="notExpected">The not Expected.</param>
		/// <param name="actual">The actual.</param>
		[Conditional("DEBUG")]
		public static void AreNotEqual<T>(T notExpected, T actual)
		{
			if (notExpected == null)
			{
				// Two nulls are considered equal, regardless of type semantics.
				if (actual == null || actual.Equals(notExpected)) _Break();
			}
			else if (notExpected.Equals(actual))
			{
				_Break();
			}
		}

		/// <summary>
		/// Executes the implies operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <param name="result">The result.</param>
		[Conditional("DEBUG")]
		public static void Implies(bool condition, bool result)
		{
			if (condition && !result) _Break();
		}

		/// <summary>
		/// Executes the implies operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <param name="result">The result.</param>
		[Conditional("DEBUG")]
		public static void Implies(bool condition, ImplicationFunction result)
		{
			if (condition && !result()) _Break();
		}

		/// <summary>
		/// Executes the is Neither Null Nor Empty operation.
		/// </summary>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void IsNeitherNullNorEmpty(string value)
		{
			IsFalse(string.IsNullOrEmpty(value));
		}

		/// <summary>
		/// Executes the is Neither Null Nor Whitespace operation.
		/// </summary>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void IsNeitherNullNorWhitespace(string value)
		{
			if (string.IsNullOrEmpty(value)) _Break();
			if (value.Trim().Length == 0) _Break();
		}

		/// <summary>
		/// Executes the is Not Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void IsNotNull<T>(T value)
			where T : class
		{
			if (value == null) _Break();
		}

		/// <summary>
		/// Executes the is Default operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void IsDefault<T>(T value)
			where T : struct
		{
			if (!value.Equals(default(T))) Assert.Fail();
		}

		/// <summary>
		/// Executes the is Not Default operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void IsNotDefault<T>(T value)
			where T : struct
		{
			if (value.Equals(default(T))) Assert.Fail();
		}

		/// <summary>
		/// Executes the is False operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		[Conditional("DEBUG")]
		public static void IsFalse(bool condition)
		{
			if (condition) _Break();
		}

		/// <summary>
		/// Executes the is False operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <param name="message">The message.</param>
		[Conditional("DEBUG")]
		public static void IsFalse(bool condition, string message)
		{
			if (condition) _Break();
		}

		/// <summary>
		/// Executes the is True operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		[Conditional("DEBUG")]
		public static void IsTrue(bool condition)
		{
			if (!condition) _Break();
		}

		/// <summary>
		/// Executes the is True operation.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <param name="message">The message.</param>
		[Conditional("DEBUG")]
		public static void IsTrue(bool condition, string message)
		{
			if (!condition) _Break();
		}

		/// <summary>
		/// Executes the fail operation.
		/// </summary>
		[Conditional("DEBUG")]
		public static void Fail() => _Break();

		/// <summary>
		/// Executes the fail operation.
		/// </summary>
		/// <param name="message">The message.</param>
		[Conditional("DEBUG")]
		public static void Fail(string message) => _Break();

		/// <summary>
		/// Executes the is Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="item">The item.</param>
		[Conditional("DEBUG")]
		public static void IsNull<T>(T item)
			where T : class
		{
			if (item != null) _Break();
		}

		/// <summary>
		/// Executes the bounded Double Inc operation.
		/// </summary>
		/// <param name="lowerBoundInclusive">The lower Bound Inclusive.</param>
		/// <param name="value">The value.</param>
		/// <param name="upperBoundInclusive">The upper Bound Inclusive.</param>
		[Conditional("DEBUG")]
		public static void BoundedDoubleInc(double lowerBoundInclusive, double value, double upperBoundInclusive)
		{
			if (value < lowerBoundInclusive || value > upperBoundInclusive) _Break();
		}

		/// <summary>
		/// Executes the bounded Integer operation.
		/// </summary>
		/// <param name="lowerBoundInclusive">The lower Bound Inclusive.</param>
		/// <param name="value">The value.</param>
		/// <param name="upperBoundExclusive">The upper Bound Exclusive.</param>
		[Conditional("DEBUG")]
		public static void BoundedInteger(int lowerBoundInclusive, int value, int upperBoundExclusive)
		{
			if (value < lowerBoundInclusive || value >= upperBoundExclusive) _Break();
		}

		/// <summary>
		/// Executes the is Apartment State operation.
		/// </summary>
		/// <param name="expectedState">The expected State.</param>
		[Conditional("DEBUG")]
		public static void IsApartmentState(ApartmentState expectedState)
		{
			if (Thread.CurrentThread.GetApartmentState() != expectedState) _Break();
		}

		/// <summary>
		/// Executes the nullable Is Not Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void NullableIsNotNull<T>(T? value)
			where T : struct
		{
			if (value == null) _Break();
		}

		/// <summary>
		/// Executes the nullable Is Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="value">The value.</param>
		[Conditional("DEBUG")]
		public static void NullableIsNull<T>(T? value)
			where T : struct
		{
			if (value != null) _Break();
		}

		/// <summary>
		/// Executes the is Not On Main Thread operation.
		/// </summary>
		[Conditional("DEBUG")]
		public static void IsNotOnMainThread()
		{
			if (System.Windows.Application.Current.Dispatcher.CheckAccess()) _Break();
		}
	}
}