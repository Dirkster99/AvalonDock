using System.ComponentModel;
using System.Windows;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Document Item.
	/// </summary>
	public class LayoutDocumentItem : LayoutItem
	{
		private LayoutDocument _document;   // The content of this item

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutDocumentItem"/> class.
		/// </summary>
		internal LayoutDocumentItem()
		{
		}

		/// <summary>
		/// <see cref="Description"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(LayoutDocumentItem),
					new FrameworkPropertyMetadata(null, OnDescriptionChanged));

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		[Bindable(true)]
		[Description("Gets/sets the description to display (in the NavigatorWindow) for the document item.")]
		[Category("Other")]
		public string Description
		{
			get => (string)GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}

		/// <summary>
		/// Handles the on Description Changed.
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The event arguments.</param>
		private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentItem)d).OnDescriptionChanged(e);

		/// <summary>
		/// Handles the on Description Changed.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnDescriptionChanged(DependencyPropertyChangedEventArgs e) => _document.Description = (string)e.NewValue;

		/// <inheritdoc/>
		protected override void Close()
		{
			if (_document.Root?.Manager == null) return;
			var dockingManager = _document.Root.Manager;
			dockingManager.ExecuteCloseCommand(_document);
		}

		/// <inheritdoc/>
		protected override void OnVisibilityChanged()
		{
			if (_document?.Root != null)
			{
				_document.IsVisible = Visibility == Visibility.Visible;
				if (_document.Parent is LayoutDocumentPane layoutDocumentPane) layoutDocumentPane.ComputeVisibility();
			}

			base.OnVisibilityChanged();
		}

		/// <inheritdoc/>
		internal override void Attach(LayoutContent model)
		{
			_document = model as LayoutDocument;
			base.Attach(model);
		}

		/// <inheritdoc/>
		internal override void Detach()
		{
			_document = null;
			base.Detach();
		}

		/// <inheritdoc/>
		protected override bool CanExecuteDockAsDocumentCommand()
		{
			return LayoutElement != null && LayoutElement.FindParent<LayoutDocumentPane>() != null && LayoutElement.IsFloating;
		}
	}
}