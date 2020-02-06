/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Data;
using System.Windows;

namespace AvalonDock.Converters
{
	/// <inheritdoc />
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BoolToVisibilityConverter : IValueConverter
	{
		/// <inheritdoc />
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
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (!(value is Visibility)) throw new ArgumentException("Invalid argument type. Expected argument: Visibility.", nameof(value));
			if (targetType != typeof(bool)) throw new ArgumentException("Invalid return type. Expected type: bool", nameof(targetType));
			return (Visibility)value == Visibility.Visible;
		}
	}
}
