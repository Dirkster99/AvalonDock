/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts a <see cref="LayoutContent"/> into a <see cref="LayoutItem"/>
	/// and ensures that other essential properties (Root, Root.Manager) are available.
	///
	/// Returns null or Binding.DoNothing otherwise.
	/// </summary>
	public class OverlayWindowToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Converts a <see cref="OverlayWindow"/> into a <see cref="Visibility"/>.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isHostedInFloatingWindow = value is OverlayWindow overlayWindow && overlayWindow.IsHostedInFloatingWindow;
			if (parameter == null || !bool.TryParse(parameter.ToString(), out bool isLarge))
			{
				isLarge = false;
			}

			return isHostedInFloatingWindow && isLarge ? Visibility.Hidden : Visibility.Visible;
		}

		/// <summary>
		/// Method is not implemented and will raise <see cref="System.NotSupportedException"/> when called.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns><see cref="System.NotSupportedException"/></returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
	}
}
