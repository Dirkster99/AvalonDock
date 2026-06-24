using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Anchorable Pane Control.
	/// </summary>
	public class LayoutAnchorablePaneControl : TabControlEx, ILayoutControl// , ILogicalChildrenContainer
	{
		private readonly LayoutAnchorablePane _model;

		/// <summary>
		/// Initializes static members of the <see cref="LayoutAnchorablePaneControl"/> class.
		/// </summary>
		static LayoutAnchorablePaneControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorablePaneControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorablePaneControl"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <param name="IsVirtualizing">The is Virtualizing.</param>
		internal LayoutAnchorablePaneControl(LayoutAnchorablePane model, bool IsVirtualizing)
			: base(IsVirtualizing)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			SizeChanged += OnSizeChanged;
		}

		/// <summary>
		/// Gets the model.
		/// </summary>
		[Bindable(false)]
		[Description("Gets the layout model of this control.")]
		[Category("Other")]
		public ILayoutElement Model => _model;

		/// <inheritdoc/>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (_model?.SelectedContent != null) _model.SelectedContent.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!e.Handled && _model?.SelectedContent != null) _model.SelectedContent.IsActive = true;
		}

		/// <inheritdoc/>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			if (!e.Handled && _model?.SelectedContent != null) _model.SelectedContent.IsActive = true;
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithActualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithActualSize.ActualWidth = ActualWidth;
			modelWithActualSize.ActualHeight = ActualHeight;
		}
	}
}