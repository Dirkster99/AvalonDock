using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Anchorable Control.
	/// </summary>
	public class LayoutAnchorableControl : Control
	{
		/// <summary>
		/// Initializes static members of the <see cref="LayoutAnchorableControl"/> class.
		/// </summary>
		static LayoutAnchorableControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorableControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutAnchorableControl"/> class.
		/// </summary>
		public LayoutAnchorableControl()
		{
			// SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			Unloaded += LayoutAnchorableControl_Unloaded;
		}

		/// <summary>
		/// <see cref="Model"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(LayoutAnchorableControl),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the model attached to this view.")]
		[Category("Other")]
		public LayoutAnchorable Model
		{
			get => (LayoutAnchorable)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableControl)d).OnModelChanged(e);

		/// <summary>
		/// Handles the on Model Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null) ((LayoutContent)e.OldValue).PropertyChanged -= Model_PropertyChanged;
			if (Model != null)
			{
				Model.PropertyChanged += Model_PropertyChanged;
				SetLayoutItem(Model?.Root?.Manager?.GetLayoutItemFromModel(Model));
			}
			else
			{
				SetLayoutItem(null);
			}
		}

		private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(LayoutAnchorable.IsEnabled)) return;
			if (Model == null) return;
			IsEnabled = Model.IsEnabled;
			if (IsEnabled || !Model.IsActive) return;
			if (Model.Parent != null && Model.Parent is LayoutAnchorablePane layoutAnchorablePane)
				layoutAnchorablePane.SetNextSelectedIndex();
		}

		/// <summary>
		/// The layout Item Property Key field.
		/// </summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableControl),
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

		/// <inheritdoc/>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (Model != null)
				Model.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		/// <summary>
		/// Executes the layout Anchorable Control Unloaded operation.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The event arguments.</param>
		private void LayoutAnchorableControl_Unloaded(object sender, RoutedEventArgs e)
		{
			// prevent memory leak via event handler
			if (Model != null)
				Model.PropertyChanged -= Model_PropertyChanged;

			Unloaded -= LayoutAnchorableControl_Unloaded;
		}
	}
}