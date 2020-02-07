/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System.Windows;

namespace AvalonDock.Controls
{
	public class LayoutDocumentItem : LayoutItem
	{
		#region fields
		private LayoutDocument _document;
		#endregion fields

		#region Constructors

		internal LayoutDocumentItem()
		{
		}

		#endregion Constructors

		#region Properties

		#region Description

		/// <summary><see cref="Description"/> dependency property.</summary>
		public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(LayoutDocumentItem),
					new FrameworkPropertyMetadata(null, OnDescriptionChanged));

		/// <summary>
		/// Gets or sets the <see cref="Description"/> property.  This dependency property 
		/// indicates the description to display for the document item.
		/// </summary>
		public string Description
		{
			get => (string)GetValue(DescriptionProperty);
			set => SetValue(DescriptionProperty, value);
		}

		/// <summary>Handles changes to the <see cref="Description"/> property.</summary>
		private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((LayoutDocumentItem)d).OnDescriptionChanged(e);

		/// <summary>Provides derived classes an opportunity to handle changes to the <see cref="Description"/> property.</summary>
		protected virtual void OnDescriptionChanged(DependencyPropertyChangedEventArgs e) => _document.Description = (string)e.NewValue;

		#endregion Description

		#endregion Properties

		#region Overrides

		/// <inheritdoc />
		protected override void Close()
		{
			if (_document.Root?.Manager == null) return;
			var dockingManager = _document.Root.Manager;
			dockingManager._ExecuteCloseCommand(_document);
		}

		/// <inheritdoc />
		protected override void OnVisibilityChanged()
		{
			if (_document?.Root != null)
			{
				_document.IsVisible = Visibility == Visibility.Visible;
				if (_document.Parent is LayoutDocumentPane layoutDocumentPane) layoutDocumentPane.ComputeVisibility();
			}
			base.OnVisibilityChanged();
		}

		/// <inheritdoc />
		internal override void Attach(LayoutContent model)
		{
			_document = model as LayoutDocument;
			base.Attach(model);
		}

		/// <inheritdoc />
		internal override void Detach()
		{
			_document = null;
			base.Detach();
		}

		#endregion Overrides
	}
}
