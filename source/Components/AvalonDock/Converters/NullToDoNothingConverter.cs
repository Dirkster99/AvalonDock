/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Data;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts any null value into Binding.DoNothing and returns the bound value, otherwise.
	/// </summary>
	public class NullToDoNothingConverter : IValueConverter
	{
		/// <summary>
		/// Converts any null value into Binding.DoNothing and returns the bound value, otherwise.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return Binding.DoNothing;

			return value;
		}

		/// <summary>
		/// Method is not implemented and will raise <see cref="System.NotImplementedException"/> when called.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns><see cref="System.NotImplementedException"/></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
