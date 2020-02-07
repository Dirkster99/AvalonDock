/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements the TabItem Header that is displayed when the <see cref="LayoutAnchorablePaneControl"/>
	/// shows more than 1 <see cref="LayoutAnchorableControl"/>. This TabItem is displayed at the bottom
	/// of a <see cref="LayoutAnchorablePaneControl"/>.
	/// </summary>
	/// <seealso cref="Control"/>
	public class LayoutAnchorableTabItem : Control
	{
		#region fields
		private bool _isMouseDown = false;
		private static LayoutAnchorableTabItem _draggingItem = null;
		private static bool _cancelMouseLeave = false;
		#endregion fields

		#region Constructors

		static LayoutAnchorableTabItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableTabItem), new FrameworkPropertyMetadata(typeof(LayoutAnchorableTabItem)));
		}

		#endregion Constructors

		#region Properties

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutAnchorableTabItem),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the <see cref="Model"/> property. This dependency property 
		/// indicates model attached to the anchorable tab item.
		/// </summary>
		public LayoutContent Model
		{
			get => (LayoutContent)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableTabItem)d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			SetLayoutItem(Model?.Root.Manager.GetLayoutItemFromModel(Model));
			//UpdateLogicalParent();
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableTabItem),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the <see cref="LayoutItem"/> property. This dependency property 
		/// indicates the LayoutItem attached to this tag item.
		/// </summary>
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.  
		/// This dependency property indicates the LayoutItem attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#endregion Properties

		#region Internal Methods

		internal static bool IsDraggingItem() => _draggingItem != null;

		internal static LayoutAnchorableTabItem GetDraggingItem() => _draggingItem;

		internal static void ResetDraggingItem() => _draggingItem = null;

		internal static void CancelMouseLeave() => _cancelMouseLeave = true;

		#endregion Internal Methods

		#region Overrides
		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			_isMouseDown = true;
			_draggingItem = this;
		}

		/// <inheritdoc />
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.LeftButton != MouseButtonState.Pressed)
			{
				_isMouseDown = false;
				_draggingItem = null;
			}
			else
				_cancelMouseLeave = false;
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			base.OnMouseLeftButtonUp(e);
			Model.IsActive = true;
		}

		/// <inheritdoc />
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (_isMouseDown && e.LeftButton == MouseButtonState.Pressed)
			{
				// drag the item if the mouse leave is not canceled.
				// Mouse leave should be canceled when selecting a new tab to prevent automatic undock when Panel size is Auto.
				_draggingItem = !_cancelMouseLeave ? this : null;
			}
			_isMouseDown = false;
			_cancelMouseLeave = false;
		}

		/// <inheritdoc />
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			if (_draggingItem == null || _draggingItem == this || e.LeftButton != MouseButtonState.Pressed) return;
			var model = Model;
			var container = model.Parent;
			var containerPane = model.Parent as ILayoutPane;
			if (containerPane is LayoutAnchorablePane layoutAnchorablePane && !layoutAnchorablePane.CanRepositionItems) return;
			if (containerPane.Parent is LayoutAnchorablePaneGroup layoutAnchorablePaneGroup && !layoutAnchorablePaneGroup.CanRepositionItems) return;
			var childrenList = container.Children.ToList();

			// Hotfix to avoid crash caused by a likely threading issue Back in the containerPane.
			var oldIndex = childrenList.IndexOf(_draggingItem.Model);
			var newIndex = childrenList.IndexOf(model);
			if (newIndex < containerPane.ChildrenCount && oldIndex > -1) containerPane.MoveChild(oldIndex, newIndex);
		}

		#endregion Overrides
	}
}
