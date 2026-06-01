using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Anchorable Tab Item.
	/// </summary>
	public class LayoutAnchorableTabItem : Control
	{
		private bool _isMouseDown = false;
		private static LayoutAnchorableTabItem _draggingItem = null;
		private static bool _cancelMouseLeave = false;

		/// <summary>
		/// Initializes static members of the <see cref="LayoutAnchorableTabItem"/> class.
		/// </summary>
		static LayoutAnchorableTabItem()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableTabItem), new FrameworkPropertyMetadata(typeof(LayoutAnchorableTabItem)));
		}

		/// <summary>
		/// <see cref="Model"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutAnchorableTabItem),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the model attached to the anchorable tab item.")]
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
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableTabItem)d).OnModelChanged(e);

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			SetLayoutItem(Model?.Root.Manager.GetLayoutItemFromModel(Model));
			// UpdateLogicalParent();
		}

		/// <summary>
		/// The layout Item Property Key field.
		/// </summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableTabItem),
				new FrameworkPropertyMetadata(null));

		/// <summary>
		/// <see cref="LayoutItem"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the layout Item.
		/// </summary>
		[Bindable(true)]
		[Description("Gets the the LayoutItem attached to this tag item.")]
		[Category("Other")]
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Sets the set Layout Item.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		/// <summary>
		/// Executes the is Dragging Item operation.
		/// </summary>
		/// <returns>true if the operation succeeds; otherwise, false.</returns>
		internal static bool IsDraggingItem() => _draggingItem != null;

		/// <summary>
		/// Gets the get Dragging Item.
		/// </summary>
		/// <returns>The requested value.</returns>
		internal static LayoutAnchorableTabItem GetDraggingItem() => _draggingItem;

		/// <summary>
		/// Executes the reset Dragging Item operation.
		/// </summary>
		internal static void ResetDraggingItem() => _draggingItem = null;

		/// <summary>
		/// Executes the cancel Mouse Leave operation.
		/// </summary>
		internal static void CancelMouseLeave() => _cancelMouseLeave = true;

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			// Start a drag & drop action for a LayoutAnchorable
			var anchorAble = this.Model as LayoutAnchorable;
			if (anchorAble != null)
			{
				if (anchorAble.CanMove == false) return;
			}

			_isMouseDown = true;
			_draggingItem = this;
		}

		/// <inheritdoc/>
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (e.LeftButton != MouseButtonState.Pressed)
			{
				_isMouseDown = false;
				_draggingItem = null;
			}
			else
			{
				_cancelMouseLeave = false;
			}
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			base.OnMouseLeftButtonUp(e);
			Model.IsActive = true;
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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
	}
}