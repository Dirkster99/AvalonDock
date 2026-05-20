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
	/// Represents the toggle Anchorable Pane Title.
	/// </summary>
	public class ToggleAnchorablePaneTitle : AnchorablePaneTitle
	{
		/// <summary>
		/// Initializes static members of the <see cref="ToggleAnchorablePaneTitle"/> class.
		/// </summary>
		static ToggleAnchorablePaneTitle()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleAnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(ToggleAnchorablePaneTitle)));
		}

		/// <summary>
		/// <see cref="ShowMinimizeButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowMinimizeButtonProperty =
			DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a value indicating whether show Minimize Button.
		/// </summary>
		public bool ShowMinimizeButton
		{
			get => (bool)GetValue(ShowMinimizeButtonProperty);
			set => SetValue(ShowMinimizeButtonProperty, value);
		}

		/// <summary>
		/// <see cref="ShowOptionsButton"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ShowOptionsButtonProperty =
			DependencyProperty.Register(nameof(ShowOptionsButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets a value indicating whether show Options Button.
		/// </summary>
		public bool ShowOptionsButton
		{
			get => (bool)GetValue(ShowOptionsButtonProperty);
			set => SetValue(ShowOptionsButtonProperty, value);
		}
	}
}