/**************************************************************************\
	Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// This file contains general utilities to aid in development.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
	using System;
	using System.Diagnostics;
	using System.Diagnostics.CodeAnalysis;
	using System.Globalization;
	using System.IO;
	using System.Threading;

	/// <summary>
	/// Provides helper members for verify.
	/// </summary>
	internal static class Verify
	{
		/// <summary>
		/// Executes the is Apartment State operation.
		/// </summary>
		/// <param name="requiredState">The required State.</param>
		/// <param name="message">The message.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsApartmentState(ApartmentState requiredState, string message)
		{
			if (Thread.CurrentThread.GetApartmentState() != requiredState) throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Executes the is Neither Null Nor Empty operation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		[DebuggerStepThrough]
		public static void IsNeitherNullNorEmpty(string value, string name)
		{
			// catch caller errors, mixing up the parameters.  Name should never be empty.
			Assert.IsNeitherNullNorEmpty(name);
			// Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
			const string errorMessage = "The parameter can not be either null or empty.";
			if (null == value) throw new ArgumentNullException(name, errorMessage);
			if (value == string.Empty) throw new ArgumentException(errorMessage, name);
		}

		/// <summary>
		/// Executes the is Neither Null Nor Whitespace operation.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
		[DebuggerStepThrough]
		public static void IsNeitherNullNorWhitespace(string value, string name)
		{
			// catch caller errors, mixing up the parameters.  Name should never be empty.
			Assert.IsNeitherNullNorEmpty(name);

			// Notice that ArgumentNullException and ArgumentException take the parameters in opposite order :P
			const string errorMessage = "The parameter can not be either null or empty or consist only of white space characters.";
			if (value == null) throw new ArgumentNullException(name, errorMessage);
			if (value.Trim() == string.Empty) throw new ArgumentException(errorMessage, name);
		}

		/// <summary>
		/// Executes the is Not Default operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsNotDefault<T>(T obj, string name)
			where T : struct
		{
			if (default(T).Equals(obj)) throw new ArgumentException("The parameter must not be the default value.", name);
		}

		/// <summary>
		/// Executes the is Not Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsNotNull<T>(T obj, string name)
			where T : class
		{
			if (obj == null) throw new ArgumentNullException(name);
		}

		/// <summary>
		/// Executes the is Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsNull<T>(T obj, string name)
			where T : class
		{
			if (obj != null) throw new ArgumentException("The parameter must be null.", name);
		}

		/// <summary>
		/// Executes the property Is Not Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void PropertyIsNotNull<T>(T obj, string name)
			where T : class
		{
			if (obj == null)
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} cannot be null at this time.", name));
		}

		/// <summary>
		/// Executes the property Is Null operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="obj">The obj.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void PropertyIsNull<T>(T obj, string name)
			where T : class
		{
			if (obj != null)
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The property {0} must be null at this time.", name));
		}

		/// <summary>
		/// Executes the is True operation.
		/// </summary>
		/// <param name="statement">The statement.</param>
		/// <param name="name">The name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsTrue(bool statement, string name)
		{
			if (!statement) throw new ArgumentException(string.Empty, name);
		}

		/// <summary>
		/// Executes the is True operation.
		/// </summary>
		/// <param name="statement">The statement.</param>
		/// <param name="name">The name.</param>
		/// <param name="message">The message.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void IsTrue(bool statement, string name, string message)
		{
			if (!statement) throw new ArgumentException(message, name);
		}

		/// <summary>
		/// Executes the are Equal operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="expected">The expected.</param>
		/// <param name="actual">The actual.</param>
		/// <param name="parameterName">The parameter Name.</param>
		/// <param name="message">The message.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void AreEqual<T>(T expected, T actual, string parameterName, string message)
		{
			if (expected == null)
			{
				// Two nulls are considered equal, regardless of type semantics.
				if (actual != null && !actual.Equals(expected)) throw new ArgumentException(message, parameterName);
			}
			else if (!expected.Equals(actual))
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		/// <summary>
		/// Executes the are Not Equal operation.
		/// </summary>
		/// <typeparam name="T">The t type.</typeparam>
		/// <param name="notExpected">The not Expected.</param>
		/// <param name="actual">The actual.</param>
		/// <param name="parameterName">The parameter Name.</param>
		/// <param name="message">The message.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void AreNotEqual<T>(T notExpected, T actual, string parameterName, string message)
		{
			if (notExpected == null)
			{
				// Two nulls are considered equal, regardless of type semantics.
				if (actual == null || actual.Equals(notExpected)) throw new ArgumentException(message, parameterName);
			}
			else if (notExpected.Equals(actual))
			{
				throw new ArgumentException(message, parameterName);
			}
		}

		/// <summary>
		/// Executes the uri Is Absolute operation.
		/// </summary>
		/// <param name="uri">The uri.</param>
		/// <param name="parameterName">The parameter Name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void UriIsAbsolute(Uri uri, string parameterName)
		{
			Verify.IsNotNull(uri, parameterName);
			if (!uri.IsAbsoluteUri) throw new ArgumentException("The URI must be absolute.", parameterName);
		}

		/// <summary>
		/// Executes the bounded Integer operation.
		/// </summary>
		/// <param name="lowerBoundInclusive">The lower Bound Inclusive.</param>
		/// <param name="value">The value.</param>
		/// <param name="upperBoundExclusive">The upper Bound Exclusive.</param>
		/// <param name="parameterName">The parameter Name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void BoundedInteger(int lowerBoundInclusive, int value, int upperBoundExclusive, string parameterName)
		{
			if (value < lowerBoundInclusive || value >= upperBoundExclusive)
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The integer value must be bounded with [{0}, {1})", lowerBoundInclusive, upperBoundExclusive), parameterName);
		}

		/// <summary>
		/// Executes the bounded Double Inc operation.
		/// </summary>
		/// <param name="lowerBoundInclusive">The lower Bound Inclusive.</param>
		/// <param name="value">The value.</param>
		/// <param name="upperBoundInclusive">The upper Bound Inclusive.</param>
		/// <param name="message">The message.</param>
		/// <param name="parameter">The converter parameter.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void BoundedDoubleInc(double lowerBoundInclusive, double value, double upperBoundInclusive, string message, string parameter)
		{
			if (value < lowerBoundInclusive || value > upperBoundInclusive) throw new ArgumentException(message, parameter);
		}

		/// <summary>
		/// Executes the type Supports Interface operation.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="interfaceType">The interface Type.</param>
		/// <param name="parameterName">The parameter Name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void TypeSupportsInterface(Type type, Type interfaceType, string parameterName)
		{
			Assert.IsNeitherNullNorEmpty(parameterName);
			Verify.IsNotNull(type, nameof(type));
			Verify.IsNotNull(interfaceType, nameof(interfaceType));
			if (type.GetInterface(interfaceType.Name) == null)
				throw new ArgumentException("The type of this parameter does not support a required interface", parameterName);
		}

		/// <summary>
		/// Executes the file Exists operation.
		/// </summary>
		/// <param name="filePath">The file Path.</param>
		/// <param name="parameterName">The parameter Name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		public static void FileExists(string filePath, string parameterName)
		{
			Verify.IsNeitherNullNorEmpty(filePath, parameterName);
			if (!File.Exists(filePath))
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "No file exists at \"{0}\"", filePath), parameterName);
		}

		/// <summary>
		/// Executes the implements Interface operation.
		/// </summary>
		/// <param name="parameter">The converter parameter.</param>
		/// <param name="interfaceType">The interface Type.</param>
		/// <param name="parameterName">The parameter Name.</param>
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[DebuggerStepThrough]
		internal static void ImplementsInterface(object parameter, Type interfaceType, string parameterName)
		{
			Assert.IsNotNull(parameter);
			Assert.IsNotNull(interfaceType);
			Assert.IsTrue(interfaceType.IsInterface);

			var isImplemented = false;
			foreach (var iFaceType in parameter.GetType().GetInterfaces())
			{
				if (iFaceType != interfaceType) continue;
				isImplemented = true;
				break;
			}

			if (!isImplemented)
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The parameter must implement interface {0}.", interfaceType.ToString()), parameterName);
		}
	}
}