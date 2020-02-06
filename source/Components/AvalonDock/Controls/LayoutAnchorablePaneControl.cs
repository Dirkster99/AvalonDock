/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <inheritdoc cref="TabControl"/>
	/// <inheritdoc cref="ILayoutControl"/>
	/// <summary>
	/// Provides a control to display multiple (or just one) <see cref="LayoutAnchorable"/>(s).
	/// See also <see cref="AnchorablePaneTabPanel"/>.
	/// </summary>
	/// <seealso cref="TabControl"/>
	/// <seealso cref="ILayoutControl"/>
	/// <seealso cref="AnchorablePaneTabPanel"/>.
	public class LayoutAnchorablePaneControl : TabControl, ILayoutControl//, ILogicalChildrenContainer
	{
		#region fields
		private readonly LayoutAnchorablePane _model;
		#endregion fields

		#region Constructors

		static LayoutAnchorablePaneControl()
		{
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorablePaneControl), new FrameworkPropertyMetadata(false));
		}

		public LayoutAnchorablePaneControl(LayoutAnchorablePane model)
		{
			_model = model ?? throw new ArgumentNullException(nameof(model));
			SetBinding(ItemsSourceProperty, new Binding("Model.Children") { Source = this });
			SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			// Handle SizeChanged event instead of LayoutUpdated. It will exlude fluctuations of Actual size values.
			// this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
			this.SizeChanged += OnSizeChanged;
		}

		#endregion

		#region Properties

		public ILayoutElement Model => _model;

		#endregion

		#region Overrides

		/// <inheritdoc />
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (_model != null && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			if (!e.Handled && _model != null && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		/// <inheritdoc />
		protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);
			if (!e.Handled && _model != null && _model.SelectedContent != null)
				_model.SelectedContent.IsActive = true;
		}

		#endregion

		#region Private Methods

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			var modelWithActualSize = _model as ILayoutPositionableElementWithActualSize;
			modelWithActualSize.ActualWidth = ActualWidth;
			modelWithActualSize.ActualHeight = ActualHeight;
		}

		#endregion
	}
}
