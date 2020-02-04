/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls;
using System.Windows.Data;

namespace AvalonDock.Controls
{
	/// <summary>
	/// Implements an extended <see cref="ContextMenu"/> for the <see cref="LayoutDocumentPaneControl"/>.
	/// </summary>
	public class ContextMenuEx : ContextMenu
	{
		#region Constructors

		static ContextMenuEx()
		{
		}

		public ContextMenuEx()
		{
		}

		#endregion Constructors

		#region Overrides

		protected override System.Windows.DependencyObject GetContainerForItemOverride()
		{
			return new MenuItemEx();
		}

		protected override void OnOpened(System.Windows.RoutedEventArgs e)
		{
			BindingOperations.GetBindingExpression(this, ItemsSourceProperty).UpdateTarget();

			base.OnOpened(e);
		}

		#endregion Overrides
	}
}
