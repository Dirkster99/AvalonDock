﻿/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace AvalonDock.Converters
{
	/// <inheritdoc />
	/// <summary>
	/// Converts a bool value into a <see cref="Visibility"/> value and back.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityConverter : MarkupExtension, IValueConverter
	{
		/// <inheritdoc />
		/// <summary>
		/// Converts a bool value into a <see cref="Visibility"/> value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			switch (value)
			{
				case bool val when targetType == typeof(Visibility):
					if (val) return Visibility.Visible;
					return parameter is Visibility ? parameter : Visibility.Collapsed;

				case null when parameter is Visibility:
					return parameter;

				case null:
					return Visibility.Collapsed;

				default:
					return Visibility.Visible;
					///throw new ArgumentException("Invalid argument/return type. Expected argument: bool and return type: Visibility");
			}
		}

		/// <inheritdoc />
		/// <summary>
		/// Converts a <see cref="Visibility"/> value into a bool value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is Visibility)) throw new ArgumentException("Invalid argument type. Expected argument: Visibility.", nameof(value));
			if (targetType != typeof(bool)) throw new ArgumentException("Invalid return type. Expected type: bool", nameof(targetType));
			return (Visibility)value == Visibility.Visible;
		}

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<BoolToVisibilityConverter>();
		}
	}
}