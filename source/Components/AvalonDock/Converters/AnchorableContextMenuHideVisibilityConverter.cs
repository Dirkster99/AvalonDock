/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts a binding of 2 values:
	/// 1) <see cref="Visibility"/> and
	/// 2) bool
	/// into a <see cref="Visibility"/> value.
	///
	/// The actual <see cref="Visibility"/> value returned is 1) if 2) is true,
	/// <see cref="Visibility.Collapsed"/>  is returned otherwise. 
	/// </summary>
	public class AnchorableContextMenuHideVisibilityConverter : IMultiValueConverter
	{
		/// <summary>
		/// Converts a binding of 2 values:
		/// 1) <see cref="Visibility"/> and
		/// 2) bool
		/// into a <see cref="Visibility"/> value.
		///
		/// The actual <see cref="Visibility"/> value returned is 1) if 2) is true,
		/// <see cref="Visibility.Collapsed"/>  is returned otherwise. 
		/// </summary>
		/// <param name="values">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if ((values.Count() == 2)
			  && (values[0] != DependencyProperty.UnsetValue)
			  && (values[1] != DependencyProperty.UnsetValue)
			  && (values[1] is bool))
			{
				var canClose = (bool)values[1];

				return canClose ? Visibility.Collapsed : values[0];
			}
			else
			{
				return values[0];
			}
		}

		/// <summary>
		/// Method is not implemented and will raise <see cref="System.NotImplementedException"/> when called.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="targetTypes"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns><see cref="System.NotImplementedException"/></returns>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
