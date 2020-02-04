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
	/// <summary>
	/// Implements the document container control with the
	/// TabItem Header (<see cref="LayoutDocumentTabItem"/>) that contains the document titles
	/// inside the <see cref="DocumentPaneTabPanel"/>.
	/// </summary>
	public class LayoutDocumentPaneControl : TabControl, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields
		private LayoutDocumentPane _model;
		#endregion fields

		#region Constructors
		/// <summary>
		/// Static class constructor to register WPF style keys.
		/// </summary>
		static LayoutDocumentPaneControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutDocumentPaneControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Class constructor from model parameter.
		/// </summary>
		/// <param name="model"></param>
		internal LayoutDocumentPaneControl(LayoutDocumentPane model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			_model = model;
			SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });

			// Handle SizeChanged event instead of LayoutUpdated. It will exlude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			this.SizeChanged += OnSizeChanged;
		}
		#endregion Constructors

		#region Properties

		public ILayoutElement Model
		{
			get
			{
				return _model;
			}
		}

		#endregion Properties

		#region Overrides

		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			if (!e.Handled && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

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
