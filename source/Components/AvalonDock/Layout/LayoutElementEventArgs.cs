/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using System;

namespace AvalonDock.Layout
{
	/// <summary>
	/// Provides an implmentation to raise an event concerning a particular <see cref="LayoutElement"/>.
	/// (eg. This <see cref="LayoutElement"/> has been removed from my childrens collection)
	/// </summary>
	public class LayoutElementEventArgs : EventArgs
	{
		/// <summary>
		/// Class constructor
		/// </summary>
		public LayoutElementEventArgs(LayoutElement element)
		{
			Element = element;
		}

		/// <summary>Gets the particular <see cref="LayoutElement"/> for which this event has been raised.</summary>
		public LayoutElement Element { get; private set; }
	}
}
