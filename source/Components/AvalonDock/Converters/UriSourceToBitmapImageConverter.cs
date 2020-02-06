/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts a <see cref="Uri"/> object into a corresponding <see cref="Image"/> object
	/// that is expected to be available at the given target of the uri.
	///
	/// Returns null or Binding.DoNothing otherwise.
	/// </summary>
	public class UriSourceToBitmapImageConverter : IValueConverter
	{
		/// <summary>
		/// Converts a <see cref="Uri"/> object into a corresponding <see cref="Image"/> object
		/// that is expected to be available at the given target of the uri.
		///
		/// Returns null or Binding.DoNothing otherwise.
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

			return new Image() { Source = new BitmapImage((Uri)value) };
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
