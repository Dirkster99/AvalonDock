/************************************************************************
   AvalonDock

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at https://opensource.org/licenses/MS-PL
 ************************************************************************/

using AvalonDock.Layout;
using System;

namespace AvalonDock
{
	/// <summary>
	/// Implements an event that can be raised to inform the client application about an
	/// anchorable that been hidden.
	/// </summary>
	public class AnchorableHiddenEventArgs : EventArgs
	{
		/// <summary>
		/// Class constructor from the anchorables layout model.
		/// </summary>
		/// <param name="document"></param>
		public AnchorableHiddenEventArgs(LayoutAnchorable anchorable)
		{
			Anchorable = anchorable;
		}

		/// <summary>
		/// Gets the model of the anchorable that has been hidden.
		/// </summary>
		public LayoutAnchorable Anchorable { get; private set; }
	}
}