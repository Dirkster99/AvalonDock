/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout content selector.
	/// </summary>
	public interface ILayoutContentSelector
	{
		/// <summary>
		/// Gets or sets the selected content index.
		/// </summary>
		int SelectedContentIndex { get; set; }

		/// <summary>
		/// Gets the selected content.
		/// </summary>
		LayoutContent SelectedContent { get; }

		/// <summary>
		/// Executes the index of operation.
		/// </summary>
		/// <param name="content">The layout content.</param>
		/// <returns>The resulting value.</returns>
		int IndexOf(LayoutContent content);
	}
}