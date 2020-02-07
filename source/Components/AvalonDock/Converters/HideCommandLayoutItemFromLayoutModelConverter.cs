/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Data;
using AvalonDock.Layout;
using AvalonDock.Controls;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts a <see cref="LayoutContent"/> into a <see cref="LayoutAnchorableItem.HideCommand"/>
	/// and ensures that other essential properties (Root, Root.Manager) are available.
	///
	/// Returns null or Binding.DoNothing otherwise.
	/// </summary>
	public class HideCommandLayoutItemFromLayoutModelConverter : IValueConverter
	{
		/// <summary>
		/// Converts a <see cref="LayoutContent"/> into a <see cref="LayoutAnchorableItem.HideCommand"/>
		/// and ensures that other essential properties (Root, Root.Manager) are available.
		///
		/// Returns null or Binding.DoNothing otherwise.
		/// </summary>
		/// <param name="values">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>A converted value.</returns>
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

			var layoutItemModel = layoutModel.Root.Manager.GetLayoutItemFromModel(layoutModel) as LayoutAnchorableItem;
			if (layoutItemModel == null)
				return Binding.DoNothing;

			return layoutItemModel.HideCommand;
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
