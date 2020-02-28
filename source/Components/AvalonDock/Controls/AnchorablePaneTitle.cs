/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// This control defines the Title area of a <see cref="LayoutAnchorableControl"/>.
	/// It is used to show a title bar with docking window buttons to let users interact
	/// with a <see cref="LayoutAnchorable"/> via drop down menu click or drag & drop.
	/// </summary>
	public class AnchorablePaneTitle : Control
	{
		#region fields
		private bool _isMouseDown = false;
		#endregion fields

		#region Constructors
		/// <summary>
		/// Static class constructor
		/// </summary>
		static AnchorablePaneTitle()
		{
			IsHitTestVisibleProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(true));
			FocusableProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(false));
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(AnchorablePaneTitle)));
		}

		#endregion Constructors

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata(null, _OnModelChanged));

		/// <summary>Gets or sets the <see cref="Model"/> property.  This dependency property indicates model attached to this view.</summary>
		public LayoutAnchorable Model
		{
			get => (LayoutAnchorable)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		private static void _OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((AnchorablePaneTitle)sender).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (Model != null)
				SetLayoutItem(Model.Root.Manager.GetLayoutItemFromModel(Model));
			else
				SetLayoutItem(null);
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata((LayoutItem)null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>Gets the <see cref="LayoutItem"/> property. This dependency property indicates the LayoutItem attached to this tag item.</summary>
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.
		/// This dependency property indicates the <see cref="AvalonDock.Controls.LayoutItem"/> attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#region Overrides

		/// <inheritdoc />
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed) _isMouseDown = false;
			base.OnMouseMove(e);
		}

		/// <inheritdoc />
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			if (_isMouseDown && e.LeftButton == MouseButtonState.Pressed)
			{
				var pane = this.FindVisualAncestor<LayoutAnchorablePaneControl>();
				if (pane != null)
				{
					var paneModel = pane.Model as LayoutAnchorablePane;
					var manager = paneModel.Root.Manager;
					manager.StartDraggingFloatingWindowForPane(paneModel);
				}
				else
				{
					// Start dragging a LayoutAnchorable control that docked/reduced into a SidePanel and
					// 1) made visible by clicking on to its name in AutoHide mode
					// 2) user drags the top title bar of the LayoutAnchorable control to drag it out of its current docking position
					Model?.Root?.Manager?.StartDraggingFloatingWindowForContent(Model);
				}
			}
			_isMouseDown = false;
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (e.Handled) return;
			var attachFloatingWindow = false;
			var parentFloatingWindow = Model.FindParent<LayoutAnchorableFloatingWindow>();
			if (parentFloatingWindow != null) attachFloatingWindow = parentFloatingWindow.Descendents().OfType<LayoutAnchorablePane>().Count() == 1;

			if (attachFloatingWindow)
			{
				//the pane is hosted inside a floating window that contains only an anchorable pane so drag the floating window itself
				var floatingWndControl = Model.Root.Manager.FloatingWindows.Single(fwc => fwc.Model == parentFloatingWindow);
				floatingWndControl.AttachDrag(false);
			}
			else
				_isMouseDown = true;//normal drag
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			base.OnMouseLeftButtonUp(e);
			if (Model != null) Model.IsActive = true;//FocusElementManager.SetFocusOnLastElement(Model);
		}

		#endregion Overrides
	}
}
