using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Document Control.
	/// </summary>
	public class LayoutDocumentControl : Control
	{
		/// <summary>
		/// Initializes static members of the <see cref="LayoutDocumentControl"/> class.
		/// </summary>
		static LayoutDocumentControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentControl), new FrameworkPropertyMetadata(typeof(LayoutDocumentControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutDocumentControl), new FrameworkPropertyMetadata(true));
		}

		/// <summary>
		/// <see cref="Model"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutDocumentControl),
		  new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the model.
		/// </summary>
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
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentControl)d).OnModelChanged(e);

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

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(LayoutContent.IsEnabled)) return;
			if (Model == null) return;
			IsEnabled = Model.IsEnabled;
			if (IsEnabled || !Model.IsActive) return;
			if (Model.Parent is LayoutDocumentPane layoutDocumentPane) layoutDocumentPane.SetNextSelectedIndex();
		}

		/// <summary>
		/// The layout Item Property Key field.
		/// </summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutDocumentControl),
		  new FrameworkPropertyMetadata(null));

		/// <summary>
		/// <see cref="LayoutItem"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the layout Item.
		/// </summary>
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Sets the set Layout Item.
		/// </summary>
		/// <param name="value">The value.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		/// <inheritdoc/>
		protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			Debug.WriteLine($"{nameof(OnPreviewMouseLeftButtonUp)}: {LayoutItem.ContentId}");
			SetIsActive();
			base.OnPreviewMouseLeftButtonUp(e);
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			SetIsActive();
			base.OnMouseLeftButtonDown(e);
		}

		/// <inheritdoc/>
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			SetIsActive();
			base.OnMouseLeftButtonDown(e);
		}

		private void SetIsActive()
		{
			if (Model != null && !Model.IsActive) Model.IsActive = true;
		}
	}
}