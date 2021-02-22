/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements the title display for various items: <see cref="LayoutAnchorablePaneControl"/>,
	/// <see cref="LayoutDocumentTabItem"/>, <see cref="LayoutAnchorableTabItem"/>,
	/// <see cref="LayoutDocumentFloatingWindowControl"/>, and <see cref="LayoutAnchorableFloatingWindowControl"/>.
	///
	/// The content is usually displayed via ContentPresenter binding in the theme definition.
	/// </summary>
	/// <seealso cref="ContentControl"/>
	public class DropDownControlArea : ContentControl
	{
		#region ctors
		/// <summary>
		/// Static class constructor
		/// </summary>
		static DropDownControlArea()
		{
			// Fixing issue with Keyboard up/down in textbox in floating anchorable focusing DropDownControlArea
			// https://github.com/Dirkster99/AvalonDock/issues/225
			FocusableProperty.OverrideMetadata(typeof(DropDownControlArea), new FrameworkPropertyMetadata(false));
		}
		#endregion ctors

		#region Properties

		#region DropDownContextMenu

		/// <summary><see cref="DropDownContextMenu"/> dependency property.</summary>
		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register(nameof(DropDownContextMenu), typeof(ContextMenu), typeof(DropDownControlArea),
				new FrameworkPropertyMetadata(null));

		/// <summary>Gets/sets the drop down menu to show up when user click on an anchorable menu pin.</summary>
		[Bindable(true), Description("Gets/sets the drop down menu to show up when user click on an anchorable menu pin."), Category("Menu")]
		public ContextMenu DropDownContextMenu
		{
			get => (ContextMenu)GetValue(DropDownContextMenuProperty);
			set => SetValue(DropDownContextMenuProperty, value);
		}

		#endregion DropDownContextMenu

		#region DropDownContextMenuDataContext

		/// <summary><see cref="DropDownContextMenuDataContext"/> dependency property. </summary>
		public static readonly DependencyProperty DropDownContextMenuDataContextProperty = DependencyProperty.Register(nameof(DropDownContextMenuDataContext), typeof(object), typeof(DropDownControlArea),
				new FrameworkPropertyMetadata(null));

		/// <summary>Gets/sets the DataContext to set for the DropDownContext menu property.</summary>
		[Bindable(true), Description("Gets/sets the DataContext to set for the DropDownContext menu property."), Category("Menu")]
		public object DropDownContextMenuDataContext
		{
			get => (object)GetValue(DropDownContextMenuDataContextProperty);
			set => SetValue(DropDownContextMenuDataContextProperty, value);
		}

		#endregion DropDownContextMenuDataContext

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override void OnPreviewMouseRightButtonUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnPreviewMouseRightButtonUp(e);
			if (e.Handled) return;
			if (DropDownContextMenu == null) return;
			DropDownContextMenu.PlacementTarget = null;
			DropDownContextMenu.Placement = PlacementMode.MousePoint;
			DropDownContextMenu.DataContext = DropDownContextMenuDataContext;
			DropDownContextMenu.IsOpen = true;
			// e.Handled = true;
		}

		//protected override System.Windows.Media.HitTestResult HitTestCore(System.Windows.Media.PointHitTestParameters hitTestParameters)
		//{
		//    var hitResult = base.HitTestCore(hitTestParameters);
		//    if (hitResult == null)
		//        return new PointHitTestResult(this, hitTestParameters.HitPoint);

		//    return hitResult;
		//}

		#endregion Overrides
	}
}