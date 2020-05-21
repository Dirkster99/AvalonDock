/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System.ComponentModel;

namespace AvalonDock.Layout
{
	/// <summary>This interface should be implemented by a classe that supports
	/// - Manipulation of the children of a given parent <see cref="LayoutContainer"/> or
	/// - Manipulation of the children of the <see cref="LayoutRoot"/>.
	/// </summary>
	public interface ILayoutElement : INotifyPropertyChanged, INotifyPropertyChanging
	{
		/// <summary>Gets the parent <see cref="LayoutContainer"/> for this layout element.</summary>
		ILayoutContainer Parent { get; }

		/// <summary>Gets the <see cref="LayoutRoot"/> for this layout element.</summary>
		ILayoutRoot Root { get; }
	}
}
