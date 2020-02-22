/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;
using System.Windows.Controls;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <inheritdoc />
	/// <summary>Implements the inner part of tool window.</summary>
	/// <seealso cref="Control"/>
	public class LayoutAnchorableControl : Control
	{
		#region Constructors
		/// <summary>
		/// Static class constructor
		/// </summary>
		static LayoutAnchorableControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(typeof(LayoutAnchorableControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutAnchorableControl), new FrameworkPropertyMetadata(false));
		}

		/// <summary>
		/// Class constructor
		/// </summary>
		public LayoutAnchorableControl()
		{
			//SetBinding(FlowDirectionProperty, new Binding("Model.Root.Manager.FlowDirection") { Source = this });
			Unloaded += LayoutAnchorableControl_Unloaded;
		}
		#endregion Constructors

		#region Properties

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutAnchorable), typeof(LayoutAnchorableControl),
				new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>Gets or sets the <see cref="Model"/> property. This dependency property indicates the model attached to this view.</summary>
		public LayoutAnchorable Model
		{
			get => (LayoutAnchorable)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutAnchorableControl)d).OnModelChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Model"/> property.</summary>
		protected virtual void OnModelChanged(DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null) ((LayoutContent) e.OldValue).PropertyChanged -= Model_PropertyChanged;
			if (Model != null)
			{
				Model.PropertyChanged += Model_PropertyChanged;
				SetLayoutItem(Model.Root.Manager.GetLayoutItemFromModel(Model));
			}
			else
				SetLayoutItem(null);
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

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> read-only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutAnchorableControl),
				new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary> Gets the <see cref="LayoutItem"/> property. This dependency property indicates the LayoutItem attached to this tag item.</summary>
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.  
		/// This dependency property indicates the LayoutItem attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#endregion Properties

		#region Methods
		/// <inheritdoc/>
		protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
		{
			if (Model != null) 
				Model.IsActive = true;
			base.OnGotKeyboardFocus(e);
		}

		/// <summary>
		/// Executes when the element is removed from within an element tree of loaded elements.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LayoutAnchorableControl_Unloaded(object sender, RoutedEventArgs e)
		{
			// prevent memory leak via event handler
			if (Model != null)
				Model.PropertyChanged -= Model_PropertyChanged;

			Unloaded -= LayoutAnchorableControl_Unloaded;
		}
		#endregion Methods
	}
}
