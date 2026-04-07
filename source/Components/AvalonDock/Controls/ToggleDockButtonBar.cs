/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AvalonDock.Controls
{
	/// <summary>
	/// A vertical/horizontal bar of toggle buttons representing anchorable tool windows.
	/// Used by <see cref="ToggleDockingManager"/> to provide VSCode/Rider-style sidebar buttons.
	/// </summary>
	public class ToggleDockButtonBar : ItemsControl
	{
		/// <summary>Gets/sets which anchor side this button bar represents.</summary>
		public AnchorSide Section
		{
			get => (AnchorSide)GetValue(SectionProperty);
			set => SetValue(SectionProperty, value);
		}

		public static readonly DependencyProperty SectionProperty =
			DependencyProperty.Register(nameof(Section), typeof(AnchorSide), typeof(ToggleDockButtonBar), new PropertyMetadata(AnchorSide.Left));

		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(ToggleDockButtonBar), new PropertyMetadata(Orientation.Vertical));

		/// <summary>Populates this bar with buttons for the given anchorables.</summary>
		public void SetAnchorables(IEnumerable<LayoutAnchorable> anchorables)
		{
			Items.Clear();
			foreach (var anc in anchorables)
			{
				var btn = new ToggleDockButton { Anchorable = anc, Section = Section };
				Items.Add(btn);
			}
		}
	}

	/// <summary>
	/// A toggle button that represents a single anchorable tool window in the sidebar.
	/// Clicking it toggles the anchorable between auto-hidden and docked states.
	/// </summary>
	public class ToggleDockButton : ToggleButton
	{
		static ToggleDockButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleDockButton), new FrameworkPropertyMetadata(typeof(ToggleDockButton)));
		}

		/// <summary>The anchorable model this button controls.</summary>
		public LayoutAnchorable Anchorable
		{
			get => (LayoutAnchorable)GetValue(AnchorableProperty);
			set => SetValue(AnchorableProperty, value);
		}

		public static readonly DependencyProperty AnchorableProperty =
			DependencyProperty.Register(nameof(Anchorable), typeof(LayoutAnchorable), typeof(ToggleDockButton),
				new PropertyMetadata(null, OnAnchorableChanged));

		/// <summary>Which anchor side section this button belongs to.</summary>
		public AnchorSide Section
		{
			get => (AnchorSide)GetValue(SectionProperty);
			set => SetValue(SectionProperty, value);
		}

		public static readonly DependencyProperty SectionProperty =
			DependencyProperty.Register(nameof(Section), typeof(AnchorSide), typeof(ToggleDockButton), new PropertyMetadata(AnchorSide.Left));

		private static void OnAnchorableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var btn = (ToggleDockButton)d;
			if (e.NewValue is LayoutAnchorable anc)
			{
				btn.Content = anc.Title;
				btn.IsChecked = !anc.IsAutoHidden;
			}
		}

		protected override void OnClick()
		{
			base.OnClick();
			if (Anchorable == null) return;

			// Find the ToggleDockingManager to coordinate exclusive activation
			var manager = Anchorable.Root?.Manager as ToggleDockingManager;
			manager?.ToggleAnchorable(Anchorable, Section);
		}
	}
}
