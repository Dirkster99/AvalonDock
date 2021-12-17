/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>
	/// Implements the control for the TabItem Header of a <see cref="LayoutDocumentPaneControl"/>.
	/// The Document's TabItem Header displays the title of a document and can be used to drag a
	/// document from its docking position.
	/// </summary>
	/// <seealso cref="Control"/>
	public class LayoutDocumentTabItem : ContentControl
	{
		#region fields

		private List<Rect> _otherTabsScreenArea = null;
		private List<TabItem> _otherTabs = null;
		private Rect _parentDocumentTabPanelScreenArea;
		private DocumentPaneTabPanel _parentDocumentTabPanel;
		private bool _isMouseDown = false;
		private Point _mouseDownPoint;
		private bool _allowDrag = false;

		#endregion fields

		#region Contructors

		/// <summary>Static class constructor to register WPF style keys.</summary>
		static LayoutDocumentTabItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentTabItem), new FrameworkPropertyMetadata(typeof(LayoutDocumentTabItem)));
		}

		#endregion Contructors

		#region Properties

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutDocumentTabItem),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>Gets/sets the layout content model attached to the tab item.</summary>
		[Bindable(true), Description("Gets wether this floating window is being dragged."), Category("Other")]
		public LayoutContent Model
		{
			get => (LayoutContent)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentTabItem)d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			var layoutItem = (Model?.Root?.Manager)?.GetLayoutItemFromModel(Model);
			SetLayoutItem(layoutItem);
			if (layoutItem != null)
				Model.TabItem = this;
			//UpdateLogicalParent();
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutDocumentTabItem),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>Gets the LayoutItem attached to this tag item.</summary>
		[Bindable(true), Description("Gets the LayoutItem attached to this tag item."), Category("Other")]
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		#endregion LayoutItem

		#endregion Properties

		#region Overrides

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.
		/// This dependency property indicates the LayoutItem attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			_allowDrag = false;
			Model.IsActive = true;
			if (Model is LayoutDocument layoutDocument && !layoutDocument.CanMove) return;
			if (e.ClickCount != 1) return;
			_mouseDownPoint = e.GetPosition(this);
			_isMouseDown = true;
		}

		/// <inheritdoc />
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			_isMouseDown = Mouse.LeftButton == MouseButtonState.Pressed && _isMouseDown;
			if (_isMouseDown)
			{
				var ptMouseMove = e.GetPosition(this);
				CaptureMouse();
				if (Math.Abs(ptMouseMove.X - _mouseDownPoint.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(ptMouseMove.Y - _mouseDownPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					UpdateDragDetails();
					_isMouseDown = false;
					_allowDrag = true;
				}
			}
			if (!IsMouseCaptured || !_allowDrag) return;
			var mousePosInScreenCoord = this.PointToScreenDPI(e.GetPosition(this));
			if (!_parentDocumentTabPanelScreenArea.Contains(mousePosInScreenCoord))
				StartDraggingFloatingWindowForContent();
			else
			{
				var indexOfTabItemWithMouseOver = _otherTabsScreenArea.FindIndex(r => r.Contains(mousePosInScreenCoord));
				if (indexOfTabItemWithMouseOver < 0) return;
				var targetModel = _otherTabs[indexOfTabItemWithMouseOver].Content as LayoutContent;
				var container = Model.Parent as ILayoutContainer;
				var containerPane = Model.Parent as ILayoutPane;

				if (containerPane is LayoutDocumentPane layoutDocumentPane && !layoutDocumentPane.CanRepositionItems) return;
				if (containerPane.Parent is LayoutDocumentPaneGroup layoutDocumentPaneGroup && !layoutDocumentPaneGroup.CanRepositionItems) return;

				var childrenList = container.Children.ToList();
				containerPane.MoveChild(childrenList.IndexOf(Model), childrenList.IndexOf(targetModel));
				Model.IsActive = true;
				_parentDocumentTabPanel.UpdateLayout();
				UpdateDragDetails();
			}
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			_allowDrag = false;
			if (IsMouseCaptured) ReleaseMouseCapture();
			base.OnMouseLeftButtonUp(e);
		}

		/// <inheritdoc />
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);
			_isMouseDown = false;
		}

		/// <inheritdoc />
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);
			_isMouseDown = false;
		}

		/// <inheritdoc />
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (LayoutItem != null && e.ChangedButton == MouseButton.Middle && LayoutItem.CloseCommand.CanExecute(null))
			{
				LayoutItem.CloseCommand.Execute(null);
			}
			
			base.OnMouseDown(e);
		}

		#endregion Overrides

		#region Private Methods

		private void UpdateDragDetails()
		{
			_parentDocumentTabPanel = this.FindLogicalAncestor<DocumentPaneTabPanel>();
			_parentDocumentTabPanelScreenArea = _parentDocumentTabPanel.GetScreenArea();
			_otherTabs = _parentDocumentTabPanel.Children.Cast<TabItem>().Where(ch => ch.Visibility != Visibility.Collapsed).ToList();
			var currentTabScreenArea = this.FindLogicalAncestor<TabItem>().GetScreenArea();
			_otherTabsScreenArea = _otherTabs.Select(ti =>
			{
				var screenArea = ti.GetScreenArea();
				return new Rect(screenArea.Left, screenArea.Top, currentTabScreenArea.Width, screenArea.Height);
			}).ToList();
		}

		/// <summary>
		/// Is invoked when the user started to drag this control and its content
		/// should be contained in a <see cref="LayoutFloatingWindowControl"/> to allow
		/// dragging out of the currently docked position.
		/// </summary>
		private void StartDraggingFloatingWindowForContent()
		{
			ReleaseMouseCapture();
			// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
			//if (Model is LayoutAnchorable layoutAnchorable) layoutAnchorable.ResetCanCloseInternal();
			var manager = Model.Root.Manager;
			manager.StartDraggingFloatingWindowForContent(Model);
		}

		#endregion Private Methods
	}
}
