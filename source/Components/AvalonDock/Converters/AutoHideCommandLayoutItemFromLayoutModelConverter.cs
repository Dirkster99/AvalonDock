﻿/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Controls;
using AvalonDock.Layout;
using System;
using System.Windows.Data;
using System.Windows.Markup;

namespace AvalonDock.Converters
{
	/// <summary>
	/// Converts a <see cref="LayoutContent"/> into a <see cref="LayoutAnchorableItem.AutoHideCommand"/>
	/// and ensures that other essential properties (Root, Root.Manager) are available.
	///
	/// Returns null or Binding.DoNothing otherwise.
	/// </summary>
	public class AutoHideCommandLayoutItemFromLayoutModelConverter : MarkupExtension, IValueConverter
	{
		/// <summary>
		/// Converts a <see cref="LayoutContent"/> into a <see cref="LayoutAnchorableItem.AutoHideCommand"/>
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
			if (value as LayoutContent == null)
				return null;

			if ((value as LayoutContent).Root == null)
				return null;

			if ((value as LayoutContent).Root.Manager == null)
				return null;

			if (!((value as LayoutContent).Root.Manager.GetLayoutItemFromModel(value as LayoutContent) is LayoutAnchorableItem layoutItemModel))
				return Binding.DoNothing;

			return layoutItemModel.AutoHideCommand;
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

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			return ConverterCreater.Get<AutoHideCommandLayoutItemFromLayoutModelConverter>();
		}
	}
}