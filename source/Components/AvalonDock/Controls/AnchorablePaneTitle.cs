using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the anchorable Pane Title.
	/// </summary>
	public class AnchorablePaneTitle : Control
	{
		private bool _isMouseDown = false;

		/// <summary>
		/// Initializes static members of the <see cref="AnchorablePaneTitle"/> class.
		/// </summary>
		static AnchorablePaneTitle()
		{
			IsHitTestVisibleProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(true));
			FocusableProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(false));
			DefaultStyleKeyProperty.OverrideMetadata(typeof(AnchorablePaneTitle), new FrameworkPropertyMetadata(typeof(AnchorablePaneTitle)));
		}

		/// <summary>
		/// <see cref="Model"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata(null, _OnModelChanged));

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the LayoutAnchorable model attached of this view.")]
		[Category("Anchorable")]
		public LayoutAnchorable Model
		{
			get => (LayoutAnchorable)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		private static void _OnModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((AnchorablePaneTitle)sender).OnModelChanged(e);

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (Model != null)
				SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			else
				SetLayoutItem(null);
		}

		/// <summary>
		/// The layout Item Property Key field.
		/// </summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(AnchorablePaneTitle),
				new FrameworkPropertyMetadata((LayoutItem)null));

		/// <summary>
		/// <see cref="LayoutItem"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the layout Item.
		/// </summary>
		[Bindable(true)]
		[Description("Gets the LayoutItem (LayoutAnchorableItem or LayoutDocumentItem) attached to this object.")]
		[Category("Layout")]
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Sets the set Layout Item.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		/// <inheritdoc/>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (e.LeftButton != MouseButtonState.Pressed) _isMouseDown = false;
			base.OnMouseMove(e);
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			// Start a drag & drop action for a LayoutAnchorable
			if (e.Handled || Model.CanMove == false) return;
			var attachFloatingWindow = false;
			var parentFloatingWindow = Model.FindParent<LayoutAnchorableFloatingWindow>();
			if (parentFloatingWindow != null) attachFloatingWindow = parentFloatingWindow.Descendents().OfType<LayoutAnchorablePane>().Count() == 1;

			if (attachFloatingWindow)
			{
				// the pane is hosted inside a floating window that contains only an anchorable pane so drag the floating window itself
				var floatingWndControl = Model.Root.Manager.FloatingWindows.Single(fwc => fwc.Model == parentFloatingWindow);
				floatingWndControl.AttachDrag(false);
			}
			else
			{
				_isMouseDown = true; // normal drag
			}
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_isMouseDown = false;
			base.OnMouseLeftButtonUp(e);
			if (Model != null) Model.IsActive = true; // FocusElementManager.SetFocusOnLastElement(Model);
		}
	}
}