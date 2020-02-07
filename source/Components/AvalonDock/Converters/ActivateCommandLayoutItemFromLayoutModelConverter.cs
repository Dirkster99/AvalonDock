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
	/// Converts a <see cref="LayoutContent"/> into an  <see cref="LayoutItem.ActivateCommand"/>
	/// and ensures that other essential properties (Root, Root.Manager) are available.
	///
	/// Returns null or Binding.DoNothing otherwise.
	/// </summary>
	public class ActivateCommandLayoutItemFromLayoutModelConverter : IValueConverter
	{
		/// <summary>
		/// Converts a <see cref="LayoutContent"/> into an  <see cref="LayoutItem.ActivateCommand"/>
		/// and ensures that other essential properties (Root, Root.Manager) are available.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			//when this converter is called layout could be constructing so many properties here are potentially not valid
			var layoutModel = value as LayoutContent;
			if (layoutModel == null)
				return null;

			if (layoutModel.Root == null)
				return null;

			if (layoutModel.Root.Manager == null)
				return null;

			var layoutItemModel = layoutModel.Root.Manager.GetLayoutItemFromModel(layoutModel);
			if (layoutItemModel == null)
				return Binding.DoNothing;

			return layoutItemModel.ActivateCommand;
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
