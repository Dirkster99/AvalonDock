/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Interface for layout orientable group.
	/// </summary>
	public interface ILayoutOrientableGroup : ILayoutGroup
	{
		/// <summary>
		/// Gets or sets the orientation.
		/// </summary>
		Orientation Orientation { get; set; }
	}
}