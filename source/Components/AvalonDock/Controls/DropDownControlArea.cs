/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

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
	/// <seealso cref="UserControl"/>
	public class DropDownControlArea : UserControl
	{
		#region Properties

		#region DropDownContextMenu

		/// <summary><see cref="DropDownContextMenu"/> dependency property.</summary>
		public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register(nameof(DropDownContextMenu), typeof(ContextMenu), typeof(DropDownControlArea),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets or sets the <see cref="DropDownContextMenu"/> property. This dependency property 
		/// indicates context menu to show when a right click is detected over the area occupied by the control.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the <see cref="DropDownContextMenuDataContext"/> property. This dependency property 
		/// indicates data context to attach when context menu is shown.
		/// </summary>
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
