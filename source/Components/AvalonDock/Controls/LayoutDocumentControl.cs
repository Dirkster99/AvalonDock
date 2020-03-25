/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements the content part of the document control.
	/// It hosts a <see cref="LayoutDocument"/> as its <see cref="Model"/>.
	/// </summary>
	public class LayoutDocumentControl : Control
	{
		#region Constructors
		/// <summary>
		/// Static class constructor
		/// </summary>
		static LayoutDocumentControl()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentControl), new FrameworkPropertyMetadata(typeof(LayoutDocumentControl)));
			FocusableProperty.OverrideMetadata(typeof(LayoutDocumentControl), new FrameworkPropertyMetadata(true));
		}

		#endregion Constructors

		#region Properties

		#region Model

		/// <summary><see cref="Model"/> dependency property.</summary>
		public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(LayoutContent), typeof(LayoutDocumentControl),
		  new FrameworkPropertyMetadata(null, OnModelChanged));

		/// <summary>
		/// Gets or sets the <see cref="Model"/> property.
		/// This dependency property indicates the model attached to this view.
		/// </summary>
		public LayoutContent Model
		{
			get => (LayoutContent)GetValue(ModelProperty);
			set => SetValue(ModelProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Model"/> property.</summary>
		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentControl)d).OnModelChanged(e);

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

		private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(LayoutContent.IsEnabled)) return;
			if (Model == null) return;
			IsEnabled = Model.IsEnabled;
			if (IsEnabled || !Model.IsActive) return;
			if (Model.Parent is LayoutDocumentPane layoutDocumentPane) layoutDocumentPane.SetNextSelectedIndex();
		}

		#endregion Model

		#region LayoutItem

		/// <summary><see cref="LayoutItem"/> Read-Only dependency property.</summary>
		private static readonly DependencyPropertyKey LayoutItemPropertyKey = DependencyProperty.RegisterReadOnly(nameof(LayoutItem), typeof(LayoutItem), typeof(LayoutDocumentControl),
		  new FrameworkPropertyMetadata(null));

		public static readonly DependencyProperty LayoutItemProperty = LayoutItemPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the <see cref="LayoutItem"/> property. This dependency property 
		/// indicates the LayoutItem attached to this tag item.
		/// </summary>
		public LayoutItem LayoutItem => (LayoutItem)GetValue(LayoutItemProperty);

		/// <summary>
		/// Provides a secure method for setting the <see cref="LayoutItem"/> property.  
		/// This dependency property indicates the LayoutItem attached to this tag item.
		/// </summary>
		/// <param name="value">The new value for the property.</param>
		protected void SetLayoutItem(LayoutItem value) => SetValue(LayoutItemPropertyKey, value);

		#endregion LayoutItem

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			Debug.WriteLine("OnPreviewGotKeyboardFocus: " + LayoutItem.ContentId);
			SetIsActive();
			base.OnPreviewGotKeyboardFocus(e);
		}

		/// <inheritdoc />
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			SetIsActive();
			base.OnMouseLeftButtonDown(e);
		}

		/// <inheritdoc />
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			SetIsActive();
			base.OnMouseLeftButtonDown(e);
		}
		
		#endregion Overrides

		#region Private Methods

		private void SetIsActive()
		{
			if (Model != null) Model.IsActive = true;
		}

		#endregion Private Methods
	}
}
