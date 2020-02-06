/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts an <see cref="AnchorSide"/> value into a floating point value that
	/// indicates whether the content should be rotated by 90.0 degrees or not.
	/// 
	/// Returns Binding.DoNothing if not rotation is required.
	/// </summary>
	[ValueConversion(typeof(AnchorSide), typeof(double))]
	public class AnchorSideToAngleConverter : IValueConverter
	{
		/// <summary>
		/// Converts an <see cref="AnchorSide"/> value into a floating point value that
		/// indicates whether the content should be rotated by 90.0 degrees or not.
		/// 
		/// Returns Binding.DoNothing if not rotation is required.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			AnchorSide side = (AnchorSide)value;
			if (side == AnchorSide.Left || side == AnchorSide.Right)
				return 90.0;

			return Binding.DoNothing;
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
