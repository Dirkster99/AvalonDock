/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Provides attached properties for configuring toggle dock button appearance
	/// on <see cref="Layout.LayoutAnchorable"/> elements.
	/// <para>
	/// These properties allow developers to declaratively set icons and tooltips
	/// in XAML without modifying the serializable layout model.
	/// </para>
	/// <example>
	/// <code>
	/// &lt;avalonDock:LayoutAnchorable Title="ToolBox"
	///     avalonDock:ToggleDock.Icon="{StaticResource WrenchIcon}"
	///     avalonDock:ToggleDock.ToolTip="Toggle ToolBox panel" /&gt;
	/// </code>
	/// </example>
	/// </summary>
	public static class ToggleDock
	{
		/// <summary>
		/// Gets or sets the icon content displayed on the toggle dock button.
		/// Accepts any object: <see cref="System.Windows.Media.ImageSource"/>,
		/// <see cref="UIElement"/> (e.g. Path, Viewbox), or a <see cref="DataTemplate"/>.
		/// </summary>
		public static readonly DependencyProperty IconProperty =
			DependencyProperty.RegisterAttached(
				"Icon",
				typeof(object),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		public static object GetIcon(DependencyObject element) => element.GetValue(IconProperty);

		public static void SetIcon(DependencyObject element, object value) => element.SetValue(IconProperty, value);

		/// <summary>
		/// Gets or sets a custom tooltip for the toggle dock button.
		/// When not set, the anchorable's <see cref="Layout.LayoutAnchorable.Title"/> is used.
		/// </summary>
		public static readonly DependencyProperty ToolTipProperty =
			DependencyProperty.RegisterAttached(
				"ToolTip",
				typeof(object),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		public static object GetToolTip(DependencyObject element) => element.GetValue(ToolTipProperty);

		public static void SetToolTip(DependencyObject element, object value) => element.SetValue(ToolTipProperty, value);

		/// <summary>
		/// Gets or sets a <see cref="DataTemplate"/> used to render the icon.
		/// When set, this takes precedence over <see cref="IconProperty"/> for rendering.
		/// The <see cref="IconProperty"/> value is passed as the DataContext.
		/// </summary>
		public static readonly DependencyProperty IconTemplateProperty =
			DependencyProperty.RegisterAttached(
				"IconTemplate",
				typeof(DataTemplate),
				typeof(ToggleDock),
				new FrameworkPropertyMetadata(null));

		public static DataTemplate GetIconTemplate(DependencyObject element) => (DataTemplate)element.GetValue(IconTemplateProperty);

		public static void SetIconTemplate(DependencyObject element, DataTemplate value) => element.SetValue(IconTemplateProperty, value);
	}
}