using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Document Tab Item.
	/// </summary>
	public class LayoutDocumentTabItem : ContentControl
	{
		private List<Rect> _otherTabsScreenArea = null;
		private List<TabItem> _otherTabs = null;
		private Rect _parentDocumentTabPanelScreenArea;
		private DocumentPaneTabPanel _parentDocumentTabPanel;
		private bool _isMouseDown = false;
		private Point _mouseDownPoint;
		private bool _allowDrag = false;

		/// <summary>
		/// Initializes static members of the <see cref="LayoutDocumentTabItem"/> class.
		/// </summary>
		static LayoutDocumentTabItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentTabItem), new FrameworkPropertyMetadata(typeof(LayoutDocumentTabItem)));
		}

		/// <summary>
		/// <see cref="Model"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutDocumentTabItem),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		[Bindable(true)]
		[Description("Gets wether this floating window is being dragged.")]
		[Category("Other")]
		public LayoutContent Model
		{
			get => (LayoutContent)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentTabItem)d).OnModelChanged(e);

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			var layoutItem = (Model?.Root?.Manager)?.GetLayoutItemFromModel(Model);
			SetLayoutItem(layoutItem);
			if (layoutItem != null)
				Model.TabItem = this;
			// UpdateLogicalParent();
		}

		/// <summary>
		/// The layout Item Property Key field.
		/// </summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutDocumentTabItem),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// <see cref="LayoutItem"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the layout Item.
		/// </summary>
		[Bindable(true)]
		[Description("Gets the LayoutItem attached to this tag item.")]
		[Category("Other")]
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Sets the set Layout Item.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (!IsLoaded) return;

			base.OnMouseLeftButtonDown(e);
			CaptureMouse();
			_allowDrag = false;
			if (Model != null)
			{
				Model.IsActive = true;
			}

			if (Model is LayoutDocument layoutDocument && !layoutDocument.CanMove)
			{
				return;
			}
			
			if (e.ClickCount != 1) return;
			_mouseDownPoint = e.GetPosition(this);
			_isMouseDown = true;
		}

		/// <inheritdoc/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (!IsLoaded) return;

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
			{
				StartDraggingFloatingWindowForContent();
			}
			else
			{
				var indexOfTabItemWithMouseOver = _otherTabsScreenArea.FindIndex(r => r.Contains(mousePosInScreenCoord));
				if (indexOfTabItemWithMouseOver < 0) return;
				var targetModel = _otherTabs[indexOfTabItemWithMouseOver].Content as LayoutContent;
				var container = Model.Parent as ILayoutContainer;
				var containerPane = Model.Parent as ILayoutPane;

				if (containerPane is LayoutDocumentPane layoutDocumentPane && !layoutDocumentPane.CanRepositionItems) return;
				if (containerPane?.Parent is LayoutDocumentPaneGroup layoutDocumentPaneGroup && !layoutDocumentPaneGroup.CanRepositionItems) return;

				var childrenList = container.Children.ToList();
				containerPane?.MoveChild(childrenList.IndexOf(Model), childrenList.IndexOf(targetModel));
				Model.IsActive = true;
				_parentDocumentTabPanel.UpdateLayout();
				UpdateDragDetails();
			}
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (!IsLoaded) return;

			_isMouseDown = false;
			_allowDrag = false;
			if (IsMouseCaptured) ReleaseMouseCapture();
			base.OnMouseLeftButtonUp(e);
		}

		/// <inheritdoc/>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			if (!IsLoaded) return;

			base.OnMouseLeave(e);
			_isMouseDown = false;
		}

		/// <inheritdoc/>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			if (!IsLoaded) return;

			base.OnMouseEnter(e);
			_isMouseDown = false;
		}

		/// <inheritdoc/>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (!IsLoaded) return;

			if (LayoutItem != null && e.ChangedButton == MouseButton.Middle && LayoutItem.CloseCommand.CanExecute(null))
			{
				LayoutItem.CloseCommand.Execute(null);
			}

			base.OnMouseDown(e);
		}

		private void UpdateDragDetails()
		{
			_parentDocumentTabPanel = this.FindLogicalAncestor<DocumentPaneTabPanel>();
			_parentDocumentTabPanelScreenArea = _parentDocumentTabPanel.GetScreenArea();
			// Add vertical buffer to prevent accidental floating when reordering tabs
			_parentDocumentTabPanelScreenArea.Inflate(0, _parentDocumentTabPanelScreenArea.Height / 2);
			_otherTabs = _parentDocumentTabPanel.Children.Cast<TabItem>().Where(ch => ch.Visibility != Visibility.Collapsed).ToList();
			var currentTabScreenArea = this.FindLogicalAncestor<TabItem>().GetScreenArea();
			_otherTabsScreenArea = _otherTabs.Select(ti =>
			{
				var screenArea = ti.GetScreenArea();
				return new Rect(screenArea.Left, screenArea.Top, currentTabScreenArea.Width, screenArea.Height);
			}).ToList();
		}

		/// <summary>
		/// Executes the start Dragging Floating Window For Content operation.
		/// </summary>
		private void StartDraggingFloatingWindowForContent()
		{
			ReleaseMouseCapture();
			// BD: 17.08.2020 Remove that bodge and handle CanClose=false && CanHide=true in XAML
			// if (Model is LayoutAnchorable layoutAnchorable) layoutAnchorable.ResetCanCloseInternal();
			var manager = Model.Root.Manager;
			manager.StartDraggingFloatingWindowForContent(Model);
		}
	}
}