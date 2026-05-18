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
	/// An enhanced pane title bar for use with <see cref="ToggleDockingManager"/>.
	/// Provides a configurable header with Minimize and Options (three-dot) buttons.
	/// Users can override the entire look via <see cref="ToggleDockingManager.AnchorableHeaderTemplate"/>.
	/// </summary>
	public class ToggleAnchorablePaneTitle : AnchorablePaneTitle
	{
		static ToggleAnchorablePaneTitle()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleAnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(ToggleAnchorablePaneTitle)));
		}

		/// <summary><see cref="ShowMinimizeButton"/> dependency property.</summary>
		public static readonly DependencyProperty ShowMinimizeButtonProperty =
			DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets whether the Minimize button is shown. Default is true.
		/// </summary>
		public bool ShowMinimizeButton
		{
			get => (bool)GetValue(ShowMinimizeButtonProperty);
			set => SetValue(ShowMinimizeButtonProperty, value);
		}

		/// <summary><see cref="ShowOptionsButton"/> dependency property.</summary>
		public static readonly DependencyProperty ShowOptionsButtonProperty =
			DependencyProperty.Register(nameof(ShowOptionsButton), typeof(bool), typeof(ToggleAnchorablePaneTitle),
				new PropertyMetadata(true));

		/// <summary>
		/// Gets or sets whether the Options (three-dot) button is shown. Default is true.
		/// </summary>
		public bool ShowOptionsButton
		{
			get => (bool)GetValue(ShowOptionsButtonProperty);
			set => SetValue(ShowOptionsButtonProperty, value);
		}
	}
}