/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements a button that is used to display a <see cref="ContextMenuEx"/>
	/// when the user clicks on it
	/// (see templates for <see cref="LayoutAnchorableFloatingWindowControl"/>,
	/// <see cref="AnchorablePaneTitle"/>, and <see cref="LayoutDocumentPaneControl"/>).
	/// </summary>
	/// <seealso cref="ToggleButton"/>
	public class DropDownButton : ToggleButton
	{
		#region Constructors

		public DropDownButton()
		{
			Unloaded += DropDownButton_Unloaded;
		}

		#endregion Constructors

		#region Properties

		#region DropDownContextMenu

		/// <summary><see cref="DropDownContextMenu"/> dependency property.</summary>
		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register(nameof(DropDownContextMenu), typeof(ContextMenu), typeof(DropDownButton),
				new FrameworkPropertyMetadata(null, OnDropDownContextMenuChanged));

		/// <summary>
		/// Gets or sets the <see cref="DropDownContextMenu"/> property. This dependency property 
		/// indicates drop down menu to show up when user click on an anchorable menu pin.
		/// </summary>
		public ContextMenu DropDownContextMenu
		{
			get => (ContextMenu)GetValue(DropDownContextMenuProperty);
			set => SetValue(DropDownContextMenuProperty, value);
		}

		/// <summary>Handles changes to the <see cref="DropDownContextMenu"/> property.</summary>
		private static void OnDropDownContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((DropDownButton)d).OnDropDownContextMenuChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="DropDownContextMenu"/> property.</summary>
		protected virtual void OnDropDownContextMenuChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue is ContextMenu oldContextMenu && IsChecked.GetValueOrDefault())
				oldContextMenu.Closed -= OnContextMenuClosed;
		}

		#endregion DropDownContextMenu

		#region DropDownContextMenuDataContext

		/// <summary><see cref="DropDownContextMenuDataContext"/> dependency property.</summary>
		public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register(nameof(DropDownContextMenuDataContext), typeof(object), typeof(DropDownButton),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the <see cref="DropDownContextMenuDataContext"/> property. This dependency property 
		/// indicates data context to set for drop down context menu.
		/// </summary>
		public object DropDownContextMenuDataContext
		{
			get => GetValue(DropDownContextMenuDataContextProperty);
			set => SetValue(DropDownContextMenuDataContextProperty, value);
		}

		#endregion DropDownContextMenuDataContext

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override void OnClick()
		{
			if (DropDownContextMenu != null)
			{
				//IsChecked = true;
				DropDownContextMenu.PlacementTarget = this;
				DropDownContextMenu.Placement = PlacementMode.Bottom;
				DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
				DropDownContextMenu.Closed += OnContextMenuClosed;
				DropDownContextMenu.IsOpen = true;
			}
			base.OnClick();
		}

		#endregion Overrides

		#region Private Methods

		private void OnContextMenuClosed(object sender, RoutedEventArgs e)
		{
			//Debug.Assert(IsChecked.GetValueOrDefault());
			var ctxMenu = sender as ContextMenu;
			ctxMenu.Closed -= OnContextMenuClosed;
			IsChecked = false;
		}

		private void DropDownButton_Unloaded(object sender, RoutedEventArgs e)
		{
			// When changing theme, Unloaded event is called, erasing the DropDownContextMenu.
			// Prevent this on theme changes.
			if (IsLoaded) DropDownContextMenu = null;
		}

		#endregion Private Methods
	}
}
