/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for adjustable size layout.
	/// </summary>
	public interface IAdjustableSizeLayout
	{
		/// <summary>
		/// Executes the adjust fixed children panel sizes operation.
		/// </summary>
		/// <param name="parentSize">The parent size.</param>
		void AdjustFixedChildrenPanelSizes(Size? parentSize = null);
	}
}