/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements a control that is displayed when a <see cref="LayoutAnchorableControl"/>
	/// is in AutoHide mode (which can be applied via context menu drop entry or click on the Pin symbol.
	/// </summary>
	public class LayoutAnchorControl : Control, ILayoutControl
	{
		private LayoutAnchorable _model;
		private DispatcherTimer _openUpTimer = null;

		static LayoutAnchorControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorControl)));
			Control.IsHitTestVisibleProperty.AddOwner(typeof(LayoutAnchorControl), new FrameworkPropertyMetadata(true));
		}

		internal LayoutAnchorControl(LayoutAnchorable model)
		{
			_model = model;
			_model.IsActiveChanged += new EventHandler(_model_IsActiveChanged);
			_model.IsSelectedChanged += new EventHandler(_model_IsSelectedChanged);

			SetSide(_model.FindParent<LayoutAnchorSide>().Side);
		}

		public ILayoutElement Model
		{
			get
			{
				return _model;
			}
		}

		/// <summary>
		/// Side Read-Only Dependency Property
		/// </summary>
		private static readonly DependencyPropertyKey SidePropertyKey = DependencyProperty.RegisterReadOnly("Side", typeof(AnchorSide), typeof(LayoutAnchorControl),
				new FrameworkPropertyMetadata((AnchorSide)AnchorSide.Left));

		public static readonly DependencyProperty SideProperty = SidePropertyKey.DependencyProperty;

		/// <summary>Gets the anchor side of the control.</summary>
		[Bindable(true)]
		[Description("Gets the anchor side of the control.")]
		[Category("Anchor")]
		public AnchorSide Side
		{
			get
			{
				return (AnchorSide)GetValue(SideProperty);
			}
		}

		/// <summary>
		/// Provides a secure method for setting the Side property.
		/// This dependency property indicates the anchor side of the control.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetSide(AnchorSide value)
		{
			SetValue(SidePropertyKey, value);
		}

		// protected override void OnVisualParentChanged(DependencyObject oldParent)
		// {
		//    base.OnVisualParentChanged(oldParent);

		// var contentModel = _model;

		// if (oldParent != null && contentModel != null && contentModel.Content is UIElement)
		//    {
		//        var oldParentPaneControl = oldParent.FindVisualAncestor<LayoutAnchorablePaneControl>();
		//        if (oldParentPaneControl != null)
		//        {
		//            ((ILogicalChildrenContainer)oldParentPaneControl).InternalRemoveLogicalChild(contentModel.Content);
		//        }
		//    }

		// if (contentModel.Content != null && contentModel.Content is UIElement)
		//    {
		//        var oldLogicalParentPaneControl = LogicalTreeHelper.GetParent(contentModel.Content as UIElement)
		//            as ILogicalChildrenContainer;
		//        if (oldLogicalParentPaneControl != null)
		//            oldLogicalParentPaneControl.InternalRemoveLogicalChild(contentModel.Content);
		//    }

		// if (contentModel != null && contentModel.Content != null && contentModel.Root != null && contentModel.Content is UIElement)
		//    {
		//        ((ILogicalChildrenContainer)contentModel.Root.Manager).InternalAddLogicalChild(contentModel.Content);
		//    }
		// }
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (!e.Handled)
			{
				_model.Root.Manager.ShowAutoHideWindow(this);
				_model.IsActive = true;
			}
		}

		/// <summary>
		/// Handles double-click to toggle auto-hide (dock/pin the anchorable).
		/// </summary>
		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);

			if (!e.Handled && e.ChangedButton == MouseButton.Left
				&& _model.Root.Manager.AllowAnchorDoubleClickDock)
			{
				_model.Root.Manager.ExecuteAutoHideCommand(_model);
				e.Handled = true;
			}
		}

		/// <summary>
		/// Handles right-click to show the anchorable context menu (Float, Dock, Auto Hide, etc.).
		/// </summary>
		protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonUp(e);

			if (!e.Handled)
			{
				var manager = _model.Root?.Manager;
				if (manager == null || !manager.AllowAnchorRightClickContextMenu) return;

				var layoutItem = manager.GetLayoutItemFromModel(_model);
				var contextMenu = manager.AnchorableContextMenu;
				if (contextMenu == null || layoutItem == null) return;

				contextMenu.PlacementTarget = this;
				contextMenu.Placement = PlacementMode.MousePoint;
				contextMenu.DataContext = layoutItem;
				contextMenu.IsOpen = true;
				e.Handled = true;
			}
		}

		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			// If the model wants to auto-show itself on hover then initiate the show action
			if (!e.Handled && _model.CanShowOnHover)
			{
				_openUpTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
				_openUpTimer.Interval = TimeSpan.FromMilliseconds(400);
				_openUpTimer.Tick += new EventHandler(_openUpTimer_Tick);
				_openUpTimer.Start();
			}
		}

		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			if (_openUpTimer != null)
			{
				_openUpTimer.Tick -= new EventHandler(_openUpTimer_Tick);
				_openUpTimer.Stop();
				_openUpTimer = null;
			}

			base.OnMouseLeave(e);
		}

		private void _model_IsSelectedChanged(object sender, EventArgs e)
		{
			if (!_model.IsAutoHidden)
			{
				_model.IsSelectedChanged -= new EventHandler(_model_IsSelectedChanged);
			}
			else if (_model.IsSelected)
			{
				_model.Root.Manager.ShowAutoHideWindow(this);
				_model.IsSelected = false;
			}
		}

		private void _model_IsActiveChanged(object sender, EventArgs e)
		{
			if (!_model.IsAutoHidden)
				_model.IsActiveChanged -= new EventHandler(_model_IsActiveChanged);
			else if (_model.IsActive)
				_model.Root.Manager.ShowAutoHideWindow(this);
		}

		private void _openUpTimer_Tick(object sender, EventArgs e)
		{
			_openUpTimer.Tick -= new EventHandler(_openUpTimer_Tick);
			_openUpTimer.Stop();
			_openUpTimer = null;
			_model.Root.Manager.ShowAutoHideWindow(this);
		}
	}
}