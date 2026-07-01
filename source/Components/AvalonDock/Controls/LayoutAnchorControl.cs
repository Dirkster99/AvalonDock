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
	/// Represents the layout anchor control.
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

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorControl"/> class.
		/// </summary>
		/// <param name="model">The layout model.</param>
		internal LayoutAnchorControl(LayoutAnchorable model)
		{
			_model = model;
			_model.IsActiveChanged += new EventHandler(_model_IsActiveChanged);
			_model.IsSelectedChanged += new EventHandler(_model_IsSelectedChanged);

			SetSide(_model.FindParent<LayoutAnchorSide>().Side);
		}

		/// <summary>
		/// Gets the model.
		/// </summary>
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

		/// <summary>
		/// <see cref="Side"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty SideProperty = SidePropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the side.
		/// </summary>
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
		/// Sets the side.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetSide(AnchorSide value)
		{
			SetValue(SidePropertyKey, value);
		}

		/// <inheritdoc/>
		protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			if (!e.Handled)
			{
				_model.Root.Manager.ShowAutoHideWindow(this);
				_model.IsActive = true;
			}
		}

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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

		/// <inheritdoc/>
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