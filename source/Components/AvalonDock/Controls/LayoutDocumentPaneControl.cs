/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="TabControl"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Implements the document container control with the
	/// TabItem Header (<see cref="LayoutDocumentTabItem"/>) that contains the document titles
	/// inside the <see cref="DocumentPaneTabPanel"/>.
	/// </summary>
	/// <seealso cref="TabControlEx"/>
	/// <seealso cref="ILayoutControl"/>
	public class LayoutDocumentPaneControl : TabControlEx, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields
		private LayoutDocumentPane _model;
		#endregion fields

		#region Constructors
		/// <summary>Static class constructor to register WPF style keys.</summary>
		static LayoutDocumentPaneControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutDocumentPaneControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>Class constructor from model and virtualization parameter.</summary>
		/// <param name="model"></param>
		/// <param name="isVirtualizing">Whether tabbed items are virtualized or not.</param>
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
		#endregion Constructors

		#region Properties
		/// <summary>Gets the layout model of this control.</summary>
		public ILayoutElement Model => _model;

		#endregion Properties

		#region Overrides
		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseLeftButtonDown"/> routed
		/// event is raised on this element. Implement this method to add class handling
		/// for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data.
		/// The event data reports that the left mouse button was pressed.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!e.Handled && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="System.Windows.UIElement.MouseRightButtonDown"/> routed
		/// event reaches an element in its route that is derived from this class. Implement
		/// this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains the event data. The
		/// event data reports that the right mouse button was pressed.</param>
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			if (!e.Handled && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		#endregion Overrides

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithAtcualSize.ActualWidth = ActualWidth;
			modelWithAtcualSize.ActualHeight = ActualHeight;
		}

		#endregion Private Methods
	}
}
