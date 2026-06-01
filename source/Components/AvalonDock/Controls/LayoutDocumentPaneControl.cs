using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Represents the layout Document Pane Control.
	/// </summary>
	public class LayoutDocumentPaneControl : TabControlEx, ILayoutControl// , ILogicalChildrenContainer
	{
		private readonly LayoutDocumentPane _model;

		/// <summary>
		/// Initializes static members of the <see cref="LayoutDocumentPaneControl"/> class.
		/// </summary>
		static LayoutDocumentPaneControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutDocumentPaneControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LayoutDocumentPaneControl"/> class.
		/// </summary>
		/// <param name="model">The model.</param>
		/// <param name="isVirtualizing">The is Virtualizing.</param>
		internal LayoutDocumentPaneControl(LayoutDocumentPane model, bool isVirtualizing)
			: base(isVirtualizing)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			// Handle SizeChanged event instead of LayoutUpdated. It will exclude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			this.SizeChanged += OnSizeChanged;
		}

		/// <summary>
		/// Gets the model.
		/// </summary>
		[Bindable(false)]
		[Description("Gets the layout model of this control.")]
		[Category("Other")]
		public ILayoutElement Model => _model;

		/// <inheritdoc/>
		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			base.OnSelectionChanged(e);
			if (_model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		/// <inheritdoc/>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!e.Handled && _model.SelectedContent != null && !_model.SelectedContent.IsActive)
				_model.SelectedContent.IsActive = true;
		}

		/// <inheritdoc/>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			if (!e.Handled && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		/// <inheritdoc/>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);
			if (e.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (var item in e.OldItems)
				{
					if (item is LayoutContent layoutContent && layoutContent.TabItem != null)
					{
						layoutContent.TabItem.Model = null;
						layoutContent.TabItem.ContextMenu = null;
						layoutContent.TabItem.Content = null;
						var panel = layoutContent.TabItem.FindVisualAncestor<Panel>();
						if (panel != null) panel.Children.Remove(layoutContent.TabItem);
						layoutContent.TabItem = null;
					}
				}
			}
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithAtcualSize.ActualWidth = ActualWidth;
			modelWithAtcualSize.ActualHeight = ActualHeight;
		}
	}
}