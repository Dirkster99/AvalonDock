/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.Windows.Controls;

namespace AvalonDock.Layout
{
	/// <summary>Interface definition for a <see cref="ILayoutGroup"/> that supports a <see cref="System.Windows.Controls.Orientation"/> property.</summary>
	public interface ILayoutOrientableGroup : ILayoutGroup
	{
		/// <summary>Gets/sets the <see cref="System.Windows.Controls.Orientation"/> of the <see cref="ILayoutGroup"/>.</summary>
		Orientation Orientation { get; set; }
	}
}
